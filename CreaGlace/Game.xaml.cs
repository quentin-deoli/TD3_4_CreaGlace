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
        const int METTRE_A_JOUR = 20;
        const int TICKS_PAR_SECONDE = 50;
        const int TICKS_APPARITION = 70;
        const int VITESSE_CHUTE = 6;
        const int VITESSE_DEPLACEMENT = 25;
        const int VIES_MAX = 3;

        Random hasard = new Random();
        MediaPlayer lecteurSon = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();

        Image bouleEnChute = null;

        // MODIF 1 : Tableau réduit à 4 places (car on vide après 4)
        Image[] tableauBoules = new Image[4];

        int boulesSurCone = 0; // Compteur pour l'empilement (0 à 4)
        int totalBoules = 0;   // Compteur pour la victoire (0 à 50)

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

                if (enPause == true) txtPause.Visibility = Visibility.Visible;
                else txtPause.Visibility = Visibility.Collapsed;

                return;
            }

            if (enPause) return;

            double x = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left) x -= VITESSE_DEPLACEMENT;
            if (e.Key == Key.Right) x += VITESSE_DEPLACEMENT;

            if (x < 0) x = 0;
            if (x + imgConeChoisi.Width > canvasJeu.ActualWidth)
                x = canvasJeu.ActualWidth - imgConeChoisi.Width;

            Canvas.SetLeft(imgConeChoisi, x);

            // On déplace uniquement les boules qui sont sur le cône (variable boulesSurCone)
            for (int i = 0; i < boulesSurCone; i++)
            {
                Canvas.SetLeft(tableauBoules[i], x + 10);
            }
        }

        bool TestCollision()
        {
            Image cible;

            if (boulesSurCone == 0)
            {
                cible = imgConeChoisi;
            }
            else
            {
                cible = tableauBoules[boulesSurCone - 1];
            }

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

        void AttraperBoule()
        {
            JouerLeSon("pop.mp3");
            score += 10;
            txtScore.Text = "Score : " + score;

            // On stocke la boule dans le petit tableau (de 0 à 3)
            tableauBoules[boulesSurCone] = bouleEnChute;

            // Placement visuel
            double nouvelleHauteur = 270 - (boulesSurCone * 50);
            Canvas.SetTop(bouleEnChute, nouvelleHauteur);

            // On incrémente les deux compteurs
            boulesSurCone++;
            totalBoules++; // Celui-ci ne revient jamais à 0, il compte jusqu'à 50

            // MODIF 2 : Gestion de la boucle de 4
            // Si le cône est plein (4 boules)
            if (boulesSurCone == 4)
            {
                JouerLeSon("bonus.mp3"); // Petit son de validation
                score += 50; // Bonus de points
                txtScore.Text = "Score : " + score;

                // On vide le visuel (on retire les 4 images de l'écran)
                for (int i = 0; i < 4; i++)
                {
                    canvasJeu.Children.Remove(tableauBoules[i]);
                    tableauBoules[i] = null;
                }

                // On remet le compteur du cône à 0 (mais PAS le totalBoules !)
                boulesSurCone = 0;
            }

            // MODIF 3 : Victoire basée sur le TOTAL
            if (totalBoules >= 50)
            {
                timer.Stop();
                Gagne fenetreVictoire = new Gagne(score, txtTemps.Text);
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
                GameOver fin = new GameOver(score, txtTemps.Text);
                fin.Show();
                Close();
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