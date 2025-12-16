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
        // ===== CONSTANTES =====
        const int INTERVAL_TIMER = 20;
        const int TICKS_PAR_SECONDE = 50;
        const int TICKS_APPARITION = 70;
        const int VITESSE_CHUTE = 5;
        const int VITESSE_DEPLACEMENT = 20;
        const int VIES_MAX = 3;

        // ===== OUTILS =====
        Random hasard = new Random();
        MediaPlayer lecteurSon = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();

        // ===== JEU =====
        Image bouleEnChute = null;
        List<Image> pileBoules = new List<Image>();

        int score = 0;
        int viesPerdues = 0;
        int numeroCone;
        bool enPause = false;

        // ===== TEMPS =====
        int secondesEcoulees = 0;
        int ticksTemps = 0;
        int ticksBoule = 0;

        // ================= CONSTRUCTEUR =================
        public Game(int choixDuJoueur)
        {
            InitializeComponent();

            numeroCone = choixDuJoueur;

            Loaded += DemarrerJeu;
            KeyDown += AppuiTouche;

            timer.Interval = TimeSpan.FromMilliseconds(INTERVAL_TIMER);
            timer.Tick += BoucleDuJeu;
        }

        // ================= DEMARRAGE =================
        void DemarrerJeu(object sender, RoutedEventArgs e)
        {
            imgConeChoisi.Source = new BitmapImage(
                new Uri("Images/cone" + numeroCone + ".png", UriKind.Relative));

            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - 100);

            timer.Start();
        }

        // ================= BOUCLE DU JEU =================
        void BoucleDuJeu(object sender, EventArgs e)
        {
            if (enPause) return;

            GererTemps();
            GererBoule();
        }

        // ================= TEMPS =================
        void GererTemps()
        {
            ticksTemps++;

            if (ticksTemps >= TICKS_PAR_SECONDE)
            {
                ticksTemps = 0;
                secondesEcoulees++;

                txtTemps.Text = "Temps : " +
                    (secondesEcoulees / 60).ToString("D2") + ":" +
                    (secondesEcoulees % 60).ToString("D2");
            }
        }

        // ================= BOULE =================
        void GererBoule()
        {
            ticksBoule++;

            if (bouleEnChute == null)
            {
                if (ticksBoule >= TICKS_APPARITION)
                {
                    ticksBoule = 0;
                    CreerNouvelleBoule();
                }
            }
            else
            {
                double y = Canvas.GetTop(bouleEnChute);
                Canvas.SetTop(bouleEnChute, y + VITESSE_CHUTE);

                if (TestCollision())
                {
                    AttraperBoule();
                }
                else if (y > canvasJeu.ActualHeight)
                {
                    PerdreVie();
                }
            }
        }

        // ================= CREATION =================
        void CreerNouvelleBoule()
        {
            bouleEnChute = new Image();
            bouleEnChute.Width = 60;
            bouleEnChute.Height = 60;

            int num = hasard.Next(1, 6);
            bouleEnChute.Source = new BitmapImage(new Uri("Images/image" + num + ".png", UriKind.Relative));

            double x = hasard.Next(0, (int)(canvasJeu.ActualWidth - bouleEnChute.Width));

            Canvas.SetLeft(bouleEnChute, x);
            Canvas.SetTop(bouleEnChute, -60);

            canvasJeu.Children.Add(bouleEnChute);
        }

        // ================= CLAVIER =================
        void AppuiTouche(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                enPause = !enPause;
               if (enPause == true)
                  {
                    txtPause.Visibility = Visibility.Visible;
                  }
               else
                  {
                    txtPause.Visibility = Visibility.Collapsed;
                  }
                return;
            }

            if (enPause) return;

            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
                x -= VITESSE_DEPLACEMENT;

            if (e.Key == Key.Right)
                x += VITESSE_DEPLACEMENT;

            if (x < 0) x = 0;
            if (x + imgConeChoisi.Width > canvasJeu.ActualWidth)
                x = canvasJeu.ActualWidth - imgConeChoisi.Width;

            Canvas.SetLeft(imgConeChoisi, x);

            foreach (Image boule in pileBoules)
            {
                Canvas.SetLeft(boule, x + 10);
            }
        }

        // ================= COLLISION =================
        bool TestCollision()
        {
            Image cible;

            if (pileBoules.Count == 0)
                cible = imgConeChoisi;
            else
                cible = pileBoules[pileBoules.Count - 1];

            double bouleBas = Canvas.GetTop(bouleEnChute) + bouleEnChute.Height;
            double bouleGauche = Canvas.GetLeft(bouleEnChute);
            double bouleDroite = bouleGauche + bouleEnChute.Width;

            double cibleHaut = Canvas.GetTop(cible);
            double cibleGauche = Canvas.GetLeft(cible);
            double cibleDroite = cibleGauche + cible.Width;

            bool toucheHauteur = bouleBas >= cibleHaut && bouleBas <= cibleHaut + 15;
            bool toucheLargeur = bouleDroite > cibleGauche && bouleGauche < cibleDroite;

            return toucheHauteur && toucheLargeur;
        }

        // ================= ACTIONS =================
        void AttraperBoule()
        {
            JouerLeSon("pop.mp3");

            score += 10;
            txtScore.Text = "Score : " + score;

            pileBoules.Add(bouleEnChute);

            Image support;
            if (pileBoules.Count == 1)
                support = imgConeChoisi;
            else
                support = pileBoules[pileBoules.Count - 2];

            Canvas.SetTop(bouleEnChute, Canvas.GetTop(support) - 40);

            bouleEnChute = null;

            VerifierBonus();
        }

        void PerdreVie()
        {
            JouerLeSon("fail.mp3");

            canvasJeu.Children.Remove(bouleEnChute);
            bouleEnChute = null;

            viesPerdues++;

            if (viesPerdues >= VIES_MAX)
            {
                timer.Stop();
                GameOver fin = new GameOver(score, txtTemps.Text);
                fin.Show();
                Close();
            }
        }

        void VerifierBonus()
        {
            if (pileBoules.Count == 0) return;

            if (Canvas.GetTop(pileBoules[pileBoules.Count - 1]) < 50)
            {
                score += 100;
                JouerLeSon("bonus.mp3");

                foreach (Image b in pileBoules)
                    canvasJeu.Children.Remove(b);

                pileBoules.Clear();
            }
        }

        void JouerLeSon(string fichier)
        {
            lecteurSon.Stop();
            lecteurSon.Open(new Uri("Sons/" + fichier, UriKind.Relative));
            lecteurSon.Play();
        }
    }
}
