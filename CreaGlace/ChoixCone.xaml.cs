using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class ChoixCone : Window
    {
        // Numéro du cône choisi (0 = aucun)
        private int coneChoisi = 0;

        public ChoixCone()
        {
            InitializeComponent();
        }

        private void Cone_Click(object sender, RoutedEventArgs e)
        {
            // Récupération du bouton cliqué
            Button boutonClique = (Button)sender;

            // Récupération du numéro du cône via le Tag
            coneChoisi = int.Parse(boutonClique.Tag.ToString());

            // Réinitialisation des bordures
            ReinitialiserBordures();

            // Mise en évidence du bouton sélectionné
            Border bordure = (Border)boutonClique.Content;
            bordure.BorderThickness = new Thickness(4);
            bordure.BorderBrush = Brushes.DeepSkyBlue;

        }

        // Petite méthode pour remettre tout à zéro (facile à comprendre)
        private void ReinitialiserBordures()
        {
            Border1.BorderThickness = new Thickness(0);
            Border2.BorderThickness = new Thickness(0);
            Border3.BorderThickness = new Thickness(0);
            Border4.BorderThickness = new Thickness(0);
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            // Vérification simple (vu dans le cours page 7 sur les MessageBox)
            if (coneChoisi == 0)
            {
                MessageBox.Show("Choisis un cône avant de continuer !");
                return;
            }

            // On passe l'information à la fenêtre suivante via le constructeur
            // C'est de la Programmation Orientée Objet de base (Sequence 3)
            // Ouverture de la fenêtre suivante en passant le numéro du cône
            Regle fenetreRegle = new Regle(coneChoisi);
            fenetreRegle.Show();

            // On cache cette fenêtre
            this.Hide();
        }
    }
}