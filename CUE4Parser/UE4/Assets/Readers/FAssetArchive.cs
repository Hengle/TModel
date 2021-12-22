using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Readers;
using Serilog;

namespace CUE4Parse.UE4.Assets.Readers
{
    public class FAssetArchive : FArchive
    {
        private readonly Dictionary<PayloadType, Lazy<FAssetArchive?>> Payloads;
        private readonly FArchive BaseArchive;

        public bool HasUnversionedProperties => Owner.HasFlags(EPackageFlags.PKG_UnversionedProperties);
        public bool IsFilterEditorOnly => Owner.HasFlags(EPackageFlags.PKG_FilterEditorOnly);
        public readonly AbstractUePackage Owner;
        public int AbsoluteOffset;

        public FAssetArchive(FArchive baseArchive, AbstractUePackage owner, int absoluteOffset = 0, Dictionary<PayloadType, Lazy<FAssetArchive?>>? payloads = null) : base(baseArchive.Versions)
        {
            Payloads = payloads ?? new();
            if (payloads != null && payloads.Count > 0)
            {
                throw new Exception("Found payload");
            }
            BaseArchive = baseArchive;
            Owner = owner;
            AbsoluteOffset = absoluteOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override FName ReadFName()
        {
            var nameIndex = Read<int>();
            var extraIndex = Read<int>();
#if !NO_FNAME_VALIDATION
            if (nameIndex < 0 || nameIndex >= Owner.NameMap.Length)
            {
                throw new ParserException(this, $"FName could not be read, requested index {nameIndex}, name map size {Owner.NameMap.Length}");
            }
#endif
            return new FName(Owner.NameMap[nameIndex], nameIndex, extraIndex);
        }

        // TODO not really optimal, there should be TryReadObject functions etc
        public Lazy<T?> ReadObject<T>() where T : UObject
        {
            var index = new FPackageIndex(this);
            var resolved = index.ResolvedObject;
            return new Lazy<T?>(() =>
            {
                if (resolved == null)
                {
                    if (index.IsNull)
                    {
                        // Silent this as it is expected to not have an object
                        return null;
                    }
                    Log.Warning("Failed to resolve index {Index}", index);
                    return null;
                }

                if (!resolved.TryLoad(out var obj))
                {
                    Log.Warning("Failed to load object {Obj}", resolved.Name);
                    return null;
                }

                if (obj is T cast)
                {
                    return cast;
                }
                Log.Warning("Object has unexpected type {ObjType}, expected type {Type}", obj.GetType().Name, typeof(T).Name);

                return null;
            });
        }

        public bool TryGetPayload(PayloadType type, out FAssetArchive? ar)
        {
            ar = null;
            if (!Payloads.TryGetValue(type, out var ret)) return false;

            ar = ret.Value;
            return true;
        }

        public FAssetArchive GetPayload(PayloadType type)
        {
            Payloads.TryGetValue(type, out var ret);
            FAssetArchive reader = ret?.Value;
            return reader ?? throw new ParserException(this, $"{type} is needed to parse the current package");
        }

        public void AddPayload(PayloadType type, FAssetArchive payload)
        {
            if (Payloads.ContainsKey(type))
            {
                throw new ParserException(this, $"Can't add a payload that is already attached of type {type}");
            }

            Payloads[type] = new Lazy<FAssetArchive?>(() => payload);
        }

        public void AddPayload(PayloadType type, int absoluteOffset, FArchive? payload)
        {
            if (Payloads.ContainsKey(type))
            {
                throw new ParserException(this, $"Can't add a payload that is already attached of type {type}");
            }

            Payloads[type] = new Lazy<FAssetArchive?>(() =>
            {
                var rawAr = payload;
                return rawAr == null ? null : new FAssetArchive(rawAr, Owner, absoluteOffset);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(byte[] buffer, int offset, int count)
            => BaseArchive.Read(buffer, offset, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin)
            => BaseArchive.Seek(offset, origin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long SeekAbsolute(long offset, SeekOrigin origin)
            => BaseArchive.Seek(offset - AbsoluteOffset, origin);

        public override bool CanSeek => BaseArchive.CanSeek;
        public override long Length => BaseArchive.Length;
        public long AbsolutePosition => AbsoluteOffset + Position;
        public override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BaseArchive.Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => BaseArchive.Position = value;
        }

        public override string Name => BaseArchive.Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override T Read<T>()
            => BaseArchive.Read<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte[] ReadBytes(int length)
            => BaseArchive.ReadBytes(length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Serialize(byte* ptr, int length)
            => BaseArchive.Serialize(ptr, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override T[] ReadArray<T>(int length)
            => BaseArchive.ReadArray<T>(length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReadArray<T>(T[] array)
            => BaseArchive.ReadArray(array);

        // For performance reasons we carry over the payloads dict to the cloned instance
        // Shouldn't be a big deal since we add the payloads during package initialization phase, not during object serialization 
        public override object Clone() => new FAssetArchive((FArchive) BaseArchive.Clone(), Owner, AbsoluteOffset, Payloads);
    }
}