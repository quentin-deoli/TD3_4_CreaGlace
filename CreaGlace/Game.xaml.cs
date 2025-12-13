using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading; // Indispensable pour le DispatcherTimer

namespace CreaGlace
{
    public partial class Game : Window
    {
        // Outils pour le jeu
        private Random generateurAleatoire = new Random();
        private DispatcherTimer moteurJeu;       // Gère les mouvements (60 fps)
        private DispatcherTimer timerProgression; // Gère le chronomètre

        // Listes pour gérer les objets
        private List<Image> boulesTombantes = new List<Image>();
        private List<Image> boulesEmpilees = new List<Image>();

        // Paramètres de jeu
        private double vitesseCone = 15;
        private double vitesseChute = 3;
        private int delaiSpawn = 1500;   // Temps en ms entre deux boules
        private DateTime prochainSpawn;

        // État de la partie
        private DateTime debutPartie;
        private int viesPerdues = 0;
        private int score = 0;
        private const int hauteurMax = 50; // Si la pile atteint le haut, on gagne un bonus

        // --- CONSTRUCTEUR ---
        public Game(ImageSource imageDuCone)
        {
            InitializeComponent();

            // 1. On récupère l'image envoyée par la fenêtre précédente
            if (imageDuCone != null)
                imgConeChoisi.Source = imageDuCone;

            // 2. Initialisation des Timers (Vu en cours WPF Avancé)
            moteurJeu = new DispatcherTimer();
            moteurJeu.Interval = TimeSpan.FromMilliseconds(16); // Environ 60 images/seconde
            moteurJeu.Tick += BoucleDeJeu_Tick;

            timerProgression = new DispatcherTimer();
            timerProgression.Interval = TimeSpan.FromSeconds(1);
            timerProgression.Tick += Chronometre_Tick;

            // 3. Événements de fenêtre
            this.Loaded += Game_Loaded;
            this.KeyDown += Game_KeyDown;
        }

        // Au chargement de la fenêtre
        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            // On place le cône au milieu, en bas
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double y = canvasJeu.ActualHeight - imgConeChoisi.Height - 10;

            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, y);

            // On lance le jeu
            debutPartie = DateTime.Now;
            prochainSpawn = DateTime.Now;
            moteurJeu.Start();
            timerProgression.Start();

