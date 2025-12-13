using System.Windows;

namespace CreaGlace
{
    public partial class Options : Window
    {
        public partial class App : Application
        {
            // C'est ici qu'on déclare les variables pour qu'elles soient visibles partout !
            public static double VolumeMusique = 0.5;
            public static double VolumeSFX = 1.0;
        }
        public Options()
        {
            InitializeComponent();

            // 1. À l'ouverture, on positionne les sliders selon ce qui est mémorisé
            SliderMusique.Value = App.VolumeMusique * 100;
            SliderSFX.Value = App.VolumeSFX * 100;
            
            // On met à jour l'affichage du texte (ex: "50%")
            txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";
            txtSFXValeur.Text = $"{SliderSFX.Value:0}%";
        }

        // Quand on bouge le slider Musique
        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Mise à jour du texte à côté du slider
            if (txtMusiqueValeur != null)
                txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";

            // On sauvegarde la valeur dans la mémoire globale (App)
            // On divise par 100 car le slider est de 0 à 100, mais le volume souvent géré de 0.0 à 1.0
            App.VolumeMusique = SliderMusique.Value / 100.0;
        }

        // Quand on bouge le slider Effets Sonores
        private void SliderSFX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtSFXValeur != null)
                txtSFXValeur.Text = $"{SliderSFX.Value:0}%";

            App.VolumeSFX = SliderSFX.Value / 100.0;
        }

        private void BoutonRetour_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
