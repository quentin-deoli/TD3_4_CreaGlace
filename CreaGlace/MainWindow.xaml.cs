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

            // Initialiser et jouer la musique Winter.mp3 si ce n'est pas déjà fait
            AudioManager.InitMusique("Winter.mp3");
        }

        private void Jouer_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre la page de choix du cornet
            ChoixCone choix = new ChoixCone();
            choix.Show();

            // Ferme le menu
            this.Close();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options opt = new Options();
            opt.Show();
        }

        private void Quitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
