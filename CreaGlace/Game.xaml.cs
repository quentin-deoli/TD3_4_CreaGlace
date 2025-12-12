using System;
using System.Collections.Generic;
using System.Linq;
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
        // --- 1. VARIABLES ---
        private Random random = new Random();

        // Listes pour gérer les objets
        private List<Image> boulesTombantes = new List<Image>();
        private List<Image> boulesEmpilees = new List<Image>();

        // Timers
        private DispatcherTimer moteurJeu;       // Gère le mouvement (60 fois par seconde)
        private DispatcherTimer timerProgression; // Gère le temps (1 fois par seconde)

        // Paramètres de jeu (Valeurs originales)
        private double coneSpeed = 10;
        private double bouleFallSpeed = 1.5; // Vitesse initiale lente
        private int spawnDelay = 1800;       // Délai initial (en millisecondes)
        private DateTime prochainSpawn;      // Pour savoir quand faire apparaitre la prochaine

        // Progression
        private double zoneSpawnOffset = 150;
        private DateTime debutPartie;
        private int boulesAuSol = 0;
        private int score = 0;
        private const int hauteurMax = 50; // Distance du haut de l'écran

        public Game(ImageSource coneImage)
        {
            InitializeComponent();

            // Image du cône
            if (coneImage != null)
                imgConeChoisi.Source = coneImage;

            // Événements
            this.Loaded += Game_Loaded;
            this.KeyDown += Game_KeyDown;
            this.SizeChanged += Game_SizeChanged;

            // Initialisation du MOTEUR PHYSIQUE (boucle rapide ~60 FPS)
            moteurJeu = new DispatcherTimer();
            moteurJeu.Interval = TimeSpan.FromMilliseconds(16);
            moteurJeu.Tick += MoteurJeu_Tick;
            moteurJeu.Start();

            // Initialisation du TIMER PROGRESSION (boucle lente 1 seconde)
            timerProgression = new DispatcherTimer();
            timerProgression.Interval = TimeSpan.FromSeconds(1);
            timerProgression.Tick += TimerProgression_Tick;
            timerProgression.Start();

            // Initialisation du temps
            debutPartie = DateTime.Now;
            prochainSpawn = DateTime.Now; // La première boule arrive tout de suite
        }

        // --- 2. CONFIGURATION ET TOUCHES ---

        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            // Centrer le cône
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

            // Déplacer toute la pile avec le cône
            foreach (var boule in boulesEmpilees)
            {
                double offsetX = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, x + offsetX);
            }
        }

        // --- 3. PROGRESSION (Toutes les secondes) ---

        private void TimerProgression_Tick(object sender, EventArgs e)
        {
            // Affichage Temps
            TimeSpan tempsEcoule = DateTime.Now - debutPartie;
            txtTemps.Text = $"Temps: {tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";

            // LOGIQUE DE PROGRESSION ORIGINALE
            bouleFallSpeed += 0.05; // Les boules accélèrent un peu
            zoneSpawnOffset = Math.Min(350, zoneSpawnOffset + 10); // La zone s'élargit

            // Le délai réduit (mais pas en dessous de 500ms)
            spawnDelay = Math.Max(500, spawnDelay - 5);

            // Le cône accélère un peu pour suivre le rythme
            coneSpeed = 10 + (zoneSpawnOffset - 150) / 20.0;
        }

        // --- 4. MOTEUR DU JEU (60 fois par seconde) ---

        private void MoteurJeu_Tick(object sender, EventArgs e)
        {
            // A. GESTION DU SPAWN (APPARITION)
            // On vérifie si l'heure actuelle a dépassé l'heure prévue pour le prochain spawn
            if (DateTime.Now >= prochainSpawn)
            {
                SpawnBoule();
                // On programme la prochaine apparition dans "spawnDelay" millisecondes
                prochainSpawn = DateTime.Now.AddMilliseconds(spawnDelay);
            }

            // B. DEPLACEMENT DES BOULES
            // On boucle à l'envers pour pouvoir supprimer des éléments proprement
            for (int i = boulesTombantes.Count - 1; i >= 0; i--)
            {
                Image boule = boulesTombantes[i];

                // 1. Chute
                double y = Canvas.GetTop(boule) + bouleFallSpeed;
                Canvas.SetTop(boule, y);

                // 2. Vérifier si touche le sol (PERDU)
                if (y >= canvasJeu.ActualHeight - boule.Height)
                {
                    boulesAuSol++;
                    canvasJeu.Children.Remove(boule);
                    boulesTombantes.RemoveAt(i);

                    if (boulesAuSol >= 3)
                    {
                        FinDePartie();      // Mansour CHANGE
                    }
                    continue;
                }

                // 3. Vérifier collision (GAGNÉ / EMPILÉ)
                if (VerifierCollision(boule))
                {
                    boulesTombantes.RemoveAt(i); // Elle ne tombe plus
                    EmpilerSurPile(boule);       // Elle se fixe
                    AjouterScore(10);

                    // Vérifier si on a atteint le haut
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

        // --- 5. LOGIQUE COLLISIONS ET PILE ---

        private bool VerifierCollision(Image boule)
        {
            // Collision avec le Cône
            if (CollisionBas(boule, imgConeChoisi) && boulesEmpilees.Count == 0)
                return true;

            // Collision avec la pile existante
            foreach (var b in boulesEmpilees)
            {
                if (CollisionBas(boule, b)) return true;
            }
            return false;
        }

        private bool CollisionBas(Image bouleQuiTombe, Image obstacle)
        {
            // Logique simplifiée avec Rect (Rectangle)
            // On regarde si le bas de la boule touche le haut de l'obstacle
            double ay = Canvas.GetTop(bouleQuiTombe) + bouleQuiTombe.Height;
            double by = Canvas.GetTop(obstacle);

            // Tolérance verticale de 10 pixels
            bool verticalOk = (ay >= by && ay <= by + 10);

            // Chevauchement horizontal
            double ax = Canvas.GetLeft(bouleQuiTombe);
            double bx = Canvas.GetLeft(obstacle);
            bool horizontalOk = (ax + bouleQuiTombe.Width > bx && ax < bx + obstacle.Width);

            return verticalOk && horizontalOk;
        }

        private void EmpilerSurPile(Image boule)
        {
            // TA METHODE DE SUPERPOSITION
            int pixelsChevauchement = 15;
            double decalageX = (imgConeChoisi.Width - boule.Width) / 2;
            double posX = Canvas.GetLeft(imgConeChoisi) + decalageX;

            double referenceY;
            if (boulesEmpilees.Count == 0)
                referenceY = Canvas.GetTop(imgConeChoisi);
            else
                referenceY = Canvas.GetTop(boulesEmpilees.Last());

            double posY = referenceY - boule.Height + pixelsChevauchement;

            Canvas.SetLeft(boule, posX);
            Canvas.SetTop(boule, posY);
            boulesEmpilees.Add(boule);
        }

        private void CheckHauteurMax()
        {
            if (boulesEmpilees.Count == 0) return;

            // Si la dernière boule dépasse la hauteur max (le haut de l'écran)
            if (Canvas.GetTop(boulesEmpilees.Last()) <= hauteurMax)
            {
                // 1. On supprime visuellement toutes les boules de la pile
                foreach (var boule in boulesEmpilees)
                {
                    canvasJeu.Children.Remove(boule);
                }

                // 2. On vide la liste mémoire
                boulesEmpilees.Clear();

                // 3. Bonus de points
                AjouterScore(100);

                // 4. On ne fait rien d'autre : le jeu continue tout seul !
                // (Optionnel : ajouter un petit son ici)
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
            MessageBox.Show("Perdu ! 3 boules au sol.");
            this.Close();
        }
    }
}