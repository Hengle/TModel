﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Assets.Utils;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;
using Serilog;

// TODO: Remove is not used
#if false
namespace CUE4Parse.UE4.Assets
{

    public sealed class BadPackage : AbstractUePackage
    {
        public override FPackageFileSummary Summary { get; }
        public override FNameEntrySerialized[] NameMap { get; }
        public FObjectImport[] ImportMap { get; }
        public FObjectExport[] ExportMap { get; }
        public FPackageIndex[][]? DependsMap { get; }
        public FPackageIndex[]? PreloadDependencies { get; }
        public override Lazy<UObject>[] ExportsLazy => ExportMap.Select(it => it.ExportObject).ToArray();
        public override bool IsFullyLoaded { get; } = false;
        private ExportLoader[] _exportLoaders; // Nonnull if useLazySerialization is false

        public BadPackage(FArchive uasset, FArchive? uexp, FArchive? ubulk = null, FArchive? uptnl = null, TypeMappings? mappings = null, bool useLazySerialization = true)
            : base(uasset.Name.SubstringBeforeLast("."), mappings)
        {
            // We clone the version container because it can be modified with package specific versions when reading the summary
            uasset.Versions = (VersionContainer) uasset.Versions.Clone();
            var uassetAr = new FAssetArchive(uasset, this);
            Summary = new FPackageFileSummary(uassetAr);

            uassetAr.SeekAbsolute(Summary.NameOffset, SeekOrigin.Begin);
            NameMap = new FNameEntrySerialized[Summary.NameCount];
            uassetAr.ReadArray(NameMap, () => new FNameEntrySerialized(uassetAr));

            uassetAr.SeekAbsolute(Summary.ImportOffset, SeekOrigin.Begin);
            ImportMap = new FObjectImport[Summary.ImportCount];
            uassetAr.ReadArray(ImportMap, () => new FObjectImport(uassetAr));

            uassetAr.SeekAbsolute(Summary.ExportOffset, SeekOrigin.Begin);
            ExportMap = new FObjectExport[Summary.ExportCount]; // we need this to get its final size in some case
            uassetAr.ReadArray(ExportMap, () => new FObjectExport(uassetAr));

            if (!useLazySerialization && Summary.DependsOffset > 0 && Summary.ExportCount > 0)
            {
                uassetAr.SeekAbsolute(Summary.DependsOffset, SeekOrigin.Begin);
                DependsMap = uassetAr.ReadArray(Summary.ExportCount, () => uassetAr.ReadArray(() => new FPackageIndex(uassetAr)));
            }

            if (!useLazySerialization && Summary.PreloadDependencyCount > 0 && Summary.PreloadDependencyOffset > 0)
            {
                uassetAr.SeekAbsolute(Summary.PreloadDependencyOffset, SeekOrigin.Begin);
                PreloadDependencies = uassetAr.ReadArray(Summary.PreloadDependencyCount, () => new FPackageIndex(uassetAr));
            }

            var uexpAr = uexp != null ? new FAssetArchive(uexp, this, (int) uassetAr.Length) : uassetAr;

            if (ubulk != null)
            {
                //var offset = (int) (Summary.TotalHeaderSize + ExportMap.Sum(export => export.SerialSize));
                var offset = Summary.BulkDataStartOffset;
                uexpAr.AddPayload(PayloadType.UBULK, offset, ubulk);
            }

            if (uptnl != null)
            {
                var offset = Summary.BulkDataStartOffset;
                uexpAr.AddPayload(PayloadType.UPTNL, offset, uptnl);
            }

            if (useLazySerialization)
            {
                foreach (var export in ExportMap)
                {
                    export.ExportObject = new Lazy<UObject>(() =>
                    {
                        // Create
                        var obj = ConstructObject(ResolvePackageIndex(export.ClassIndex)?.Object?.Value as UStruct);
                        obj.Name = export.ObjectName.Text;
                        obj.Outer = (ResolvePackageIndex(export.OuterIndex) as ResolvedExportObject)?.Object.Value ?? this;
                        obj.Super = ResolvePackageIndex(export.SuperIndex) as ResolvedExportObject;
                        obj.Template = ResolvePackageIndex(export.TemplateIndex) as ResolvedExportObject;
                        obj.Flags |= (EObjectFlags) export.ObjectFlags; // We give loaded objects the RF_WasLoaded flag in ConstructObject, so don't remove it again in here 

                        // Serialize
                        var Ar = (FAssetArchive) uexpAr.Clone();
                        Ar.SeekAbsolute(export.SerialOffset, SeekOrigin.Begin);
                        DeserializeObject(obj, Ar, export.SerialSize);
                        // TODO right place ???
                        obj.Flags |= EObjectFlags.RF_LoadCompleted;
                        obj.PostLoad();
                        return obj;
                    });
                }
            }
            else
            {
                _exportLoaders = new ExportLoader[ExportMap.Length];
                for (var i = 0; i < ExportMap.Length; i++)
                {
                    _exportLoaders[i] = new(this, ExportMap[i], uexpAr);
                }
            }

            IsFullyLoaded = true;
        }