            // On donne le focus au Canvas pour capter les touches
            this.Focus();
        }

        // Gestion des touches (Gauche / Droite)
        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            double xActuel = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
            {
                // Math.Max évite de sortir à gauche (valeur négative)
                xActuel = Math.Max(0, xActuel - vitesseCone);
            }

            if (e.Key == Key.Right)
            {
                // Math.Min évite de sortir à droite
                xActuel = Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, xActuel + vitesseCone);
            }

            // On applique la nouvelle position au cône
            Canvas.SetLeft(imgConeChoisi, xActuel);

            foreach (Image boule in boulesEmpilees)
            {
                // On centre la boule par rapport au cône
                double decalage = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, xActuel + decalage);
            }
        }

        // Gestion du Temps (1 fois par seconde)
        private void Chronometre_Tick(object sender, EventArgs e)
        {
            TimeSpan tempsEcoule = DateTime.Now - debutPartie;
            txtTemps.Text = $"Temps: {tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";

            // Petite augmentation de difficulté
            vitesseChute += 0.1;
            if (delaiSpawn > 500) delaiSpawn -= 20;
        }

        // BOUCLE PRINCIPALE DU JEU (60 fois par seconde)
        private void BoucleDeJeu_Tick(object sender, EventArgs e)
        {
            // A. Création des boules
            if (DateTime.Now >= prochainSpawn)
            {
                CreerBoule();
                prochainSpawn = DateTime.Now.AddMilliseconds(delaiSpawn);
            }

            // B. Gestion des boules qui tombent
            // On parcourt à l'envers (i--) pour pouvoir supprimer des éléments sans bug
            for (int i = boulesTombantes.Count - 1; i >= 0; i--)
            {
                Image boule = boulesTombantes[i];

                // On fait descendre la boule
                double newY = Canvas.GetTop(boule) + vitesseChute;
                Canvas.SetTop(boule, newY);

                // Cas 1 : La boule touche le sol (Raté)
                if (newY >= canvasJeu.ActualHeight - boule.Height)
                {
                    viesPerdues++;
                    canvasJeu.Children.Remove(boule); // Supprime du visuel
                    boulesTombantes.RemoveAt(i);      // Supprime de la liste logique

                    if (viesPerdues >= 3) FinDePartie();
                    continue;
                }

                // Cas 2 : Collision (Gagné)
                if (DetecterCollision(boule))
                {
                    boulesTombantes.RemoveAt(i); // On l'enlève de la liste "qui tombe"
                    EmpilerLaBoule(boule);       // On la met dans la liste "fixe"

                    score += 10;
                    txtScore.Text = $"Score: {score}";

                    VerifierHauteurPile();
                }
            }
        }

        private void CreerBoule()
        {
            // Création de l'image
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            // Choix aléatoire de l'image (1 à 5)
            // Assure-toi que tes images sont bien dans un dossier "Images" à la racine du projet
            int numeroImage = generateurAleatoire.Next(1, 6);
            string cheminImage = $"pack://application:,,,/Images/image{numeroImage}.png";

            try
            {
                boule.Source = new BitmapImage(new Uri(cheminImage));
            }
            catch
            {
                // Si l'image n'existe pas, ça évite le crash (optionnel)
            }

            // Position horizontale aléatoire
            double x = generateurAleatoire.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60); // Commence juste au-dessus de l'écran

            canvasJeu.Children.Add(boule);
            boulesTombantes.Add(boule);
        }

        private bool DetecterCollision(Image boule)
        {
            // On cherche avec quoi la boule peut entrer en collision
            // Soit le cône (si vide), soit la dernière boule posée
            Image cible;
            if (boulesEmpilees.Count == 0)
                cible = imgConeChoisi;
            else
                cible = boulesEmpilees[boulesEmpilees.Count - 1];

            // Coordonnées
            double bouleY = Canvas.GetTop(boule) + boule.Height;
            double cibleY = Canvas.GetTop(cible);

            double bouleX = Canvas.GetLeft(boule);
            double cibleX = Canvas.GetLeft(cible);

            // Test de collision simple (rectangles)
            bool toucheHauteur = (bouleY >= cibleY && bouleY <= cibleY + 15); // Marge de tolérance
            bool toucheLargeur = (bouleX + boule.Width > cibleX && bouleX < cibleX + cible.Width);

            return toucheHauteur && toucheLargeur;
        }

        private void EmpilerLaBoule(Image boule)
        {
            // On centre la boule sur le cône
            double centreX = Canvas.GetLeft(imgConeChoisi) + (imgConeChoisi.Width - boule.Width) / 2;

            // On calcule la position Y (sur le cône ou sur la pile)
            double cibleY;
            if (boulesEmpilees.Count == 0)
                cibleY = Canvas.GetTop(imgConeChoisi) - boule.Height + 15; // +15 pour l'effet "emboîté"
            else
                cibleY = Canvas.GetTop(boulesEmpilees[boulesEmpilees.Count - 1]) - boule.Height + 15;

            Canvas.SetLeft(boule, centreX);
            Canvas.SetTop(boule, cibleY);

            boulesEmpilees.Add(boule);
        }

        private void VerifierHauteurPile()
        {
            // Si on a des boules empilées
            if (boulesEmpilees.Count > 0)
            {
                // On regarde la boule tout en haut
                Image bouleDuHaut = boulesEmpilees[boulesEmpilees.Count - 1];

                // Si elle atteint le haut de l'écran
                if (Canvas.GetTop(bouleDuHaut) <= hauteurMax)
                {
                    // Bonus !
                    score += 100;
                    txtScore.Text = $"Score: {score}";

                    // On vide la pile (visuellement et logiquement)
                    foreach (Image b in boulesEmpilees)
                    {
                        canvasJeu.Children.Remove(b);
                    }
                    boulesEmpilees.Clear();
                }
            }
        }

        private void FinDePartie()
        {
            // Arrêt des chronos
            moteurJeu.Stop();
            timerProgression.Stop();

            // Préparation des infos pour la fin
            TimeSpan fin = DateTime.Now - debutPartie;
            string tempsStr = $"{fin.Minutes:D2}:{fin.Seconds:D2}";

            // Ouverture de la fenêtre GameOver
            GameOver fenetreFin = new GameOver(score, tempsStr);
            fenetreFin.Show();

            this.Close();
        }
    }
}