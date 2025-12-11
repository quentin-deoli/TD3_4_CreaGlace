using System.Windows;

namespace CreaGlace
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // La musique est déjà lancée dans App.xaml.cs, pas besoin de la relancer
        }

        private void Jouer_Click(object sender, RoutedEventArgs e)
        {
            ChoixCone choix = new ChoixCone();
            choix.Show();
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
