using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CreaGlace
{
    public partial class Game : Window
    {
        private double coneSpeed = 10;
        private int score = 0;

        private DispatcherTimer spawnTimer;
        private DispatcherTimer speedUpTimer;

        private List<Image> boules = new List<Image>();
        private List<Image> boulesEmpilees = new List<Image>();

        private int boulesAuSol = 0;
        private double bouleFallSpeed = 2;

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            imgConeChoisi.Source = coneImage;

            Loaded += Game_Loaded;
            KeyDown += Game_KeyDown;
            canvasJeu.SizeChanged += CanvasJeu_SizeChanged;
        }

        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            // Position initiale du cône
            Canvas.SetLeft(imgConeChoisi, (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2);
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - imgConeChoisi.Height);

            // Timer d’apparition des boules
            spawnTimer = new DispatcherTimer();
            spawnTimer.Interval = TimeSpan.FromMilliseconds(1200);
            spawnTimer.Tick += SpawnBoule;
            spawnTimer.Start();

            // Timer pour accélérer l’apparition
            speedUpTimer = new DispatcherTimer();
            speedUpTimer.Interval = TimeSpan.FromSeconds(7);
            speedUpTimer.Tick += (s, ev) =>
            {
                if (spawnTimer.Interval.TotalMilliseconds > 350)
                    spawnTimer.Interval = TimeSpan.FromMilliseconds(spawnTimer.Interval.TotalMilliseconds - 150);
            };
            speedUpTimer.Start();
        }

        private void CanvasJeu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - imgConeChoisi.Height);
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
                x -= coneSpeed;
            else if (e.Key == Key.Right)
                x += coneSpeed;

            if (x < 0) x = 0;
            if (x > canvasJeu.ActualWidth - imgConeChoisi.Width)
                x = canvasJeu.ActualWidth - imgConeChoisi.Width;

            Canvas.SetLeft(imgConeChoisi, x);

            // Les boules empilées suivent le cornet
            foreach (var b in boulesEmpilees)
                Canvas.SetLeft(b, x + imgConeChoisi.Width / 2 - b.Width / 2);
        }

        // ---------------------------------------------------
        //                 SPAWN DE BOULES
        // ---------------------------------------------------
        private void SpawnBoule(object sender, EventArgs e)
        {
            Image boule = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/image1.png"))
            };

            canvasJeu.Children.Add(boule);

            double x = new Random().Next(0, (int)(canvasJeu.ActualWidth - boule.Width));
            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -50);

            boules.Add(boule);

            // Lancer animation
            CompositionTarget.Rendering += BoulesFall;
        }


        // ---------------------------------------------------
        //             CHUTE + COLLISIONS
        // ---------------------------------------------------
        private void BoulesFall(object sender, EventArgs e)
        {
            for (int i = boules.Count - 1; i >= 0; i--)
            {
                var b = boules[i];
                double y = Canvas.GetTop(b);
                y += bouleFallSpeed;
                Canvas.SetTop(b, y);

                // Touché le cône ?
                if (DetecteCollisionCone(b))
                {
                    EmpilerBoule(b);
                    boules.RemoveAt(i);
                    continue;
                }

                // Sol ?
                if (y > canvasJeu.ActualHeight - 50)
                {
                    canvasJeu.Children.Remove(b);
                    boules.RemoveAt(i);
                    boulesAuSol++;

                    if (boulesAuSol >= 3)
                        GameOver();

                    continue;
                }
            }
        }

        private bool DetecteCollisionCone(Image b)
        {
            double bouleX = Canvas.GetLeft(b) + b.Width / 2;
            double bouleY = Canvas.GetTop(b) + b.Height;

            double coneX = Canvas.GetLeft(imgConeChoisi);
            double coneY = Canvas.GetTop(imgConeChoisi);

            return (bouleX > coneX &&
                    bouleX < coneX + imgConeChoisi.Width &&
                    bouleY >= coneY);
        }

        // ---------------------------------------------------
        //                 EMPILAGE SANS ESPACE
        // ---------------------------------------------------
        private void EmpilerBoule(Image b)
        {
            canvasJeu.Children.Remove(b);

            Image copie = new Image
            {
                Width = b.Width,
                Height = b.Height,
                Source = b.Source
            };

            canvasJeu.Children.Add(copie);

            double coneX = Canvas.GetLeft(imgConeChoisi);
            double coneY = Canvas.GetTop(imgConeChoisi);

            double hauteurPile = boulesEmpilees.Count * 50; // pile compacte

            Canvas.SetLeft(copie, coneX + imgConeChoisi.Width / 2 - copie.Width / 2);
            Canvas.SetTop(copie, coneY - copie.Height - hauteurPile);

            boulesEmpilees.Add(copie);

            AjouterScore(10);
        }


        // ---------------------------------------------------
        //                     SCORE
        // ---------------------------------------------------
        public void AjouterScore(int points)
        {
            score += points;
            txtScore.Text = $"Score: {score}";
        }

        // ---------------------------------------------------
        //                   GAME OVER
        // ---------------------------------------------------
        private void GameOver()
        {
            spawnTimer.Stop();
            speedUpTimer.Stop();

            MessageBox.Show("GAME OVER !");
            Close();
        }
    }
}
