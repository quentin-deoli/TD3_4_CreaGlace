using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CreaGlace
{
    public partial class Game : Window
    {
        private double coneSpeed = 10; // Vitesse de déplacement

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            // Position initiale en bas au centre
            imgConeChoisi.Source = coneImage;
            double startX = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double startY = canvasJeu.ActualHeight - imgConeChoisi.Height;
            Canvas.SetLeft(imgConeChoisi, startX);
            Canvas.SetTop(imgConeChoisi, startY);

            // Focus pour capturer les touches
            this.Loaded += (s, e) => this.Focus();
            this.KeyDown += Game_KeyDown;

            // Redimensionner le cône si la fenêtre change
            canvasJeu.SizeChanged += CanvasJeu_SizeChanged;
        }

        private void CanvasJeu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Repositionner le cône en bas si la fenêtre change
            double currentX = Canvas.GetLeft(imgConeChoisi);
            double currentY = Canvas.GetTop(imgConeChoisi);
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
    }
}
