using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class Game : Window
    {
        private double coneSpeed = 10; // Vitesse de déplacement
        private int score = 0;          // Score du joueur

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            // Afficher le cône choisi
            if (coneImage != null)
                imgConeChoisi.Source = coneImage;

            // Position initiale en bas au centre après le rendu
            this.Loaded += (s, e) =>
            {
                double startX = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
                double startY = canvasJeu.ActualHeight - imgConeChoisi.Height;
                Canvas.SetLeft(imgConeChoisi, startX);
                Canvas.SetTop(imgConeChoisi, startY);
                this.Focus(); // Capturer les touches
            };

            // Événements
            this.KeyDown += Game_KeyDown;
            canvasJeu.SizeChanged += CanvasJeu_SizeChanged;
        }

        private void CanvasJeu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Maintenir le cône en bas si la fenêtre change
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - imgConeChoisi.Height);
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
            {
                x -= coneSpeed;
                if (x < 0) x = 0; // Bord gauche
            }
            else if (e.Key == Key.Right)
            {
                x += coneSpeed;
                if (x > canvasJeu.ActualWidth - imgConeChoisi.Width)
                    x = canvasJeu.ActualWidth - imgConeChoisi.Width; // Bord droit
            }

            Canvas.SetLeft(imgConeChoisi, x);
        }

        // Méthode pour ajouter des points au score
        public void AjouterScore(int points)
        {
            score += points;
            txtScore.Text = $"Score: {score}";
        }

        // Méthode pour remettre le score à zéro
        public void ResetScore()
        {
            score = 0;
            txtScore.Text = $"Score: {score}";
        }
    }
}
