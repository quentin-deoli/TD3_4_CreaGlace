using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CreaGlace
{
    public partial class Regle : Window
    {
        private DispatcherTimer timer;
        private int coneChoisi;

        public Regle(int coneChoisi)
        {
            InitializeComponent();
            imgCone.Visibility = Visibility.Hidden;

            this.coneChoisi = coneChoisi;

            AfficherCone();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += FinDuCompteur_Tick;
            timer.Start();

        }

        // Affiche l'image correspondant au cône choisi
        private void AfficherCone()
        {
            string cheminImage = "";

            if (coneChoisi == 1)
                cheminImage = "Images/cone1.png";
            else if (coneChoisi == 2)
                cheminImage = "Images/cone2.png";
            else if (coneChoisi == 3)
                cheminImage = "Images/cone3.png";
            else if (coneChoisi == 4)
                cheminImage = "Images/cone4.png";

            imgCone.Source = new BitmapImage(
                new Uri(cheminImage, UriKind.Relative)
            );
        }

        // Appelée automatiquement après 3 secondes
        private void FinDuCompteur_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            // Le cône apparaît juste avant de lancer le jeu
            imgCone.Visibility = Visibility.Visible;

            Game partie = new Game(coneChoisi);
            partie.Show();

            this.Close();
        }
    }
}