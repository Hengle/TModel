﻿using System;
using System.Runtime.InteropServices;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.UObject;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FPrecomputedVisibilityCell : IUStruct
    {
        public readonly FVector Min;
        public readonly ushort ChunkIndex;
        public readonly ushort DataOffset;
    }

    [JsonConverter(typeof(FCompressedVisibilityChunkConverter))]
    public readonly struct FCompressedVisibilityChunk : IUStruct
    {
        public readonly bool bCompressed;
        public readonly int UncompressedSize;
        public readonly byte[] Data;

        public FCompressedVisibilityChunk(FAssetArchive Ar)
        {
            bCompressed = Ar.ReadBoolean();
            UncompressedSize = Ar.Read<int>();
            Data = Ar.ReadBytes(Ar.Read<int>());
        }
    }

    public class FCompressedVisibilityChunkConverter : JsonConverter<FCompressedVisibilityChunk>
    {
        public override void WriteJson(JsonWriter writer, FCompressedVisibilityChunk value, JsonSerializer serializer)
        {
            writer.WritePropertyName("bCompressed");
            writer.WriteValue(value.bCompressed);

            writer.WritePropertyName("UncompressedSize");
            writer.WriteValue(value.UncompressedSize);
        }

        public override FCompressedVisibilityChunk ReadJson(JsonReader reader, Type objectType, FCompressedVisibilityChunk existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public readonly struct FPrecomputedVisibilityBucket : IUStruct
    {
        public readonly int CellDataSize;
        public readonly FPrecomputedVisibilityCell[] Cells;
        public readonly FCompressedVisibilityChunk[] CellDataChunks;

        public FPrecomputedVisibilityBucket(FAssetArchive Ar)
        {
            CellDataSize = Ar.Read<int>();
            Cells = Ar.ReadArray<FPrecomputedVisibilityCell>();
            CellDataChunks = Ar.ReadArray(() => new FCompressedVisibilityChunk(Ar));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FPrecomputedVisibilityHandler : IUStruct
    {
        public readonly FVector2D PrecomputedVisibilityCellBucketOriginXY;
        public readonly float PrecomputedVisibilityCellSizeXY;
        public readonly float PrecomputedVisibilityCellSizeZ;
        public readonly int PrecomputedVisibilityCellBucketSizeXY;
        public readonly int PrecomputedVisibilityNumCellBuckets;
        public readonly FPrecomputedVisibilityBucket[] PrecomputedVisibilityCellBuckets;

        public FPrecomputedVisibilityHandler(FAssetArchive Ar)
        {
            PrecomputedVisibilityCellBucketOriginXY = Ar.Read<FVector2D>();
            PrecomputedVisibilityCellSizeXY = Ar.Read<float>();
            PrecomputedVisibilityCellSizeZ = Ar.Read<float>();
            PrecomputedVisibilityCellBucketSizeXY = Ar.Read<int>();
            PrecomputedVisibilityNumCellBuckets = Ar.Read<int>();
            PrecomputedVisibilityCellBuckets = Ar.ReadArray(() => new FPrecomputedVisibilityBucket(Ar));
        }
    }

    public readonly struct FPrecomputedVolumeDistanceField : IUStruct
    {
        public readonly float VolumeMaxDistance;
        public readonly FBox VolumeBox;
        public readonly int VolumeSizeX;
        public readonly int VolumeSizeY;
        public readonly int VolumeSizeZ;
        public readonly FColor[] Data;

        public FPrecomputedVolumeDistanceField(FAssetArchive Ar)
        {
            VolumeMaxDistance = Ar.Read<float>();
            VolumeBox = Ar.Read<FBox>();
            VolumeSizeX = Ar.Read<int>();
            VolumeSizeY = Ar.Read<int>();
            VolumeSizeZ = Ar.Read<int>();
            Data = Ar.ReadArray<FColor>();
        }
    }

    public class ULevel : Assets.Exports.UObject
    {
        public FPackageIndex[] Actors { get; private set; }
        public FURL URL { get; private set; }
        public FPackageIndex Model { get; private set; }
        public FPackageIndex[] ModelComponents { get; private set; }
        public FPackageIndex LevelScriptActor { get; private set; }
        public FPackageIndex NavListStart { get; private set; }
        public FPackageIndex NavListEnd { get; private set; }
        public FPrecomputedVisibilityHandler PrecomputedVisibilityHandler { get; private set; }
        public FPrecomputedVolumeDistanceField PrecomputedVolumeDistanceField { get; private set; }

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);
            Actors = Ar.ReadArray(() => new FPackageIndex(Ar));
            URL = new FURL(Ar);
            Model = new FPackageIndex(Ar);
            ModelComponents = Ar.ReadArray(() => new FPackageIndex(Ar));
            LevelScriptActor = new FPackageIndex(Ar);
            NavListStart = new FPackageIndex(Ar);
            NavListEnd = new FPackageIndex(Ar);
            PrecomputedVisibilityHandler = new FPrecomputedVisibilityHandler(Ar);
            PrecomputedVolumeDistanceField = new FPrecomputedVolumeDistanceField(Ar);
        }
    }
}