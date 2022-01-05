using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.IO;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Pak;
using CUE4Parse.UE4.Pak.Objects;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Vfs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TModel.Modules;
using static CUE4Parse.Utils.StringUtils;

namespace TModel
{
    public static class DebugTools
    {
        private static List<UObject> Result = new();

        private static string FullPath = @"FortniteGame/Content/Athena/Items/Cosmetics/Characters/CID_A_291_Athena_Commando_F_IslandNomad.uasset";
        private static string MainKey = "dae1418b289573d4148c72f3c76abc7e2db9caa618a3eaf2d8580eb3a1bb7a63";

        private static DefaultFileProvider Provider;

        public static void DirectLoad()
        {
            Setup();
            LoadObject();
        }

        private static void Setup()
        {
            Provider = new DefaultFileProvider(Preferences.GameDirectory, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_1));

            // Initilize

            DirectoryInfo directory = Provider._workingDirectory;
            SearchOption option = SearchOption.TopDirectoryOnly;

            var osFiles = new Dictionary<string, GameFile>();

            // In .uproject mode, we must recursively look for files
            option = SearchOption.TopDirectoryOnly;
            IEnumerable<FileInfo> FoundFiles = directory.EnumerateFiles("*.*", option);
            foreach (FileInfo file in FoundFiles)
            {
                var ext = file.FullName.SubstringAfterLast('.');
                if (ext.Equals("pak", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var reader = new PakFileReader(file.FullName, file.OpenRead(), Provider.Versions) { IsConcurrent = true, CustomEncryption = Provider.CustomEncryption };
                        if (reader.IsEncrypted && !Provider._requiredKeys.ContainsKey(reader.Info.EncryptionKeyGuid))
                        {
                            Provider._requiredKeys[reader.Info.EncryptionKeyGuid] = null;
                        }
                        Provider.UnloadedVFS[reader] = null;
                    }
                    catch { }
                }
                else if (ext.Equals("utoc", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var reader = new IoStoreReader(file.FullName, file.OpenRead(), File.OpenRead(file.FullName.SubstringBeforeLast('.') + ".ucas"), EIoStoreTocReadOptions.ReadDirectoryIndex, Provider.Versions) { IsConcurrent = true, CustomEncryption = Provider.CustomEncryption };
                        if (reader.IsEncrypted && !Provider._requiredKeys.ContainsKey(reader.Info.EncryptionKeyGuid))
                        {
                            Provider._requiredKeys[reader.Info.EncryptionKeyGuid] = null;
                        }
                        Provider.UnloadedVFS[reader] = null;
                    }
                    catch { } // Could be bad Magic
                }
            }

            Provider._files.AddFiles(osFiles);

            SubmitKey();

            Provider.LoadMappings();
        }

        private static void SubmitKey()
        {
            var countNewMounts = 0;
            FGuid guid = new FGuid();
            var key = new FAesKey(MainKey);
            // Gets all VfsReaders that have the same GUID
            IEnumerable<IAesVfsReader> MatchingGUIDs = Provider.UnloadedVFS.Keys.Where(it => it.EncryptionKeyGuid == guid);
            foreach (IAesVfsReader reader in MatchingGUIDs)
            {
                if (Provider.GlobalData == null && reader is IoStoreReader ioReader && reader.Name.Equals("global.utoc", StringComparison.OrdinalIgnoreCase))
                {
                    Provider.GlobalData = new IoGlobalData(ioReader);
                }
                if (!reader.HasDirectoryIndex)
                    continue;
                try
                {
                    Provider._files.AddFiles(reader.Mount(Provider.IsCaseInsensitive));
                    Provider.UnloadedVFS.TryRemove(reader, out _);
                    Provider._mountedVfs[reader] = null;
                    Interlocked.Increment(ref countNewMounts);
                    FAesKey localKey = reader?.AesKey;
                    Provider._requiredKeys.TryRemove(reader.EncryptionKeyGuid, out _);
                    Provider._workingKeys.TryAdd(reader.EncryptionKeyGuid, localKey);
                }
                catch (InvalidAesKeyException)
                {
                    // Ignore this 
                }
                catch (Exception e)
                {
                    // Log.Warning(e, $"Uncaught exception while loading pak file {reader.Path.SubstringAfterLast('/')}");
                }
            }
        }

        private static void LoadObject()
        {
            GameFile file = Provider[FullPath];

            if (!file.IsUE4Package) throw new ArgumentException("File must be a package to be a loaded as one", nameof(file));
            Provider.Files.TryGetValue(file.PathWithoutExtension + ".uexp", out var uexpFile);
            Provider.Files.TryGetValue(file.PathWithoutExtension + ".ubulk", out var ubulkFile);
            Provider.Files.TryGetValue(file.PathWithoutExtension + ".uptnl", out var uptnlFile);
            FArchive uasset = file.CreateReader();
            FArchive ubulk = ubulkFile?.CreateReader() ?? null;
            FArchive uptnl = uptnlFile?.CreateReader() ?? null;

            IVfsFileProvider vfsFileProvider = Provider;

            var containerHeader = ((FIoStoreEntry)file).IoStoreReader.ContainerHeader;
            IPackage Package = new IoPackage(uasset, vfsFileProvider.GlobalData, containerHeader, ubulk, uptnl, Provider, Provider.MappingsForThisGame);

            Result = Package.GetExports().ToList();
        }
    }
}
