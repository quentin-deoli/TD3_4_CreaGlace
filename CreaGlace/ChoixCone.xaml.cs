using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CreaGlace
{
    public partial class ChoixCone : Window
    {
        // On stocke l'image choisie pour la passer à la fenêtre suivante
        private ImageSource imageChoisie = null;

        public ChoixCone()
        {
            InitializeComponent();
        }

        private void Cone_Click(object sender, RoutedEventArgs e)
        {
            //On récupère le bouton cliqué
            Button boutonClique = (Button)sender;

            // On récupère la bordure et l'image à l'intérieur du bouton
            Border bordureActuelle = (Border)boutonClique.Content;
            Image imageDansLeBouton = (Image)bordureActuelle.Child;

            //On réinitialise toutes les bordures (on efface la sélection précédente)
            ReinitialiserBordures();

            //On met en valeur le cône cliqué (Bordure bleue épaisse)
            bordureActuelle.BorderThickness = new Thickness(4);
            bordureActuelle.BorderBrush = Brushes.DeepSkyBlue;

            //On sauvegarde l'image source
            imageChoisie = imageDansLeBouton.Source;
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
            if (imageChoisie == null)
            {
                MessageBox.Show("Choisis un cône avant de continuer !");
                return;
            }

            // On passe l'information à la fenêtre suivante via le constructeur
            // C'est de la Programmation Orientée Objet de base (Sequence 3)
            Regle fenetreRegle = new Regle(imageChoisie);

            fenetreRegle.Show();

            // On cache cette fenêtre
            this.Hide();
        }
    }
}