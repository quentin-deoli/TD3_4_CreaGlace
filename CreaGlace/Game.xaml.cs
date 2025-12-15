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
        // ====== OUTILS ======
        private Random alea = new Random();
        private MediaPlayer lecteurSon = new MediaPlayer();

        // ====== TIMER ======
        private DispatcherTimer timerJeu;

        // ====== JEU ======
        private Image bouleQuiTombe = null;
        private List<Image> pile = new List<Image>();

        private int score = 0;
        private int viesPerdues = 0;
        private const int NB_VIES_MAX = 3;

        private bool estEnPause = false;

        // ====== TEMPS ======
        private int tempsSecondes = 0;
        private int compteurMs = 0;
        private int compteurSpawnMs = 0;

        private int delaiSpawnMs = 1500;
        private double vitesseChute = 3;

        // ====== DEPLACEMENT ======
        private double vitesseCone = 15;

        // ====== CONE ======
        private int coneChoisi;

        // ================= CONSTRUCTEUR =================
        public Game(int coneChoisi)
        {
            InitializeComponent();

            this.coneChoisi = coneChoisi;
            ChargerCone();

            // ====== CHARGEMENT DU SON (BONNE MÉTHODE) ======
            string cheminSon = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Sons",
                "pop.mp3"
            );
            lecteurSon.Open(new Uri(cheminSon, UriKind.Absolute));

            // ====== TIMER ======
            timerJeu = new DispatcherTimer();
            timerJeu.Interval = TimeSpan.FromMilliseconds(20);
            timerJeu.Tick += TimerJeu_Tick;

            Loaded += Game_Loaded;
            KeyDown += Game_KeyDown;
        }

        // ================= CHARGEMENT =================
        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            PlacerConeAuCentre();

            score = 0;
            viesPerdues = 0;
            tempsSecondes = 0;
            compteurMs = 0;
            compteurSpawnMs = 0;

            txtScore.Text = "Score: 0";
            txtTemps.Text = "Temps: 00:00";

            timerJeu.Start();
            Focus();

        }

        // ================= CLAVIER =================
        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                estEnPause = !estEnPause;
                txtPause.Visibility = estEnPause ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            if (estEnPause) return;

            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
                x = Math.Max(0, x - vitesseCone);

            if (e.Key == Key.Right)
                x = Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, x + vitesseCone);

            Canvas.SetLeft(imgConeChoisi, x);

            foreach (Image b in pile)
            {
                double decalage = (imgConeChoisi.Width - b.Width) / 2;
                Canvas.SetLeft(b, x + decalage);
            }
        }

        // ================= BOUCLE DE JEU =================
        private void TimerJeu_Tick(object sender, EventArgs e)
        {
            if (estEnPause) return;

            // ---- TEMPS ----
            compteurMs += 20;
            if (compteurMs >= 1000)
            {
                compteurMs = 0;
                tempsSecondes++;
                AfficherTemps();

                vitesseChute += 0.1;
                if (delaiSpawnMs > 500)
                    delaiSpawnMs -= 15;
            }

            // ---- SPAWN ----
            compteurSpawnMs += 20;
            if (bouleQuiTombe == null && compteurSpawnMs >= delaiSpawnMs)
            {
                compteurSpawnMs = 0;
                bouleQuiTombe = CreerBoule();
            }

            // ---- CHUTE ----
            if (bouleQuiTombe != null)
            {
                RotateTransform rotation = (RotateTransform)bouleQuiTombe.RenderTransform;
                rotation.Angle += 5;

                double y = Canvas.GetTop(bouleQuiTombe) + vitesseChute;
                Canvas.SetTop(bouleQuiTombe, y);

                if (y >= canvasJeu.ActualHeight - bouleQuiTombe.Height)
                {
                    canvasJeu.Children.Remove(bouleQuiTombe);
                    bouleQuiTombe = null;

                    viesPerdues++;
                    if (viesPerdues >= NB_VIES_MAX)
                        FinDePartie();
                }
                else if (DetecterCollision(bouleQuiTombe))
                {
                    EmpilerBoule(bouleQuiTombe);
                    bouleQuiTombe = null;

                    score += 10;
                    txtScore.Text = "Score: " + score;

                    VerifierHauteurPile();
                }
            }
        }

        // ================= CREER BOULE =================
        private Image CreerBoule()
        {
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            boule.RenderTransformOrigin = new Point(0.5, 0.5);
            boule.RenderTransform = new RotateTransform(0);

            int numero = alea.Next(1, 6);
            string chemin = "Images/image" + numero + ".png";

            boule.Source = new BitmapImage(new Uri(chemin, UriKind.Relative));

            double x = alea.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60);

            canvasJeu.Children.Add(boule);
            return boule;
        }

        // ================= COLLISION =================
        private bool DetecterCollision(Image boule)
        {
            Image obstacle = (pile.Count == 0) ? imgConeChoisi : pile[pile.Count - 1];

            double bouleBas = Canvas.GetTop(boule) + boule.Height;
            double bouleGauche = Canvas.GetLeft(boule);
            double bouleDroite = bouleGauche + boule.Width;

            double obsHaut = Canvas.GetTop(obstacle);
            double obsGauche = Canvas.GetLeft(obstacle);
            double obsDroite = obsGauche + obstacle.Width;

            bool toucheHauteur = bouleBas >= obsHaut && bouleBas <= obsHaut + 15;
            bool toucheLargeur = bouleDroite > obsGauche && bouleGauche < obsDroite;

            return toucheHauteur && toucheLargeur;
        }

        // ================= EMPILER =================
        private void EmpilerBoule(Image boule)
        {
            double centreX = Canvas.GetLeft(imgConeChoisi)
                            + (imgConeChoisi.Width - boule.Width) / 2;

            double cibleY = (pile.Count == 0)
                ? Canvas.GetTop(imgConeChoisi) - boule.Height + 15
                : Canvas.GetTop(pile[pile.Count - 1]) - boule.Height + 15;

            Canvas.SetLeft(boule, centreX);
            Canvas.SetTop(boule, cibleY);

            pile.Add(boule);

            // ====== JOUER LE SON ======
            lecteurSon.Stop();
            lecteurSon.Position = TimeSpan.Zero;
            lecteurSon.Volume = App.VolumeSFX;
            lecteurSon.Play();
        }

        // ================= BONUS =================
        private const int HAUTEUR_MAX = 50;

        private void VerifierHauteurPile()
        {
            if (pile.Count == 0) return;

            Image bouleHaut = pile[pile.Count - 1];
            if (Canvas.GetTop(bouleHaut) <= HAUTEUR_MAX)
            {
                score += 100;
                txtScore.Text = "Score: " + score;

                foreach (Image b in pile)
                    canvasJeu.Children.Remove(b);

                pile.Clear();
            }
        }

        // ================= FIN =================
        private void FinDePartie()
        {
            timerJeu.Stop();

            int min = tempsSecondes / 60;
            int sec = tempsSecondes % 60;
            string temps = min.ToString("D2") + ":" + sec.ToString("D2");

            GameOver go = new GameOver(score, temps);
            go.Show();

            Close();
        }

        // ================= AFFICHAGE =================
        private void AfficherTemps()
        {
            int min = tempsSecondes / 60;
            int sec = tempsSecondes % 60;
            txtTemps.Text = "Temps: " + min.ToString("D2") + ":" + sec.ToString("D2");
        }

        private void PlacerConeAuCentre()
        {
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double y = canvasJeu.ActualHeight - imgConeChoisi.Height - 10;

            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, y);
        }

        private void ChargerCone()
        {
            string chemin = "";

            if (coneChoisi == 1) chemin = "Images/cone1.png";
            else if (coneChoisi == 2) chemin = "Images/cone2.png";
            else if (coneChoisi == 3) chemin = "Images/cone3.png";
            else if (coneChoisi == 4) chemin = "Images/cone4.png";

            imgConeChoisi.Source = new BitmapImage(new Uri(chemin, UriKind.Relative));
        }
    }
}