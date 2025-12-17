using System.Windows;

namespace CreaGlace
{
    public partial class Gagne : Window
    {
        
        public Gagne(int scoreFinal, string tempsFinal)
        {
            InitializeComponent();
        }

        private void BoutonMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow accueil = new MainWindow(); 
            accueil.Show();
            this.Close();  
        }

        private void BoutonQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}