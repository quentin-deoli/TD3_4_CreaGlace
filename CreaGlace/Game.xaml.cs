using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CreaGlace
{
    public partial class Game : Window
    {
        private double coneSpeed = 10;
        private int score = 0;
        private Random random = new Random();
        private List<Image> boulesEmpilees = new List<Image>();
        private double bouleFallSpeed = 2;
        private int spawnDelay = 1500;
        private DateTime lastSpeedIncrease = DateTime.Now;
        private int boulesAuSol = 0;
        private const int hauteurMax = 50;

        // Variables pour le temps
        private DateTime debutPartie;
        private System.Windows.Threading.DispatcherTimer timer;

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            if (coneImage != null)
                imgConeChoisi.Source = coneImage;

            this.Loaded += (s, e) =>
            {
                double startX = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
                double startY = canvasJeu.ActualHeight - imgConeChoisi.Height;
                Canvas.SetLeft(imgConeChoisi, startX);
                Canvas.SetTop(imgConeChoisi, startY);
                this.Focus();
            };

            this.KeyDown += Game_KeyDown;
            canvasJeu.SizeChanged += CanvasJeu_SizeChanged;

            CompositionTarget.Rendering += UpdateBoules;

            StartSpawnBoules();

            // Initialiser le temps
            debutPartie = DateTime.Now;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void CanvasJeu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - imgConeChoisi.Height);
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left) x = Math.Max(0, x - coneSpeed);
            if (e.Key == Key.Right) x = Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, x + coneSpeed);

            Canvas.SetLeft(imgConeChoisi, x);

            foreach (var boule in boulesEmpilees)
            {
                double offsetX = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, Canvas.GetLeft(imgConeChoisi) + offsetX);
            }
        }

        // Met à jour le temps
        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan tempsEcoule = DateTime.Now - debutPartie;
            txtTemps.Text = $"Temps: {tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";
        }

        public void AjouterScore(int points)
        {
            score += points;
            txtScore.Text = $"Score: {score}";
        }

        private async void StartSpawnBoules()
        {
            while (true)
            {
                await Task.Delay(spawnDelay);
                SpawnBoule();
            }
        }

        private void SpawnBoule()
        {
            Image boule = new Image
            {
                Width = 60,
                Height = 60,
                Source = new BitmapImage(new Uri($"Images/image{random.Next(1, 6)}.png", UriKind.Relative))
            };

            double x = random.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));
            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -80);
            canvasJeu.Children.Add(boule);
        }

        private void UpdateBoules(object sender, EventArgs e)
        {
            var boules = canvasJeu.Children.OfType<Image>()
                .Where(img => img != imgConeChoisi)
                .Except(boulesEmpilees)
                .ToList();

            foreach (var boule in boules)
            {
                double y = Canvas.GetTop(boule) + bouleFallSpeed;
                Canvas.SetTop(boule, y);

                if (y >= canvasJeu.ActualHeight - boule.Height)
                {
                    boulesAuSol++;
                    canvasJeu.Children.Remove(boule);
                    if (boulesAuSol >= 3)
                    {
                        timer.Stop(); // Arrêter le timer
                        MessageBox.Show("Perdu ! 3 boules au sol.");
                    }
                    continue;
                }

                if (CollisionBas(boule, imgConeChoisi))
                {
                    EmpilerSurPile(boule);
                    AjouterScore(10);
                    CheckHauteurMax();
                    AudioManager.PlaySFX("cornetsfx.wav");
                    continue;
                }

                if (DetectSupport(boule))
                {
                    EmpilerSurPile(boule);
                    AjouterScore(10);
                    CheckHauteurMax();
                    AudioManager.PlaySFX("glacesfx.wav");
                    continue;
                }
            }

            if ((DateTime.Now - lastSpeedIncrease).TotalSeconds >= 7)
            {
                bouleFallSpeed += 0.7;
                spawnDelay = Math.Max(450, spawnDelay - 100);
                lastSpeedIncrease = DateTime.Now;
            }
        }

        private bool CollisionBas(Image a, Image b)
        {
            double ax = Canvas.GetLeft(a);
            double ay = Canvas.GetTop(a) + a.Height - 5;
            double bx = Canvas.GetLeft(b);
            double by = Canvas.GetTop(b);
            return ax < bx + b.Width && ax + a.Width > bx && ay >= by - 5 && ay <= by + 5;
        }

        private bool DetectSupport(Image boule)
        {
            return boulesEmpilees.Any(b => CollisionBas(boule, b));
        }

        private void EmpilerSurPile(Image boule)
        {
            double offsetX = (imgConeChoisi.Width - boule.Width) / 2;
            double posX = Canvas.GetLeft(imgConeChoisi) + offsetX;
            double posY = boulesEmpilees.Count == 0 ? Canvas.GetTop(imgConeChoisi) - boule.Height : Canvas.GetTop(boulesEmpilees.Last()) - boule.Height;
            Canvas.SetLeft(boule, posX);
            Canvas.SetTop(boule, posY);
            boulesEmpilees.Add(boule);
        }

        private void CheckHauteurMax()
        {
            if (!boulesEmpilees.Any()) return;
            if (Canvas.GetTop(boulesEmpilees.Last()) <= hauteurMax) JouerAnimationSuppression();
        }

        private async void JouerAnimationSuppression()
        {
            foreach (var boule in boulesEmpilees.AsEnumerable().Reverse())
            {
                DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                boule.BeginAnimation(OpacityProperty, fade);
                await Task.Delay(120);
                canvasJeu.Children.Remove(boule);
            }
            boulesEmpilees.Clear();
        }
    }
}
