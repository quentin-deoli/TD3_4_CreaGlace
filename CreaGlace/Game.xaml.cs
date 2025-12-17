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
        int vitesseChute = 5;
        int vitesseCone = 20;

        const int METTRE_A_JOUR = 20;
        const int TICKS_PAR_SECONDE = 50;
        const int TICKS_APPARITION = 70;
        const int VIES_MAX = 3;

        Random hasard = new Random();
        MediaPlayer lecteurSon = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();

        Image bouleEnChute = null;
        Image[] tableauBoules = new Image[4];

        int boulesSurCone = 0;
        int totalBoules = 0;
        int score = 0;
        int viesPerdues = 0;
        int numeroCone;
        bool enPause = false;

        int secondesEcoulees = 0;
        int ticksTemps = 0;
        int ticksBoule = 0;

        public Game(int choixDuJoueur)
        {
            InitializeComponent();
            numeroCone = choixDuJoueur;

            Loaded += DemarrerJeu;
            KeyDown += AppuiTouche;

            timer.Interval = TimeSpan.FromMilliseconds(METTRE_A_JOUR);
            timer.Tick += BoucleDuJeu;
        }

        void DemarrerJeu(object sender, RoutedEventArgs e)
        {
            imgConeChoisi.Source = new BitmapImage(new Uri("Images/cone" + numeroCone + ".png", UriKind.Relative));

            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - 100);

            timer.Start();
        }

        void BoucleDuJeu(object sender, EventArgs e)
        {
            if (enPause) return;
            GererTemps();
            GererBoule();
        }

        void GererTemps()
        {
            ticksTemps++;
            if (ticksTemps >= TICKS_PAR_SECONDE)
            {
                ticksTemps = 0;
                secondesEcoulees++;
                txtTemps.Text = "Temps : " + (secondesEcoulees / 60).ToString("D2") + ":" + (secondesEcoulees % 60).ToString("D2");
            }
        }

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
                Canvas.SetTop(bouleEnChute, y + vitesseChute);

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

        void AppuiTouche(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                enPause = !enPause;
                JouerLeSon("pause.mp3");
                if (enPause) txtPause.Visibility = Visibility.Visible;
                else txtPause.Visibility = Visibility.Collapsed;
                return;
            }

            if (enPause) return;

            double x = Canvas.GetLeft(imgConeChoisi);
            if (e.Key == Key.Left) x -= vitesseCone;
            if (e.Key == Key.Right) x += vitesseCone;

            if (x < 0) x = 0;
            if (x + imgConeChoisi.Width > canvasJeu.ActualWidth)
                x = canvasJeu.ActualWidth - imgConeChoisi.Width;

            Canvas.SetLeft(imgConeChoisi, x);

            for (int i = 0; i < boulesSurCone; i++)
            {
                Canvas.SetLeft(tableauBoules[i], x + 10);
            }
        }

        bool TestCollision()
        {
            Image cible;
            if (boulesSurCone == 0) cible = imgConeChoisi;
            else cible = tableauBoules[boulesSurCone - 1];

            double bouleBas = Canvas.GetTop(bouleEnChute) + bouleEnChute.Height;
            double bouleGauche = Canvas.GetLeft(bouleEnChute);
            double bouleDroite = bouleGauche + bouleEnChute.Width;

            double cibleHaut = Canvas.GetTop(cible);
            double cibleGauche = Canvas.GetLeft(cible);
            double cibleDroite = cibleGauche + cible.Width;

            return (bouleBas >= cibleHaut && bouleBas <= cibleHaut + 15) && (bouleDroite > cibleGauche && bouleGauche < cibleDroite);
        }

        void AttraperBoule()
        {
            JouerLeSon("pop.mp3");
            score += 10;
            txtScore.Text = "Score : " + score;

            tableauBoules[boulesSurCone] = bouleEnChute;
            double nouvelleHauteur = 270 - (boulesSurCone * 50);
            Canvas.SetTop(bouleEnChute, nouvelleHauteur);

            boulesSurCone++;
            totalBoules++;

            if (boulesSurCone == 4)
            {
                JouerLeSon("bonus.mp3");
                score += 50;
                txtScore.Text = "Score : " + score;
                for (int i = 0; i < 4; i++)
                {
                    canvasJeu.Children.Remove(tableauBoules[i]);
                    tableauBoules[i] = null;
                }
                boulesSurCone = 0;
            }

            if (totalBoules >= 20)
            {
                timer.Stop();
                string chrono = (secondesEcoulees / 60).ToString("D2") + ":" + (secondesEcoulees % 60).ToString("D2");
                Gagne fenetreVictoire = new Gagne(score, chrono);
                fenetreVictoire.Show();
                this.Close();
                return;
            }

            bouleEnChute = null;
        }

        void PerdreVie()
        {
            JouerLeSon("fail.mp3");
            canvasJeu.Children.Remove(bouleEnChute);
            bouleEnChute = null;
            viesPerdues++;

            if (viesPerdues >= VIES_MAX)
            {
                JouerLeSon("gameover.mp3");
                timer.Stop();
                string chrono = (secondesEcoulees / 60).ToString("D2") + ":" + (secondesEcoulees % 60).ToString("D2");
                GameOver fin = new GameOver(score, chrono);
                fin.Show();
                Close();
            }
        }

        private void BoutonCheat_Click(object sender, RoutedEventArgs e)
        {
            score += 500;
            txtScore.Text = "Score : " + score;
            vitesseCone = 65;
            txtScore.Foreground = Brushes.Red;
            this.Focus();
        }

        void JouerLeSon(string fichier)
        {
            lecteurSon.Stop();
            lecteurSon.Open(new Uri("Sons/" + fichier, UriKind.Relative));
            lecteurSon.Play();
        }
    }
}