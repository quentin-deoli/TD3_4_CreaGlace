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
        // ===== OUTILS =====
        Random hasard = new Random();

        // ===== SON =====
        MediaPlayer lecteurSon = new MediaPlayer();

        // ===== TIMER =====
        DispatcherTimer timer;

        // ===== JEU =====
        Image bouleEnChute = null;
        List<Image> pileBoules = new List<Image>();

        int score = 0;
        int viesPerdues = 0;
        const int VIES_MAX = 3;

        bool enPause = false;

        // ===== TEMPS =====
        int tempsSecondes = 0;
        int compteurTempsMs = 0;
        int compteurSpawnMs = 0;

        int delaiApparition = 1500;
        double vitesseChute = 3;

        // ===== DEPLACEMENT =====
        double vitesseDeplacement = 15;

        // ===== CONE =====
        int numeroCone;

        // ================= CONSTRUCTEUR =================
        public Game(int coneChoisi)
        {
            InitializeComponent();

            numeroCone = coneChoisi;
            ChargerImageCone();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += BoucleJeu;

            Loaded += ChargerJeu;
            KeyDown += GérerClavier;
        }

        // ================= SON =================
        void JouerSon(string nomSon)
        {
            
                lecteurSon.Stop();
                lecteurSon.Open(new Uri("Sons/" + nomSon, UriKind.Relative));
                lecteurSon.Play();
        }

        // ================= CHARGEMENT =================
        void ChargerJeu(object sender, RoutedEventArgs e)
        {
            PlacerConeCentre();

            score = 0;
            viesPerdues = 0;
            tempsSecondes = 0;
            compteurTempsMs = 0;
            compteurSpawnMs = 0;

            txtScore.Text = "Score : 0";
            txtTemps.Text = "Temps : 00:00";

            timer.Start();
            Focus();
        }

        // ================= CLAVIER =================
        void GérerClavier(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                enPause = !enPause;

                if (enPause)
                {
                    txtPause.Visibility = Visibility.Visible;
                }
                else
                {
                    txtPause.Visibility = Visibility.Collapsed;
                }

                JouerSon("pause.mp3");
                return;
            }

            if (enPause) return;

            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
                x -= vitesseDeplacement;

            if (e.Key == Key.Right)
                x += vitesseDeplacement;

            x = Math.Max(0, Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, x));
            Canvas.SetLeft(imgConeChoisi, x);

            foreach (Image boule in pileBoules)
            {
                double decalage = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, x + decalage);
            }
        }

        // ================= BOUCLE DE JEU =================
        void BoucleJeu(object sender, EventArgs e)
        {
            if (enPause) return;

            GererTemps();
            GererApparition();
            GererChute();
        }

        // ================= TEMPS =================
        void GererTemps()
        {
            compteurTempsMs += 20;

            if (compteurTempsMs >= 1000)
            {
                compteurTempsMs = 0;
                tempsSecondes++;

                AfficherTemps();

                vitesseChute += 0.1;
                if (delaiApparition > 500)
                    delaiApparition -= 15;
            }
        }

        // ================= APPARITION =================
        void GererApparition()
        {
            compteurSpawnMs += 20;

            if (bouleEnChute == null && compteurSpawnMs >= delaiApparition)
            {
                compteurSpawnMs = 0;
                bouleEnChute = CreerBoule();
            }
        }

        // ================= CHUTE =================
        void GererChute()
        {
            if (bouleEnChute == null) return;

            double y = Canvas.GetTop(bouleEnChute) + vitesseChute;
            Canvas.SetTop(bouleEnChute, y);

            if (y >= canvasJeu.ActualHeight - bouleEnChute.Height)
            {
                JouerSon("fail.mp3");
                RaterBoule();
            }
            else if (CollisionDetectee(bouleEnChute))
            {
                JouerSon("pop.mp3");
                AttraperBoule();
            }
        }

        // ================= CREATION BOULE =================
        Image CreerBoule()
        {
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            int numeroImage = hasard.Next(1, 6);
            boule.Source = new BitmapImage(
                new Uri("Images/image" + numeroImage + ".png", UriKind.Relative));

            double x = hasard.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60);

            canvasJeu.Children.Add(boule);
            return boule;
        }

        // ================= COLLISION =================
        bool CollisionDetectee(Image boule)
        {
            Image cible;

            if (pileBoules.Count == 0)
            {
                cible = imgConeChoisi;
            }
            else
            {
                cible = pileBoules[pileBoules.Count - 1];
            }

            double bouleBas = Canvas.GetTop(boule) + boule.Height;
            double bouleGauche = Canvas.GetLeft(boule);
            double bouleDroite = bouleGauche + boule.Width;

            double cibleHaut = Canvas.GetTop(cible);
            double cibleGauche = Canvas.GetLeft(cible);
            double cibleDroite = cibleGauche + cible.Width;

            return bouleBas >= cibleHaut && bouleBas <= cibleHaut + 15 && bouleDroite > cibleGauche && bouleGauche < cibleDroite;
        }

        // ================= ATTRAPER =================
        void AttraperBoule()
        {
            EmpilerBoule(bouleEnChute);
            bouleEnChute = null;

            score += 10;
            txtScore.Text = "Score : " + score;

            VerifierBonus();
        }

        // ================= RATÉ =================
        void RaterBoule()
        {
            canvasJeu.Children.Remove(bouleEnChute);
            bouleEnChute = null;

            viesPerdues++;

            if (viesPerdues >= VIES_MAX)
            {
                JouerSon("gameover.mp3");
                FinJeu();
            }
        }

        // ================= EMPILEMENT =================
        void EmpilerBoule(Image boule)
        {
            double x = Canvas.GetLeft(imgConeChoisi) + (imgConeChoisi.Width - boule.Width) / 2;

            double y;

            if (pileBoules.Count == 0)
            {
                y = Canvas.GetTop(imgConeChoisi) - boule.Height + 15;
            }
            else
            {
                y = Canvas.GetTop(pileBoules[pileBoules.Count - 1]) - boule.Height + 15;
            }

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, y);

            pileBoules.Add(boule);
        }

        // ================= BONUS =================
        void VerifierBonus()
        {
            if (pileBoules.Count == 0) return;

            if (Canvas.GetTop(pileBoules[pileBoules.Count - 1]) <= 50)
            {
                score += 100;
                txtScore.Text = "Score : " + score;

                JouerSon("bonus.mp3");

                foreach (Image b in pileBoules)
                    canvasJeu.Children.Remove(b);

                pileBoules.Clear();
            }
        }

        // ================= FIN DE JEU =================
        void FinJeu()
        {
            timer.Stop();

            int minutes = tempsSecondes / 60;
            int secondes = tempsSecondes % 60;
            string temps = minutes.ToString("D2") + ":" + secondes.ToString("D2");

            GameOver fin = new GameOver(score, temps);
            fin.Show();
            Close();
        }

        // ================= AFFICHAGE =================
        void AfficherTemps()
        {
            int minutes = tempsSecondes / 60;
            int secondes = tempsSecondes % 60;
            txtTemps.Text = "Temps : " + minutes.ToString("D2") + ":" + secondes.ToString("D2");
        }

        void PlacerConeCentre()
        {
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double y = canvasJeu.ActualHeight - imgConeChoisi.Height - 10;

            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, y);
        }

        void ChargerImageCone()
        {
            imgConeChoisi.Source = new BitmapImage(
                new Uri("Images/cone" + numeroCone + ".png", UriKind.Relative));
        }
    }
}
