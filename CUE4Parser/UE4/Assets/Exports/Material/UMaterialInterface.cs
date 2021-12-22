using System.Collections.Generic;

namespace CUE4Parse.UE4.Assets.Exports.Material
{
    public class UMaterialInterface : UObject
    {
        public List<FMaterialResource> LoadedMaterialResources { get; } = new List<FMaterialResource>();
    }
}