        public BadPackage(string name, byte[] uasset, byte[]? uexp, byte[]? ubulk = null, byte[]? uptnl = null, bool useLazySerialization = true)
            : this(new FByteArchive($"{name}.uasset", uasset), uexp != null ? new FByteArchive($"{name}.uexp", uexp) : null,
                ubulk != null ? new FByteArchive($"{name}.ubulk", ubulk) : null,
                uptnl != null ? new FByteArchive($"{name}.uptnl", uptnl) : null, null, useLazySerialization) { }

        public override UObject? GetExportOrNull(string name, StringComparison comparisonType = StringComparison.Ordinal)
        {
            try
            {
                return ExportMap
                    .FirstOrDefault(it => it.ObjectName.Text.Equals(name, comparisonType))?.ExportObject
                    .Value;
            }
            catch (Exception e)
            {
                Log.Debug(e, "Failed to get export object");
                return null;
            }
        }

        public override UObject? GetExportOrNull(int index, StringComparison comparisonType = StringComparison.Ordinal) => ExportMap[index].ExportObject.Value;

        public override ResolvedObject? ResolvePackageIndex(FPackageIndex? index)
        {
            if (index == null || index.IsNull)
                return null;
            if (index.IsImport && -index.Index - 1 < ImportMap.Length)
                return ResolveImport(index);
            if (index.IsExport && index.Index - 1 < ExportMap.Length)
                return new ResolvedExportObject(index.Index - 1, this);
            return null;
        }

        private ResolvedObject? ResolveImport(FPackageIndex importIndex)
        {
            var import = ImportMap[-importIndex.Index - 1];
            var outerMostIndex = importIndex;
            FObjectImport outerMostImport;
            while (true)
            {
                outerMostImport = ImportMap[-outerMostIndex.Index - 1];
                if (outerMostImport.OuterIndex.IsNull)
                    break;
                outerMostIndex = outerMostImport.OuterIndex;
            }

            outerMostImport = ImportMap[-outerMostIndex.Index - 1];
            // We don't support loading script packages, so just return a fallback
            if (outerMostImport.ObjectName.Text.StartsWith("/Script/"))
            {
                return new ResolvedImportObject(import, this);
            }

            BadPackage? importPackage = null;
            if (FileProvider.FileProvider.TryLoadPackage(outerMostImport.ObjectName.Text, out var package))
                importPackage = package as BadPackage;
            if (importPackage == null)
            {
                Log.Error("Missing native package ({0}) for import of {1} in {2}.", outerMostImport.ObjectName, import.ObjectName, Name);
                return new ResolvedImportObject(import, this);
            }

            string? outer = null;
            if (outerMostIndex != import.OuterIndex && import.OuterIndex.IsImport)
            {
                var outerImport = ImportMap[-import.OuterIndex.Index - 1];
                outer = ResolveImport(import.OuterIndex)?.GetPathName();
                if (outer == null)
                {
                    Log.Fatal("Missing outer for import of ({0}): {1} in {2} was not found, but the package exists.", Name, outerImport.ObjectName, importPackage.GetFullName());
                    return new ResolvedImportObject(import, this);
                }
            }

            for (var i = 0; i < importPackage.ExportMap.Length; i++)
            {
                var export = importPackage.ExportMap[i];
                if (export.ObjectName.Text != import.ObjectName.Text)
                    continue;
                var thisOuter = importPackage.ResolvePackageIndex(export.OuterIndex);
                if (thisOuter?.GetPathName() == outer)
                    return new ResolvedExportObject(i, importPackage);
            }

            Log.Fatal("Missing import of ({0}): {1} in {2} was not found, but the package exists.", Name, import.ObjectName, importPackage.GetFullName());
            return new ResolvedImportObject(import, this);
        }

