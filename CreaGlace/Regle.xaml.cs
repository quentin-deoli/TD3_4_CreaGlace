using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class Regle : Window
    {
        private DispatcherTimer timer;
        private ImageSource coneImage; // sauvegarder le cône choisi

        public Regle(ImageSource selectedCone)
        {
            InitializeComponent();
            coneImage = selectedCone;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); 
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            // Ouvrir Game avec le cône sélectionné
            Game partie = new Game(coneImage);
            partie.Show();

            this.Close();
        }
    }
}
