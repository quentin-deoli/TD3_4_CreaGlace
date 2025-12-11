using System.Windows;

namespace CreaGlace
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Lancer la musique du menu principal une seule fois
            AudioManager.InitMusique("Winter.mp3");
        }
    }
}
