using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CreaGlace
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Jouer_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre la fenêtre du jeu
            GameWindow game = new GameWindow();
            game.Show();

            // Fermeture du menu pour passer au jeu
            this.Close();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre la fenêtre des options
            options opt = new options();
            opt.Show();
        }

        private void Quitter_Click(object sender, RoutedEventArgs e)
        {
            // Quitte l'application
            Application.Current.Shutdown();
        }
    }
}
