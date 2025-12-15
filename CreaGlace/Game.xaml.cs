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

        // ====== JEU ======
        private DispatcherTimer timerJeu;

        private Image bouleQuiTombe = null;              // 1 seule boule qui tombe (plus simple)
        private List<Image> pile = new List<Image>();    // boules empilées

        private int score = 0;
        private int viesPerdues = 0;
        private const int NB_VIES_MAX = 3;

        private bool estEnPause = false;

        // ====== TEMPS + DIFFICULTÉ ======
        private int tempsSecondes = 0;
        private int compteurMs = 0;          // sert à faire "1 seconde" avec le timer
        private int compteurSpawnMs = 0;     // sert à savoir quand créer une nouvelle boule

        private int delaiSpawnMs = 1500;     // temps entre 2 boules
        private double vitesseChute = 3;     // vitesse de chute

        // ====== DEPLACEMENT ======
        private double vitesseCone = 15;

        // ====== CONE ======
        private int coneChoisi;

        public Game(int coneChoisi)
        {
            InitializeComponent();

            this.coneChoisi = coneChoisi;
            ChargerCone();

            // --- SON : CHARGEMENT ---
            // On lui dit où chercher le fichier. UriKind.Relative est important !
            // Assure-toi que le nom du fichier est EXACTEMENT le même (majuscules/minuscules)
            lecteurSon.Open(new Uri("Sons/pop.mp3", UriKind.Relative));

            // Un seul timer pour TOUT (moteur du jeu)
            timerJeu = new DispatcherTimer();
            timerJeu.Interval = TimeSpan.FromMilliseconds(20); // 50 fps environ
            timerJeu.Tick += TimerJeu_Tick;

            Loaded += Game_Loaded;
            KeyDown += Game_KeyDown;
        }

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

        // ====== CLAVIER ======
        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            // Pause avec P
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

            // La pile suit le cône (on recentre juste en X)
            foreach (Image b in pile)
            {
                double decalage = (imgConeChoisi.Width - b.Width) / 2;
                Canvas.SetLeft(b, x + decalage);
            }
        }

        // ====== BOUCLE DE JEU ======
        private void TimerJeu_Tick(object sender, EventArgs e)
        {
            if (estEnPause) return;

            // 1) Gestion du temps (on cumule les ms)
            compteurMs += 20;
            if (compteurMs >= 1000)
            {
                compteurMs = 0;
                tempsSecondes++;
                AfficherTemps();

                // difficulté progressive simple
                vitesseChute += 0.1;
                if (delaiSpawnMs > 500)
                    delaiSpawnMs -= 15;
            }

            // 2) Gestion du spawn (créer une boule si besoin)
            compteurSpawnMs += 20;
            if (bouleQuiTombe == null && compteurSpawnMs >= delaiSpawnMs)
            {
                compteurSpawnMs = 0;
                bouleQuiTombe = CreerBoule();
            }

            // 3) Faire tomber la boule
            if (bouleQuiTombe != null)
            {
                // --- ROTATION : ACTION ---
                // On récupère la transformation qu'on a créée plus haut
                // On utilise (RotateTransform) pour dire : "Traite ça comme une Rotation"
                RotateTransform laRotation = (RotateTransform)bouleQuiTombe.RenderTransform;

                // On augmente l'angle de 5 degrés à chaque tick du timer
                laRotation.Angle += 5;
                // -------------------------

                double y = Canvas.GetTop(bouleQuiTombe);
                y += vitesseChute;
                Canvas.SetTop(bouleQuiTombe, y);

                // raté (touché le bas)
                if (y >= canvasJeu.ActualHeight - bouleQuiTombe.Height)
                {
                    canvasJeu.Children.Remove(bouleQuiTombe);
                    bouleQuiTombe = null;

                    viesPerdues++;
                    if (viesPerdues >= NB_VIES_MAX)
                    {
                        FinDePartie();
                        return;
                    }
                }
                else
                {
                    // collision -> empiler
                    if (DetecterCollisionAvecObstacle(bouleQuiTombe))
                    {
                        EmpilerBoule(bouleQuiTombe);
                        bouleQuiTombe = null;

                        score += 10;
                        txtScore.Text = "Score: " + score;

                        VerifierHauteurPile();
                    }
                }
            }
        }

        // ====== CREER UNE BOULE ======
        private Image CreerBoule()
        {
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            // --- ROTATION : PRÉPARATION ---
            // On définit le point de pivot au centre de l'image (0.5, 0.5)
            boule.RenderTransformOrigin = new Point(0.5, 0.5);
            // On lui attribue une transformation de rotation qui commence à 0 degré
            boule.RenderTransform = new RotateTransform(0);
            // ------------------------------

            int numero = alea.Next(1, 6);
            string chemin = "Images/image" + numero + ".png";

            boule.Source = new BitmapImage(new Uri(chemin, UriKind.Relative));

            double x = alea.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60);

            canvasJeu.Children.Add(boule);
            return boule;
        }

        // ====== COLLISION SIMPLE ======
        private bool DetecterCollisionAvecObstacle(Image boule)
        {
            Image obstacle = (pile.Count == 0) ? imgConeChoisi : pile[pile.Count - 1];

            double bouleBas = Canvas.GetTop(boule) + boule.Height;
            double bouleGauche = Canvas.GetLeft(boule);
            double bouleDroite = bouleGauche + boule.Width;

            double obsHaut = Canvas.GetTop(obstacle);
            double obsGauche = Canvas.GetLeft(obstacle);
            double obsDroite = obsGauche + obstacle.Width;

            bool toucheHauteur = (bouleBas >= obsHaut && bouleBas <= obsHaut + 15);
            bool toucheLargeur = (bouleDroite > obsGauche && bouleGauche < obsDroite);

            return toucheHauteur && toucheLargeur;
        }

        // ====== EMPILER ======
        private void EmpilerBoule(Image boule)
        {
            double coneX = Canvas.GetLeft(imgConeChoisi);
            double centreX = coneX + (imgConeChoisi.Width - boule.Width) / 2;

            double cibleY;
            if (pile.Count == 0)
                cibleY = Canvas.GetTop(imgConeChoisi) - boule.Height + 15;
            else
                cibleY = Canvas.GetTop(pile[pile.Count - 1]) - boule.Height + 15;

            Canvas.SetLeft(boule, centreX);
            Canvas.SetTop(boule, cibleY);

            pile.Add(boule);
            // --- SON : JOUER ---
            // 1. On rembobine le son au début (sinon il ne joue qu'une fois)
            lecteurSon.Position = TimeSpan.Zero;

            // 2. On règle le volume selon ce que le joueur a choisi dans les Options
            lecteurSon.Volume = App.VolumeSFX;

            // 3. On lance le son
            lecteurSon.Play();
            // -------------------
        }

        // ====== BONUS HAUTEUR (tu peux enlever si tu veux encore + simple) ======
        private const int HAUTEUR_MAX = 50;

        private void VerifierHauteurPile()
        {
            if (pile.Count == 0) return;

            Image bouleDuHaut = pile[pile.Count - 1];
            if (Canvas.GetTop(bouleDuHaut) <= HAUTEUR_MAX)
            {
                score += 100;
                txtScore.Text = "Score: " + score;

                // On enlève la pile
                foreach (Image b in pile)
                    canvasJeu.Children.Remove(b);

                pile.Clear();
            }
        }

        // ====== FIN ======
        private void FinDePartie()
        {
            timerJeu.Stop();

            int minutes = tempsSecondes / 60;
            int secondes = tempsSecondes % 60;
            string tempsStr = minutes.ToString("D2") + ":" + secondes.ToString("D2");

            GameOver go = new GameOver(score, tempsStr);
            go.Show();

            Close();
        }

        // ====== OUTILS D'AFFICHAGE ======
        private void AfficherTemps()
        {
            int minutes = tempsSecondes / 60;
            int secondes = tempsSecondes % 60;
            txtTemps.Text = "Temps: " + minutes.ToString("D2") + ":" + secondes.ToString("D2");
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

            // Version simple (comme Regle)
            imgConeChoisi.Source = new BitmapImage(new Uri(chemin, UriKind.Relative));
        }
    }
}
