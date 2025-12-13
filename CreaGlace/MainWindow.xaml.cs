using System.Windows;
using System.Windows.Navigation;

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
            //j'instance la fenetre suivante
            ChoixCone choix = new ChoixCone();
            //j'affiche la fenetre de choix
            choix.Show();
            //je cache le menu principal(au lieu de fermer c'est mieux)
            this.Hide();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options opt = new Options();
            //j'utilise ShowDialog() pour que l'utilisateur soit obligé de fermer
            opt.ShowDialog();
        }

        private void Quitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
