using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TModel
{
    public static class ObjectIcons
    {
        private static string BaseRoot = @"C:\Users\bigho\source\repos\TModel\Resources\";

        public static BitmapImage UObject = new BitmapImage(new Uri(BaseRoot + "UObject.png"));
        public static BitmapImage SkeletalMesh = new BitmapImage(new Uri(BaseRoot + "SkeletalMesh.png"));
        public static BitmapImage Sound = new BitmapImage(new Uri(BaseRoot + "Sound.png"));
        public static BitmapImage SoundCue = new BitmapImage(new Uri(BaseRoot + "SoundCue.png"));
        public static BitmapImage World = new BitmapImage(new Uri(BaseRoot + "World.png"));
        public static BitmapImage FortItem = new BitmapImage(new Uri(BaseRoot + "FortItem.png"));
        public static BitmapImage Skeleton = new BitmapImage(new Uri(BaseRoot + "Skeleton.png"));
        public static BitmapImage StaticMesh = new BitmapImage(new Uri(BaseRoot + "StaticMesh.png"));
        public static BitmapImage Level = new BitmapImage(new Uri(BaseRoot + "Level.png"));
        public static BitmapImage Widget = new BitmapImage(new Uri(BaseRoot + "Widget.png"));
        public static BitmapImage Blueprint = new BitmapImage(new Uri(BaseRoot + "Blueprint.png"));
        public static BitmapImage Material = new BitmapImage(new Uri(BaseRoot + "Material.png"));
        public static BitmapImage MaterialInstance = new BitmapImage(new Uri(BaseRoot + "MaterialInstance.png"));

        public static BitmapImage CurveTable = new BitmapImage(new Uri(BaseRoot + "CurveTable.png"));
        public static BitmapImage NiagaraSystem = new BitmapImage(new Uri(BaseRoot + "NiagaraSystem.png"));
        public static BitmapImage AnimMontage = new BitmapImage(new Uri(BaseRoot + "AnimMontage.png"));
        public static BitmapImage BlendSpace = new BitmapImage(new Uri(BaseRoot + "BlendSpace.png"));
        public static BitmapImage AnimSequence = new BitmapImage(new Uri(BaseRoot + "AnimSequence.png"));
    }
}