        private class ResolvedExportObject : ResolvedObject
        {
            private readonly FObjectExport _export;

            public ResolvedExportObject(int exportIndex, BadPackage package) : base(package, exportIndex)
            {
                _export = package.ExportMap[exportIndex];
            }

            public override FName Name => _export.ObjectName;
            public override ResolvedObject Outer => Package.ResolvePackageIndex(_export.OuterIndex) ?? new ResolvedLoadedObject((UObject) Package);
            public override ResolvedObject? Class => Package.ResolvePackageIndex(_export.ClassIndex);
            public override ResolvedObject? Super => Package.ResolvePackageIndex(_export.SuperIndex);
            public override Lazy<UObject> Object => _export.ExportObject;
        }

        /** Fallback if we cannot resolve the export in another package */
        private class ResolvedImportObject : ResolvedObject
        {
            private readonly FObjectImport _import;

            public ResolvedImportObject(FObjectImport import, BadPackage package) : base(package)
            {
                _import = import;
            }

            public override FName Name => _import.ObjectName;
            public override ResolvedObject? Outer => Package.ResolvePackageIndex(_import.OuterIndex);
            public override ResolvedObject Class => new ResolvedLoadedObject(new UScriptClass(_import.ClassName.Text));
            public override Lazy<UObject>? Object => _import.ClassName.Text == "Class" ? new(() => new UScriptClass(Name.Text)) : null;
        }

        private class ExportLoader
        {
            private BadPackage _package;
            private FObjectExport _export;
            private FAssetArchive _archive;
            private UObject _object;
            private List<LoadDependency>? _dependencies;
            private LoadPhase _phase = LoadPhase.Create;
            public Lazy<UObject> Lazy;

            public ExportLoader(BadPackage package, FObjectExport export, FAssetArchive archive)
            {
                _package = package;
                _export = export;
                _archive = archive;
                Lazy = new(() =>
                {
                    Fire(LoadPhase.Serialize);
                    return _object;
                });
                export.ExportObject = Lazy;
            }

