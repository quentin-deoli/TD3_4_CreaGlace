using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class ChoixCone : Window
    {
        private int coneChoisi = 0;

        public ChoixCone()
        {
            InitializeComponent();
        }

        private void Cone_Click(object sender, RoutedEventArgs e)
        {
            Button boutonClique = (Button)sender;
            coneChoisi = int.Parse(boutonClique.Tag.ToString());
            ReinitialiserBordures();

            Border bordure = (Border)boutonClique.Content;
            bordure.BorderThickness = new Thickness(4);
            bordure.BorderBrush = Brushes.DeepSkyBlue;
        }

        private void ReinitialiserBordures()
        {
            Border1.BorderThickness = new Thickness(0);
            Border2.BorderThickness = new Thickness(0);
            Border3.BorderThickness = new Thickness(0);
            Border4.BorderThickness = new Thickness(0);
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            if (coneChoisi == 0)
            {
                MessageBox.Show("Choisis un cône avant de continuer !");
                return;
            }
            Regle fenetreRegle = new Regle(coneChoisi);
            fenetreRegle.Show();
            this.Hide();
        }
    }
}