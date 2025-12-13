using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class Regle : Window
    {
        private DispatcherTimer timer;
        private ImageSource coneImage; // Variable globale pour retenir l'image reçue

        // Constructeur qui accepte l'image venant de ChoixCone
        public Regle(ImageSource selectedCone)
        {
            InitializeComponent();

            //On stocke l'image reçue
            coneImage = selectedCone;

            //On configure un minuteur 
            timer = new DispatcherTimer();

            // Le minuteur "tiquera" au bout de 3 secondes
            timer.Interval = TimeSpan.FromSeconds(3);

            // On lui dit quelle méthode lancer quand le temps est écoulé
            timer.Tick += FinDuCompteur_Tick;

            //On démarre le chrono
            timer.Start();
        }

        // Cette méthode se lance automatiquement après 3 secondes
        private void FinDuCompteur_Tick(object sender, EventArgs e)
        {
            // On arrête le timer pour pas qu'il recommence
            timer.Stop();

            // On lance le JEU en lui passant le cône
            Game partie = new Game(coneImage);
            partie.Show();

            // On ferme la fenêtre de règles car on n'y reviendra pas
            this.Close();
        }
    }
}
