using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.PhysicsEngine
{
    public class UBodySetup : Assets.Exports.UObject
    {
        public FGuid BodySetupGuid;
        public FFormatContainer? CookedFormatData;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            BodySetupGuid = Ar.Read<FGuid>();

            var bCooked = Ar.ReadBoolean();
            if (!bCooked) return;
            if (Ar.Ver >= EUnrealEngineObjectUE4Version.STORE_HASCOOKEDDATA_FOR_BODYSETUP)
            {
                var _ = Ar.ReadBoolean(); // bTemp
            }

            CookedFormatData = new FFormatContainer(Ar);
        }
    }
}