            private void EnsureDependencies()
            {
                if (_dependencies != null)
                {
                    return;
                }

                _dependencies = new();
                var runningIndex = _export.FirstExportDependency;
                if (runningIndex >= 0)
                {
                    for (var index = _export.SerializationBeforeSerializationDependencies; index > 0; index--)
                    {
                        var dep = _package.PreloadDependencies[runningIndex++];
                        // don't request IO for this export until these are serialized
                        _dependencies.Add(new(LoadPhase.Serialize, LoadPhase.Serialize, ResolveLoader(dep)));
                    }
                    for (var index = _export.CreateBeforeSerializationDependencies; index > 0; index--)
                    {
                        var dep = _package.PreloadDependencies[runningIndex++];
                        // don't request IO for this export until these are done
                        _dependencies.Add(new(LoadPhase.Serialize, LoadPhase.Create, ResolveLoader(dep)));
                    }
                    for (var index = _export.SerializationBeforeCreateDependencies; index > 0; index--)
                    {
                        var dep = _package.PreloadDependencies[runningIndex++];
                        // can't create this export until these things are serialized
                        _dependencies.Add(new(LoadPhase.Create, LoadPhase.Serialize, ResolveLoader(dep)));
                    }
                    for (var index = _export.CreateBeforeCreateDependencies; index > 0; index--)
                    {
                        var dep = _package.PreloadDependencies[runningIndex++];
                        // can't create this export until these things are created
                        _dependencies.Add(new(LoadPhase.Create, LoadPhase.Create, ResolveLoader(dep)));
                    }
                }
                else
                {
                    // We only need the outer to be created first
                    _dependencies.Add(new(LoadPhase.Create, LoadPhase.Create, ResolveLoader(_export.OuterIndex)));
                }
            }

            private ExportLoader? ResolveLoader(FPackageIndex index)
            {
                if (index.IsExport)
                {
                    return _package._exportLoaders[index.Index - 1];
                }
                return null;
            }

            private void Fire(LoadPhase untilPhase)
            {
                if (untilPhase >= LoadPhase.Create && _phase <= LoadPhase.Create)
                {
                    FireDependencies(LoadPhase.Create);
                    Create();
                }
                if (untilPhase >= LoadPhase.Serialize && _phase <= LoadPhase.Serialize)
                {
                    FireDependencies(LoadPhase.Serialize);
                    Serialize();
                }
            }

            private void FireDependencies(LoadPhase phase)
            {
                EnsureDependencies();
                foreach (var dependency in _dependencies)
                {
                    if (dependency.FromPhase == phase)
                    {
                        dependency.Target?.Fire(dependency.ToPhase);
                    }
                }
            }

            private void Create()
            {
                Trace.Assert(_phase == LoadPhase.Create);
                _phase = LoadPhase.Serialize;
                _object = ConstructObject(_package.ResolvePackageIndex(_export.ClassIndex)?.Object?.Value as UStruct);
                _object.Name = _export.ObjectName.Text;
                if (!_export.OuterIndex.IsNull)
                {
                    Trace.Assert(_export.OuterIndex.IsExport, "Outer imports are not yet supported");
                    _object.Outer = _package._exportLoaders[_export.OuterIndex.Index - 1]._object;
                }
                else
                {
                    _object.Outer = _package;
                }
                _object.Super = _package.ResolvePackageIndex(_export.SuperIndex) as ResolvedExportObject;
                _object.Template = _package.ResolvePackageIndex(_export.TemplateIndex) as ResolvedExportObject;
                _object.Flags |= (EObjectFlags) _export.ObjectFlags; // We give loaded objects the RF_WasLoaded flag in ConstructObject, so don't remove it again in here
            }

            private void Serialize()
            {
                Trace.Assert(_phase == LoadPhase.Serialize);
                _phase = LoadPhase.Complete;
                var Ar = (FAssetArchive) _archive.Clone();
                Ar.SeekAbsolute(_export.SerialOffset, SeekOrigin.Begin);
                DeserializeObject(_object, Ar, _export.SerialSize);
                // TODO right place ???
                _object.Flags |= EObjectFlags.RF_LoadCompleted;
                _object.PostLoad();
            }
        }

        private class LoadDependency
        {
            public LoadPhase FromPhase, ToPhase;
            public ExportLoader? Target;

            public LoadDependency(LoadPhase fromPhase, LoadPhase toPhase, ExportLoader? target)
            {
                FromPhase = fromPhase;
                ToPhase = toPhase;
                Target = target;
            }
        }

        private enum LoadPhase
        {
            Create, Serialize, Complete
        }
    }
}
#endif