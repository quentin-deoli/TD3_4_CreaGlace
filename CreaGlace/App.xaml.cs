using System.Windows;

namespace CreaGlace
{
    public partial class App : Application
    {
        // On déclare ces variables en "static" pour pouvoir y accéder depuis 
        // n'importe quelle autre fenêtre en écrivant simplement App.VolumeMusique
        public static double VolumeMusique = 0.5; // 0.5 = 50%
        public static double VolumeSFX = 1.0;     // 1.0 = 100%
    }
}