using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.IO;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Vfs;
using CUE4Parse.Utils;

namespace CUE4Parse.FileProvider.Vfs
{
    public abstract class AbstractVfsFileProvider : AbstractFileProvider, IVfsFileProvider
    {
        protected FileProviderDictionary _files;
        public override IReadOnlyDictionary<string, GameFile> Files => _files;
        public override IReadOnlyDictionary<FPackageId, GameFile> FilesById => _files.byId;

        public readonly ConcurrentDictionary<IAesVfsReader, object?> UnloadedVFS = new ();

        private readonly ConcurrentDictionary<IAesVfsReader, object?> _mountedVfs = new ();
        public IReadOnlyCollection<IAesVfsReader> MountedVfs => (IReadOnlyCollection<IAesVfsReader>) _mountedVfs.Keys;

        public readonly ConcurrentDictionary<FGuid, FAesKey> _workingKeys = new ();
        
        protected readonly ConcurrentDictionary<FGuid, object?> _requiredKeys = new ();
        public IReadOnlyCollection<FGuid> RequiredKeys => (IReadOnlyCollection<FGuid>) _requiredKeys.Keys;
        
        public IoGlobalData? GlobalData { get; private set; }
        
        public IAesVfsReader.CustomEncryptionDelegate? CustomEncryption { get; set; }

        protected AbstractVfsFileProvider(bool isCaseInsensitive = false, VersionContainer? versions = null) : base(isCaseInsensitive, versions)
        {
            _files = new FileProviderDictionary(IsCaseInsensitive);
        }

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
        public IEnumerable<string> GetFilesInPath(string GivenPath)
        {
            // Makes sure the path ends with '/'
            string Path = GivenPath.EndsWith('/') ? GivenPath : GivenPath + '/';
            List<string> Result = new List<string>();

            foreach (string path in Files.Keys)
            {
                if (path.StartsWith(GivenPath))
                {
                    string Base = path.Substring(GivenPath.Length + 1);
                    string Final = "";
                    if (Base.Contains('/'))
                        Final = Base.SubstringBefore('/');
                    else
                        Final = Base;

                    if (Result.IndexOf(Final) == -1)
                        Result.Add(Final);
                }
            }

            return Result;
        }

        public List<string> GetSubPaths(string GivenPath)
        {
            List<string> Result = new List<string>();
            foreach (string path in Files.Keys)
            {
                if (path.StartsWith(GivenPath))
                {
                    Result.Add(path);
                }
            }

            return Result;
        }

        public void LoadMappings()
        {
            if (GameName.Equals("FortniteGame", StringComparison.OrdinalIgnoreCase))
            {
                MappingsContainer = new FortniteMappingsProvider("fortnitegame");
            }
        }

        public IEnumerable<IAesVfsReader> UnloadedVfsByGuid(FGuid guid) => UnloadedVFS.Keys.Where(it => it.EncryptionKeyGuid == guid);
        public void UnloadAllVfs()
        {
            _files = new FileProviderDictionary(IsCaseInsensitive);
            foreach (var reader in _mountedVfs.Keys)
            {
                _workingKeys.TryRemove(reader.EncryptionKeyGuid, out _);
                _requiredKeys[reader.EncryptionKeyGuid] = null;
                _mountedVfs.TryRemove(reader, out _);
                UnloadedVFS[reader] = null;
            }
        }

        public int Mount() => MountAsync().Result;
        public async Task<int> MountAsync()
        {
            var countNewMounts = 0;
            var tasks = new LinkedList<Task>();
            foreach (var reader in UnloadedVFS.Keys)
            {
                if (GlobalData == null && reader is IoStoreReader ioReader && reader.Name.Equals("global.utoc", StringComparison.OrdinalIgnoreCase))
                {
                    GlobalData = new IoGlobalData(ioReader);
                }

                if ((reader.IsEncrypted && CustomEncryption == null) || !reader.HasDirectoryIndex)
                    continue;
                
                tasks.AddLast(Task.Run(() =>
                {
                    try
                    {
                        // Ensure that the custom encryption delegate specified for the provider is also used for the reader
                        reader.CustomEncryption = CustomEncryption;
                        reader.MountTo(_files, IsCaseInsensitive);
                        UnloadedVFS.TryRemove(reader, out _);
                        _mountedVfs[reader] = null;
                        Interlocked.Increment(ref countNewMounts);
                        return reader;
                    }
                    catch (InvalidAesKeyException)
                    {
                        // Ignore this 
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, $"Uncaught exception while loading file {reader.Path.SubstringAfterLast('/')}");
                    }
                    return null;
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return countNewMounts;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SubmitKey(FGuid guid, FAesKey key) => SubmitKeys(new Dictionary<FGuid, FAesKey> {{ guid, key }});
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SubmitKeys(IEnumerable<KeyValuePair<FGuid, FAesKey>> keys) => SubmitKeysAsync(keys).Result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<int> SubmitKeyAsync(FGuid guid, FAesKey key) =>
            await SubmitKeysAsync(new Dictionary<FGuid, FAesKey> {{ guid, key }}).ConfigureAwait(false);

        public async Task<int> SubmitKeysAsync(IEnumerable<KeyValuePair<FGuid, FAesKey>> keys)
        {
            var countNewMounts = 0;
            foreach (var it in keys)
            {
                var guid = it.Key;
                var key = it.Value;
                foreach (var reader in UnloadedVfsByGuid(guid))
                {
                    if (GlobalData == null && reader is IoStoreReader ioReader && reader.Name.Equals("global.utoc", StringComparison.OrdinalIgnoreCase))
                    {
                        GlobalData = new IoGlobalData(ioReader);
                    }
                    if (!reader.HasDirectoryIndex)
                        continue;
                    try
                    {
                        reader.MountTo(_files, IsCaseInsensitive, key);
                        UnloadedVFS.TryRemove(reader, out _);
                        _mountedVfs[reader] = null;
                        Interlocked.Increment(ref countNewMounts);
                        FAesKey localKey = reader?.AesKey;
                        _requiredKeys.TryRemove(reader.EncryptionKeyGuid, out _);
                        _workingKeys.TryAdd(reader.EncryptionKeyGuid, localKey);
                    }
                    catch (InvalidAesKeyException)
                    {
                        // Ignore this 
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, $"Uncaught exception while loading pak file {reader.Path.SubstringAfterLast('/')}");
                    }
                }
            }

            return countNewMounts;
        }

        public void Dispose()
        {
            _files = new FileProviderDictionary(IsCaseInsensitive);
            foreach (var reader in UnloadedVFS.Keys) reader.Dispose();
            UnloadedVFS.Clear();
            foreach (var reader in MountedVfs) reader.Dispose();
            _mountedVfs.Clear();
            _workingKeys.Clear();
            _requiredKeys.Clear();
            GlobalData = null;
        }
    }
}