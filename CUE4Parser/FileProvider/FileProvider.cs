using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.IO;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Localization;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Pak;
using CUE4Parse.UE4.Pak.Objects;
using CUE4Parse.UE4.Plugins;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Vfs;
using CUE4Parse.Utils;
using Ionic.Zip;
using Newtonsoft.Json;
using Serilog;

namespace CUE4Parse.FileProvider
{
    public static class FileProvider
    {
        /// <summary>
        /// Logger that shows whats happening in the program
        /// </summary>
        public static ILogger Logger = Log.Logger;

        /// <summary>
        /// Directory to .pak and .utoc files
        /// </summary>
        public static DirectoryInfo WorkingDirectory;

        /// <summary>
        /// Name of the loaded game
        /// </summary>
        public static string GameName;

        public static Dictionary<string, UObject> UObjectCache { get; } = new Dictionary<string, UObject>();

        public static VersionContainer Versions { get; set; }

        public static ITypeMappingsProvider? MappingsContainer { get; set; }

        public static TypeMappings? MappingsForThisGame => MappingsContainer?.ForGame(GameName.ToLowerInvariant());

        public static IDictionary<string, IDictionary<string, string>> LocalizedResources { get; } = new Dictionary<string, IDictionary<string, string>>();

        public static Dictionary<string, string> VirtualPaths { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// All loaded files in game - Is case senseitive
        /// Always has '/' slashes
        /// </summary>
        public static FileProviderDictionary Files { get; } = new FileProviderDictionary(false);

        public static IReadOnlyDictionary<FPackageId, GameFile> FilesById { get; } = Files.byId;

        public static bool IsCaseInsensitive { get; } = true;

        public static readonly ConcurrentDictionary<IAesVfsReader, object?> UnloadedVfs = new();

        public static IReadOnlyCollection<IAesVfsReader> UnloadedVfsKeys => (IReadOnlyCollection<IAesVfsReader>)UnloadedVfs.Keys;

        public static List<IAesVfsReader> MountedVfs { get; } = new List<IAesVfsReader>();

        public static readonly ConcurrentDictionary<FGuid, FAesKey> AesKeys = new();

        static List<FGuid> MissingKeys { get; } = new List<FGuid>();

        public static IoGlobalData? GlobalData { get; set; }

        /// <summary>
        /// Returns a <see cref="IEnumerable{string}"/> containing all names of files in given path
        /// </summary>
        /// <remarks>
        /// If the name contains a '.' it is an asset.
        /// <br/>
        /// If not then it is a folder.
        /// </remarks>
        /// <param name="GivenPath">The path to search in</param>
        /// <returns>Names of files in given path</returns>
        public static IEnumerable<string> GetFilesInPath(string GivenPath)
        {
            // Makes sure the path ends with '/'
            string Path = GivenPath.EndsWith('/') ? GivenPath : GivenPath + '/';
            // Gets the number of slashes in path
            int BaseNum = Path.NumOccurrences('/');
            List<string> Result = new List<string>();
            foreach (var TestPath in Files.Keys)
                // If the TestPath is a subpath or asset of the of the GivenPath.
                if (TestPath.StartsWith(Path))
                {
                    int ItemNum = TestPath.NumOccurrences('/') - 1;
                    // Makes sure it isn't folder in folder in folder
                    if (ItemNum <= BaseNum)
                    {
                        string FinalName = (ItemNum == BaseNum ? TestPath.SubstringBeforeLast('/') : TestPath).SubstringAfterLast('/');
                        if (Result.IndexOf(FinalName) == -1)
                            Result.Add(FinalName);
                    }
                }
            return Result;
        }

        /// <summary>
        /// Loads game given directory of pak files
        /// </summary>
        /// <param name="directory">Location of pak files on disk</param>
        /// <param name="versions">Version to load</param>
        public static void Initialize(DirectoryInfo directory, VersionContainer? versions = null)
        {
            WorkingDirectory = directory;
            Versions = versions ?? VersionContainer.Default;

            var availableFiles = new List<Dictionary<string, GameFile>> { IterateFiles(WorkingDirectory, SearchOption.TopDirectoryOnly) };
            
            foreach (var osFiles in availableFiles)
            {
                Files.AddFiles(osFiles);
            }
        }

        private static void RegisterFile(string file, Stream[] stream = null!)
        {
            var ext = file.SubstringAfterLast('.');
            if (ext.Equals("pak", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var reader = new PakFileReader(file, stream[0], Versions) { IsConcurrent = true };
                    if (reader.IsEncrypted && !MissingKeys.Contains(reader.Info.EncryptionKeyGuid))
                        MissingKeys.Add(reader.Info.EncryptionKeyGuid);
                    UnloadedVfs[reader] = null;
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }
            else if (ext.Equals("utoc", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var reader = new IoStoreReader(file, stream[0], stream[1], EIoStoreTocReadOptions.ReadDirectoryIndex, Versions) { IsConcurrent = true };
                    if (reader.IsEncrypted && !MissingKeys.Contains(reader.Info.EncryptionKeyGuid))
                        MissingKeys.Add(reader.Info.EncryptionKeyGuid);
                    UnloadedVfs[reader] = null;
                }
                catch (Exception e)
                {
                    Logger.Warning(e.ToString());
                }
            }
        }

        private static void RegisterFile(FileInfo file)
        {
            var ext = file.FullName.SubstringAfterLast('.');
            if (ext.Equals("pak", StringComparison.OrdinalIgnoreCase))
            {
                RegisterFile(file.FullName, new Stream[1] { file.OpenRead() });
            }
            else if (ext.Equals("utoc", StringComparison.OrdinalIgnoreCase))
            {
                RegisterFile(file.FullName, new Stream[2] { file.OpenRead(), File.OpenRead(file.FullName.SubstringBeforeLast('.') + ".ucas") });
            }
            else if (ext.Equals("apk", StringComparison.OrdinalIgnoreCase))
            {
                var zipfile = new ZipFile(file.FullName);
                MemoryStream pngstream = new();
                foreach (var entry in zipfile.Entries)
                {
                    if (!entry.FileName.EndsWith("main.obb.png", StringComparison.OrdinalIgnoreCase))
                        continue;
                    entry.Extract(pngstream);
                    pngstream.Seek(0, SeekOrigin.Begin);

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var container = ZipFile.Read(pngstream);

                    foreach (var fileentry in container.Entries)
                    {
                        var streams = new Stream[2];
                        if (fileentry.FileName.EndsWith(".pak"))
                        {
                            try
                            {
                                streams[0] = new MemoryStream();
                                fileentry.Extract(streams[0]);
                                streams[0].Seek(0, SeekOrigin.Begin);
                            }
                            catch (Exception e)
                            {
                                Logger.Warning(e.ToString());
                            }
                        }
                        else if (fileentry.FileName.EndsWith(".utoc"))
                        {
                            try
                            {
                                streams[0] = new MemoryStream();
                                fileentry.Extract(streams[0]);
                                streams[0].Seek(0, SeekOrigin.Begin);

                                foreach (var ucas in container.Entries) // look for ucas file
                                {
                                    if (ucas.FileName.Equals(fileentry.FileName.SubstringBeforeLast('.') + ".ucas"))
                                    {
                                        streams[1] = new MemoryStream();
                                        ucas.Extract(streams[1]);
                                        streams[1].Seek(0, SeekOrigin.Begin);
                                        break;
                                    }
                                }
                                if (streams[1] == null)
                                    continue; // ucas file not found
                            }
                            catch (Exception e)
                            {
                                Logger.Warning(e.ToString());
                            }
                        }
                        else
                        {
                            continue;
                        }
                        RegisterFile(fileentry.FileName, streams);
                    }
                }
            }
        }

        private static Dictionary<string, GameFile> IterateFiles(DirectoryInfo directory, SearchOption option)
        {
            string mountPoint;
            mountPoint = directory.Name + '/';

            var osFiles = new Dictionary<string, GameFile>();

            // In .uproject mode, we must recursively look for files
            option = SearchOption.TopDirectoryOnly;

            foreach (var file in directory.EnumerateFiles("*.*", option))
            {
                var ext = file.Extension.SubstringAfter('.');
                if (!file.Exists || string.IsNullOrEmpty(ext)) continue;

                RegisterFile(file);

                // Register local file only if it has a known extension, we don't need every file
                if (!GameFile.Ue4KnownExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) continue;

                var osFile = new OsGameFile(WorkingDirectory, file, mountPoint, Versions);
                osFiles[osFile.Path] = osFile;
            }

            return osFiles;
        }

        public static void LoadMappings()
        {
            if (GameName.Equals("FortniteGame", StringComparison.OrdinalIgnoreCase))
            {
                MappingsContainer = new BenBotMappingsProvider("fortnitegame");
            }
        }

        public static IEnumerable<IAesVfsReader> UnloadedVfsByGuid(FGuid guid) => UnloadedVfs.Keys.Where(it => it.EncryptionKeyGuid == guid);
        public static void UnloadAllVfs()
        {
            foreach (var reader in MountedVfs)
            {
                AesKeys.TryRemove(reader.EncryptionKeyGuid, out _);
                MissingKeys.Add(reader.EncryptionKeyGuid);
                MountedVfs.Remove(reader);
                UnloadedVfs[reader] = null;
            }
        }


        public static int Mount() => MountAsync().Result;
        public static async Task<int> MountAsync()
        {
            var countNewMounts = 0;
            var tasks = new LinkedList<Task>();
            foreach (var reader in UnloadedVfs.Keys)
            {
                if (GlobalData == null && reader is IoStoreReader ioReader && reader.Name.Equals("global.utoc", StringComparison.OrdinalIgnoreCase))
                {
                    GlobalData = new IoGlobalData(ioReader);
                }

                if (reader.IsEncrypted || !reader.HasDirectoryIndex)
                    continue;

                tasks.AddLast(Task.Run(() =>
                {
                    try
                    {
                        reader.MountTo(Files, IsCaseInsensitive);
                        UnloadedVfs.TryRemove(reader, out _);
                        MountedVfs.Add(reader);
                        Interlocked.Increment(ref countNewMounts);
                        return reader;
                    }
                    catch (InvalidAesKeyException)
                    {
                        // Ignore this 
                    }
                    catch (Exception e)
                    {
                        Logger.Warning(e, $"Uncaught exception while loading file {reader.Path.SubstringAfterLast('/')}");
                    }
                    return null;
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return countNewMounts;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubmitKey(FGuid guid, FAesKey key)
        {
            List<IAesVfsReader> completed = new List<IAesVfsReader>();
            var countNewMounts = 0;
            var tasks = new LinkedList<Task<IAesVfsReader?>>();
            foreach (var reader in UnloadedVfsByGuid(guid))
            {
                if (GlobalData == null && reader is IoStoreReader ioReader && reader.Name.Equals("global.utoc", StringComparison.OrdinalIgnoreCase))
                {
                    GlobalData = new IoGlobalData(ioReader);
                }

                if (!reader.HasDirectoryIndex)
                    continue;

                completed.Add(Something());

                IAesVfsReader Something()
                {
                    try
                    {
                        reader.MountTo(Files, key);
                        UnloadedVfs.TryRemove(reader, out _);
                        MountedVfs.Add(reader);
                        Interlocked.Increment(ref countNewMounts);
                        return reader;
                    }
                    catch (InvalidAesKeyException)
                    {
                        // Ignore this 
                    }
                    catch (Exception e)
                    {
                        Logger.Warning(e, $"Uncaught exception while loading pak file {reader.Path.SubstringAfterLast('/')}");
                    }
                    return null;
                }
            }

            foreach (var it in completed)
            {
                var mkey = it?.AesKey;
                if (it == null || mkey == null) continue;
                MissingKeys.Remove(it.EncryptionKeyGuid);
                AesKeys.TryAdd(it.EncryptionKeyGuid, mkey);
            }

            GameName = (Files.Keys.FirstOrDefault(it => !it.SubstringBefore('/').EndsWith("engine", StringComparison.OrdinalIgnoreCase)) ?? string.Empty).SubstringBefore('/');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubmitKeys(IEnumerable<KeyValuePair<FGuid, FAesKey>> keys) // => SubmitKeysAsync(keys).Result;
        {
            var countNewMounts = 0;

            foreach (var item in keys)
                SubmitKey(item.Key, item.Value);
        }

        public static int LoadLocalization(ELanguage language = ELanguage.English, CancellationToken cancellationToken = default)
        {
            var regex = new Regex($"^{GameName}/.+/{GetLanguageCode(language)}/.+.locres$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            LocalizedResources.Clear();

            var i = 0;
            foreach (var file in Files.Where(x => regex.IsMatch(x.Key)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!file.Value.TryCreateReader(out var archive)) continue;

                var locres = new FTextLocalizationResource(archive);
                foreach (var entries in locres.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!LocalizedResources.ContainsKey(entries.Key.Str))
                        LocalizedResources[entries.Key.Str] = new Dictionary<string, string>();

                    foreach (var keyValue in entries.Value)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        LocalizedResources[entries.Key.Str][keyValue.Key.Str] = keyValue.Value.LocalizedString;
                        i++;
                    }
                }
            }
            return i;
        }

        public static string GetLocalizedString(string namespacee, string key, string? defaultValue)
        {
            if (LocalizedResources.TryGetValue(namespacee, out var keyValue) &&
                keyValue.TryGetValue(key, out var localizedResource))
                return localizedResource;

            return defaultValue ?? string.Empty;
        }

        public static string GetLanguageCode(ELanguage language)
        {
            return GameName.ToLowerInvariant() switch
            {
                "fortnitegame" => language switch
                {
                    ELanguage.English => "en",
                    ELanguage.French => "fr",
                    ELanguage.German => "de",
                    ELanguage.Italian => "it",
                    ELanguage.Spanish => "es",
                    ELanguage.SpanishLatin => "es-419",
                    ELanguage.Arabic => "ar",
                    ELanguage.Japanese => "ja",
                    ELanguage.Korean => "ko",
                    ELanguage.Polish => "pl",
                    ELanguage.PortugueseBrazil => "pt-BR",
                    ELanguage.Russian => "ru",
                    ELanguage.Turkish => "tr",
                    ELanguage.Chinese => "zh-CN",
                    ELanguage.TraditionalChinese => "zh-Hant",
                    _ => "en"
                },
                "worldexplorers" => language switch
                {
                    ELanguage.English => "en",
                    ELanguage.French => "fr",
                    ELanguage.German => "de",
                    ELanguage.Italian => "it",
                    ELanguage.Spanish => "es",
                    ELanguage.Japanese => "ja",
                    ELanguage.Korean => "ko",
                    ELanguage.PortugueseBrazil => "pt-BR",
                    ELanguage.Russian => "ru",
                    ELanguage.Chinese => "zh-Hans",
                    _ => "en"
                },
                "shootergame" => language switch
                {
                    ELanguage.English => "en-US",
                    ELanguage.French => "fr-FR",
                    ELanguage.German => "de-DE",
                    ELanguage.Italian => "it-IT",
                    ELanguage.Spanish => "es-ES",
                    ELanguage.SpanishMexico => "es-MX",
                    ELanguage.Arabic => "ar-AE",
                    ELanguage.Japanese => "ja-JP",
                    ELanguage.Korean => "ko-KR",
                    ELanguage.Polish => "pl-PL",
                    ELanguage.PortugueseBrazil => "pt-BR",
                    ELanguage.Russian => "ru-RU",
                    ELanguage.Turkish => "tr-TR",
                    ELanguage.Chinese => "zh-CN",
                    ELanguage.TraditionalChinese => "zh-TW",
                    ELanguage.Indonesian => "id-ID",
                    ELanguage.Thai => "th-TH",
                    ELanguage.VietnameseVietnam => "vi-VN",
                    _ => "en-US"
                },
                "stateofdecay2" => language switch
                {
                    ELanguage.English => "en-US",
                    ELanguage.AustralianEnglish => "en-AU",
                    ELanguage.French => "fr-FR",
                    ELanguage.German => "de-DE",
                    ELanguage.Italian => "it-IT",
                    ELanguage.SpanishMexico => "es-MX",
                    ELanguage.PortugueseBrazil => "pt-BR",
                    ELanguage.Russian => "ru-RU",
                    ELanguage.Chinese => "zh-CN",
                    _ => "en-US"
                },
                "oakgame" => language switch
                {
                    ELanguage.English => "en",
                    ELanguage.French => "fr",
                    ELanguage.German => "de",
                    ELanguage.Italian => "it",
                    ELanguage.Spanish => "es",
                    ELanguage.Japanese => "ja",
                    ELanguage.Korean => "ko",
                    ELanguage.PortugueseBrazil => "pt-BR",
                    ELanguage.Russian => "ru",
                    ELanguage.Chinese => "zh-Hans-CN",
                    ELanguage.TraditionalChinese => "zh-Hant-TW",
                    _ => "en"
                },
                _ => "en"
            };
        }

        public static int LoadVirtualPaths() { return LoadVirtualPaths(Versions.Ver); }

        public static int LoadVirtualPaths(FPackageFileVersion version, CancellationToken cancellationToken = default)
        {
            var regex = new Regex($"^{GameName}/Plugins/.+.upluginmanifest$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            VirtualPaths.Clear();

            var i = 0;
            foreach (var (filePath, gameFile) in Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (version < EUnrealEngineObjectUE4Version.ADDED_SOFT_OBJECT_PATH) // < 4.18
                {
                    if (!filePath.EndsWith(".uplugin")) continue;
                    if (!TryCreateReader(gameFile.Path, out var stream)) continue;
                    using var reader = new StreamReader(stream);
                    var pluginFile = JsonConvert.DeserializeObject<UPluginDescriptor>(reader.ReadToEnd());
                    if (!pluginFile!.CanContainContent) continue;
                    var virtPath = gameFile.Path.SubstringAfterLast('/').SubstringBeforeLast('.');
                    var path = gameFile.Path.SubstringBeforeLast('/');

                    if (!VirtualPaths.ContainsKey(virtPath))
                    {
                        VirtualPaths.Add(virtPath, path);
                        i++; // Only increment if we don't have the path already
                    }
                    else
                    {
                        VirtualPaths[virtPath] = path;
                    }
                }
                else
                {
                    if (!regex.IsMatch(filePath)) continue;
                    if (!TryCreateReader(gameFile.Path, out var stream)) continue;
                    using var reader = new StreamReader(stream);
                    var manifest = JsonConvert.DeserializeObject<UPluginManifest>(reader.ReadToEnd());

                    foreach (var content in manifest!.Contents)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!content.Descriptor.CanContainContent) continue;
                        var virtPath = content.File.SubstringAfterLast('/').SubstringBeforeLast('.');
                        var path = content.File.Replace("../../../", string.Empty).SubstringBeforeLast('/');

                        if (!VirtualPaths.ContainsKey(virtPath))
                        {
                            VirtualPaths.Add(virtPath, path);
                            i++; // Only increment if we don't have the path already
                        }
                        else
                        {
                            VirtualPaths[virtPath] = path;
                        }
                    }
                }
            }

            return i;
        }

        public static bool TryFindGameFile(string path, out GameFile file)
        {
            var uassetPath = FixPath(path);
            if (Files.TryGetValue(uassetPath, out file))
            {
                return true;
            }

            var umapPath = uassetPath.SubstringBeforeWithLast('.') + GameFile.Ue4PackageExtensions[1];
            if (Files.TryGetValue(umapPath, out file))
            {
                return true;
            }

            return Files.TryGetValue(path, out file);
        }

        public static string FixPath(string path) => FixPath(path, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        public static string FixPath(string path, StringComparison comparisonType)
        {
            path = path.Replace('\\', '/');
            if (path[0] == '/')
                path = path[1..];
            var lastPart = path.SubstringAfterLast('/');
            // This part is only for FSoftObjectPaths and not really needed anymore internally, but it's still in here for user input
            if (lastPart.Contains('.') && lastPart.SubstringBefore('.') == lastPart.SubstringAfter('.'))
                path = string.Concat(path.SubstringBeforeLast('/'), "/", lastPart.SubstringBefore('.'));
            if (path[^1] != '/' && !lastPart.Contains('.'))
                path += "." + GameFile.Ue4PackageExtensions[0];

            var root = path.SubstringBefore('/');
            if (root.Equals(GameName, StringComparison.OrdinalIgnoreCase))
            {
                return comparisonType == StringComparison.OrdinalIgnoreCase ? path.ToLowerInvariant() : path;
            }

            if (root.Equals("Game", comparisonType) || root.Equals("Engine", comparisonType))
            {
                var gameName = root.Equals("Engine", comparisonType) ? "Engine" : GameName;
                var p = path.SubstringAfter('/').SubstringBefore('/');
                if (p.Contains('.'))
                {
                    var ret = string.Concat(gameName, "/Content/", path.SubstringAfter('/'));
                    return comparisonType == StringComparison.OrdinalIgnoreCase ? ret.ToLowerInvariant() : ret;
                }

                if (p.Equals("Config", comparisonType) ||
                    p.Equals("Content", comparisonType) ||
                    p.Equals("Plugins", comparisonType))
                {
                    var ret = string.Concat(gameName, '/', path.SubstringAfter('/'));
                    return comparisonType == StringComparison.OrdinalIgnoreCase ? ret.ToLowerInvariant() : ret;
                }
                else
                {
                    var ret = string.Concat(gameName, "/Content/", path.SubstringAfter('/'));
                    return comparisonType == StringComparison.OrdinalIgnoreCase ? ret.ToLowerInvariant() : ret;
                }
            }

            if (VirtualPaths.TryGetValue(root, out var use))
            {
                var ret = string.Concat(use, "/Content/", path.SubstringAfter('/'));
                return comparisonType == StringComparison.OrdinalIgnoreCase ? ret.ToLowerInvariant() : ret;
            }
            else
            {
                var ret = string.Concat(GameName, $"/Plugins/{(GameName.ToLowerInvariant().Equals("fortnitegame") ? "GameFeatures/" : "")}{root}/Content/", path.SubstringAfter('/'));
                return comparisonType == StringComparison.OrdinalIgnoreCase ? ret.ToLowerInvariant() : ret;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SaveAsset(string path) => Files[path].Read();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySaveAsset(string path, out byte[] data)
        {
            if (!TryFindGameFile(path, out var file))
            {
                data = default;
                return false;
            }

            return file.TryRead(out data);
        }

        public static async Task<byte[]> SaveAssetAsync(string path) => await Task.Run(() => SaveAsset(path));
        public static async Task<byte[]?> TrySaveAssetAsync(string path) => await Task.Run(() =>
        {
            TrySaveAsset(path, out var data);
            return data;
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FArchive CreateReader(string path) => Files[path].CreateReader();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCreateReader(string path, out FArchive reader)
        {
            if (!TryFindGameFile(path, out var file))
            {
                reader = default;
                return false;
            }

            return file.TryCreateReader(out reader);
        }

        public static async Task<FArchive> CreateReaderAsync(string path) => await Task.Run(() => CreateReader(path));
        public static async Task<FArchive?> TryCreateReaderAsync(string path) => await Task.Run(() =>
        {
            TryCreateReader(path, out var reader);
            return reader;
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AbstractUePackage LoadPackage(string path) => LoadPackageAsync(Files[path]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AbstractUePackage LoadPackage(GameFile file) => LoadPackageAsync(file);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IoPackage LoadPackage(FPackageId id) => (IoPackage) LoadPackage(FilesById[id]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoadPackage(string path, out AbstractUePackage package)
        {
            if (!TryFindGameFile(path, out var file))
            {
                package = default;
                return false;
            }

            return TryLoadPackage(file, out package);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoadPackage(GameFile file, out AbstractUePackage package)
        {
            package = TryLoadPackageAsync(file);
            return package != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoadPackage(FPackageId id, out IoPackage package)
        {
            if (FilesById.TryGetValue(id, out var file))
            {
                if (TryLoadPackage(file, out AbstractUePackage loaded))
                {
                    package = (IoPackage)loaded;
                    return true;
                }

            }

            package = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AbstractUePackage LoadPackageAsync(string path) => LoadPackageAsync(Files[path]);

        public static AbstractUePackage LoadPackageAsync(GameFile file)
        {
            FArchive uasset = file.CreateReader();
            var containerHeader = ((FIoStoreEntry)file).IoStoreReader.ContainerHeader;
            return new IoPackage(uasset, GlobalData, containerHeader, MappingsForThisGame);
        }

        public static AbstractUePackage TryLoadPackageAsync(string path)
        {
            if (!TryFindGameFile(path, out var file))
            {
                return null;
            }

            return TryLoadPackageAsync(file);
        }

        public static AbstractUePackage TryLoadPackageAsync(GameFile file)
        {
            if (!file.IsUE4Package)
                return null;
            Files.TryGetValue(file.PathWithoutExtension + ".uexp", out var uexpFile);
            FArchive uassetTask = file.CreateReader();
            FArchive? uexpTask = uexpFile?.CreateReader();
            FArchive uasset = uassetTask;
            if (uasset == null)
                return null;
            var uexp = uexpTask != null ? uexpTask : null;

            try
            {

                if (file is FIoStoreEntry ioStoreEntry)
                {
                    return GlobalData != null ? new IoPackage(uasset, GlobalData, ioStoreEntry.IoStoreReader.ContainerHeader, MappingsForThisGame) : null;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load package " + file);
                return null;
            }
        }

        public static IReadOnlyDictionary<string, byte[]> SavePackage(string path) => SavePackage(Files[path]);

        public static IReadOnlyDictionary<string, byte[]> SavePackage(GameFile file) => SavePackageAsync(file).Result;

        public static bool TrySavePackage(string path, out IReadOnlyDictionary<string, byte[]> package)
        {
            if (!TryFindGameFile(path, out var file))
            {
                package = default;
                return false;
            }

            return TrySavePackage(file, out package);
        }

        public static bool TrySavePackage(GameFile file, out IReadOnlyDictionary<string, byte[]> package)
        {
            package = TrySavePackageAsync(file).Result;
            return package != null;
        }

        public static async Task<IReadOnlyDictionary<string, byte[]>> SavePackageAsync(string path) =>
            await SavePackageAsync(Files[path]);

        public static async Task<IReadOnlyDictionary<string, byte[]>> SavePackageAsync(GameFile file)
        {
            Files.TryGetValue(file.PathWithoutExtension + ".uexp", out var uexpFile);
            Files.TryGetValue(file.PathWithoutExtension + ".ubulk", out var ubulkFile);
            Files.TryGetValue(file.PathWithoutExtension + ".uptnl", out var uptnlFile);
            var uassetTask = file.ReadAsync();
            var uexpTask = uexpFile?.ReadAsync();
            var ubulkTask = ubulkFile?.ReadAsync();
            var uptnlTask = uptnlFile?.ReadAsync();
            var dict = new Dictionary<string, byte[]>
            {
                { file.Path, await uassetTask }
            };
            var uexp = uexpTask != null ? await uexpTask : null;
            var ubulk = ubulkTask != null ? await ubulkTask : null;
            var uptnl = uptnlTask != null ? await uptnlTask : null;
            if (uexpFile != null && uexp != null)
                dict[uexpFile.Path] = uexp;
            if (ubulkFile != null && ubulk != null)
                dict[ubulkFile.Path] = ubulk;
            if (uptnlFile != null && uptnl != null)
                dict[uptnlFile.Path] = uptnl;
            return dict;
        }

        public static async Task<IReadOnlyDictionary<string, byte[]>?> TrySavePackageAsync(string path)
        {
            if (!TryFindGameFile(path, out var file))
            {
                return null;
            }

            return await TrySavePackageAsync(file).ConfigureAwait(false);
        }

        public static async Task<IReadOnlyDictionary<string, byte[]>?> TrySavePackageAsync(GameFile file)
        {
            Files.TryGetValue(file.PathWithoutExtension + ".uexp", out var uexpFile);
            Files.TryGetValue(file.PathWithoutExtension + ".ubulk", out var ubulkFile);
            Files.TryGetValue(file.PathWithoutExtension + ".uptnl", out var uptnlFile);
            var uassetTask = file.TryReadAsync().ConfigureAwait(false);
            var uexpTask = uexpFile?.TryReadAsync().ConfigureAwait(false);
            var ubulkTask = ubulkFile?.TryReadAsync().ConfigureAwait(false);
            var uptnlTask = uptnlFile?.TryReadAsync().ConfigureAwait(false);

            var uasset = await uassetTask;
            if (uasset == null)
                return null;
            var uexp = uexpTask != null ? await uexpTask.Value : null;
            var ubulk = ubulkTask != null ? await ubulkTask.Value : null;
            var uptnl = uptnlTask != null ? await uptnlTask.Value : null;

            var dict = new Dictionary<string, byte[]>
            {
                { file.Path, uasset }
            };
            if (uexpFile != null && uexp != null)
                dict[uexpFile.Path] = uexp;
            if (ubulkFile != null && ubulk != null)
                dict[ubulkFile.Path] = ubulk;
            if (uptnlFile != null && uptnl != null)
                dict[uptnlFile.Path] = uptnl;
            return dict;
        }

        /// <summary>
        /// Returns an <see cref="UObject"/> from the <paramref name="objectPath"/> and <paramref name="objectName"/>.
        /// </summary>
        /// <param name="objectPath">The directory location including the extension as to where the package is located</param>
        /// <param name="objectName">The name of the <see cref="UObject"/> to load</param>
        /// <remarks>
        /// Use <seealso cref="LoadObjectExports"/> if you want to load all exports.
        /// <br/>
        /// A Unreal asset can have multiple exports, <paramref name="objectName"/> specifies which one to load.
        /// </remarks>
        /// <returns>An <see cref="UObject"/> containing all loaded data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UObject LoadObject(string objectPath, string objectName)
        {
            var packagePath = objectPath;
            if (objectName == null) objectName = Path.GetFileName(packagePath).SubstringBeforeLast('.');
            var pkg = LoadPackageAsync(packagePath);
            return pkg.GetExport(objectName, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns an <see cref="UObject"/> from the <paramref name="objectPath"/> at the <paramref name="index"/>.
        /// </summary>
        /// <param name="objectPath">The directory location including the extension as to where the package is located</param>
        /// <param name="index">The index of the <see cref="UObject"/> in the package/></param>
        /// <remarks>
        /// Use <seealso cref="LoadObjectExports"/> if you want to load all exports.
        /// <br/>
        /// A Unreal asset can have multiple exports, <paramref name="objectName"/> specifies which one to load.
        /// </remarks>
        /// <returns>An <see cref="UObject"/> containing all loaded data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UObject LoadObject(string objectPath, int index = 0)
        {
            if (UObjectCache.TryGetValue(objectPath, out UObject FoundObject))
                return FoundObject;
            var packagePath = objectPath;
            var pkg = LoadPackageAsync(packagePath);
            UObject Result = pkg.GetExport(index, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            UObjectCache.Add(packagePath, Result);
            return Result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoadObject(string? objectPath, out UObject export)
        {
            export = null;
            if (objectPath == null || objectPath.Equals("None", IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) return false;
            var packagePath = objectPath;
            string objectName;
            var dotIndex = packagePath.IndexOf('.');
            if (dotIndex == -1) // use the package name as object name
            {
                objectName = packagePath.SubstringAfterLast('/');
            }
            else // packagePath.objectName
            {
                objectName = packagePath.Substring(dotIndex + 1);
                packagePath = packagePath.Substring(0, dotIndex);
            }

            var pkg = TryLoadPackageAsync(packagePath);
            export = pkg?.GetExportOrNull(objectName, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            return export != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadObjectAsync<T>(string? objectPath) where T : UObject =>
            LoadObject(objectPath) as T ??
            throw new ParserException("Loaded object but it was of wrong type");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UObject[] LoadObjectExports(string? objectPath) => LoadPackage(objectPath).GetExports().ToArray();
    }
}
