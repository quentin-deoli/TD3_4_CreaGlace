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
        private Random random = new Random();

        private List<Image> boulesTombantes = new List<Image>();
        private List<Image> boulesEmpilees = new List<Image>();

        private DispatcherTimer moteurJeu;
        private DispatcherTimer timerProgression;

        private double coneSpeed = 10;
        private double bouleFallSpeed = 1.5;
        private int spawnDelay = 1800;
        private DateTime prochainSpawn;

        private double zoneSpawnOffset = 150;
        private DateTime debutPartie;
        private int boulesAuSol = 0;
        private int score = 0;
        private const int hauteurMax = 50;

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            if (coneImage != null)
                imgConeChoisi.Source = coneImage;

            this.Loaded += Game_Loaded;
            this.KeyDown += Game_KeyDown;
            this.SizeChanged += Game_SizeChanged;

            moteurJeu = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            moteurJeu.Tick += MoteurJeu_Tick;
            moteurJeu.Start();

            timerProgression = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timerProgression.Tick += TimerProgression_Tick;
            timerProgression.Start();

            debutPartie = DateTime.Now;
            prochainSpawn = DateTime.Now;
        }

        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            double startX = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double startY = canvasJeu.ActualHeight - imgConeChoisi.Height;
            Canvas.SetLeft(imgConeChoisi, startX);
            Canvas.SetTop(imgConeChoisi, startY);
            this.Focus();
        }

        private void Game_SizeChanged(object sender, SizeChangedEventArgs e)
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
                Canvas.SetLeft(boule, x + offsetX);
            }
        }

        private void TimerProgression_Tick(object sender, EventArgs e)
        {
            TimeSpan tempsEcoule = DateTime.Now - debutPartie;
            txtTemps.Text = $"Temps: {tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";

            bouleFallSpeed += 0.05;
            zoneSpawnOffset = Math.Min(350, zoneSpawnOffset + 10);
            spawnDelay = Math.Max(500, spawnDelay - 5);
            coneSpeed = 10 + (zoneSpawnOffset - 150) / 20.0;
        }

        private void MoteurJeu_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= prochainSpawn)
            {
                SpawnBoule();
                prochainSpawn = DateTime.Now.AddMilliseconds(spawnDelay);
            }

            for (int i = boulesTombantes.Count - 1; i >= 0; i--)
            {
                Image boule = boulesTombantes[i];
                double y = Canvas.GetTop(boule) + bouleFallSpeed;
                Canvas.SetTop(boule, y);

                if (y >= canvasJeu.ActualHeight - boule.Height)
                {
                    boulesAuSol++;
                    canvasJeu.Children.Remove(boule);
                    boulesTombantes.RemoveAt(i);

                    if (boulesAuSol >= 3)
                        FinDePartie();

                    continue;
                }

                if (VerifierCollision(boule))
                {
                    boulesTombantes.RemoveAt(i);
                    EmpilerSurPile(boule);
                    AjouterScore(10);
                    CheckHauteurMax();
                }
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

            double coneX = Canvas.GetLeft(imgConeChoisi);
            double zoneMin = Math.Max(0, coneX - zoneSpawnOffset);
            double zoneMax = Math.Min(canvasJeu.ActualWidth - boule.Width, coneX + zoneSpawnOffset);
            double x = random.Next((int)zoneMin, (int)zoneMax);

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -80);

            canvasJeu.Children.Add(boule);
            boulesTombantes.Add(boule);
        }

        private bool VerifierCollision(Image boule)
        {
            if (CollisionBas(boule, imgConeChoisi) && boulesEmpilees.Count == 0)
                return true;

            foreach (var b in boulesEmpilees)
                if (CollisionBas(boule, b)) return true;

            return false;
        }

        private bool CollisionBas(Image bouleQuiTombe, Image obstacle)
        {
            double ay = Canvas.GetTop(bouleQuiTombe) + bouleQuiTombe.Height;
            double by = Canvas.GetTop(obstacle);
            bool verticalOk = (ay >= by && ay <= by + 10);

            double ax = Canvas.GetLeft(bouleQuiTombe);
            double bx = Canvas.GetLeft(obstacle);
            bool horizontalOk = (ax + bouleQuiTombe.Width > bx && ax < bx + obstacle.Width);

            return verticalOk && horizontalOk;
        }

        private void EmpilerSurPile(Image boule)
        {
            int pixelsChevauchement = 15;
            double decalageX = (imgConeChoisi.Width - boule.Width) / 2;
            double posX = Canvas.GetLeft(imgConeChoisi) + decalageX;

            double referenceY = boulesEmpilees.Count == 0 ? Canvas.GetTop(imgConeChoisi) : Canvas.GetTop(boulesEmpilees[^1]);
            double posY = referenceY - boule.Height + pixelsChevauchement;

            Canvas.SetLeft(boule, posX);
            Canvas.SetTop(boule, posY);
            boulesEmpilees.Add(boule);
        }

        private void CheckHauteurMax()
        {
            if (boulesEmpilees.Count == 0) return;

            if (Canvas.GetTop(boulesEmpilees[^1]) <= hauteurMax)
            {
                foreach (var boule in boulesEmpilees)
                    canvasJeu.Children.Remove(boule);

                boulesEmpilees.Clear();
                AjouterScore(100);
            }
        }

        private void AjouterScore(int points)
        {
            score += points;
            txtScore.Text = $"Score: {score}";
        }

        private void FinDePartie()
        {
            moteurJeu.Stop();
            timerProgression.Stop();

            TimeSpan tempsEcoule = DateTime.Now - debutPartie;
            string tempsTexte = $"{tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";

            GameOver fenetreGameOver = new GameOver(score, tempsTexte);
            fenetreGameOver.Show();

            this.Close();
        }
    }
}
