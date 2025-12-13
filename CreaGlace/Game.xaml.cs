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
        // --- 1. VARIABLES GLOBALES (Accessibles partout dans la fenêtre) ---

        // Outil pour générer des nombres aléatoires (pour la position et l'image des boules)
        private Random generateurAleatoire = new Random();

        // Liste pour stocker les boules qui sont en train de tomber (Dynamique : on peut ajouter/supprimer)
        private List<Image> boulesTombantes = new List<Image>();

        // Liste pour stocker les boules qui sont posées sur le cône
        private List<Image> boulesEmpilees = new List<Image>();

        // Les deux moteurs du jeu (Timers)
        private DispatcherTimer moteurJeu;       // Gère l'animation (60 images par seconde)
        private DispatcherTimer timerProgression; // Gère le temps qui passe (1 fois par seconde)

        // Paramètres de vitesse et de difficulté
        private double vitesseCone = 15;      // Vitesse de déplacement gauche/droite
        private double vitesseChute = 3;      // Vitesse à laquelle les boules tombent
        private int delaiApparition = 1500;   // Temps en millisecondes entre deux boules
        private DateTime prochainSpawn;       // Date/Heure de la prochaine apparition

        // Variables pour l'état de la partie
        private DateTime debutPartie;         // Pour calculer le temps total à la fin
        private int viesPerdues = 0;          // Compteur d'erreurs
        private int score = 0;                // Score du joueur
        private const int hauteurMax = 50;    // Limite de hauteur de la glace (en pixels depuis le haut)

        // Variable "Drapeau" pour savoir si le jeu est en Pause ou pas
        private bool estEnPause = false;

        // --- 2. CONSTRUCTEUR (Appelé quand la fenêtre s'ouvre) ---
        public Game(ImageSource imageDuCone)
        {
            InitializeComponent(); // Charge le XAML (le visuel)

            // On vérifie si l'image envoyée n'est pas vide, et on l'applique au cône
            if (imageDuCone != null)
                imgConeChoisi.Source = imageDuCone;

            // Configuration du Timer d'animation (Moteur du jeu)
            moteurJeu = new DispatcherTimer();
            moteurJeu.Interval = TimeSpan.FromMilliseconds(16); // ~60 fois par seconde (très fluide)
            moteurJeu.Tick += BoucleDeJeu_Tick; // Quelle méthode lancer à chaque "Tic"

            // Configuration du Timer de chronomètre
            timerProgression = new DispatcherTimer();
            timerProgression.Interval = TimeSpan.FromSeconds(1); // 1 fois par seconde
            timerProgression.Tick += Chronometre_Tick; // Quelle méthode lancer

            // On s'abonne aux événements importants
            this.Loaded += Game_Loaded; // Quand la fenêtre est totalement chargée
            this.KeyDown += Game_KeyDown; // Quand on appuie sur une touche
        }

        // --- 3. CHARGEMENT ET DÉMARRAGE ---
        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            // Calcul mathématique pour centrer le cône horizontalement
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;

            // Calcul pour placer le cône en bas (Hauteur du canvas - Hauteur du cône - Marge)
            double y = canvasJeu.ActualHeight - imgConeChoisi.Height - 10;

            // On applique les positions
            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, y);

            // On initialise le temps et on lance les timers
            debutPartie = DateTime.Now;
            prochainSpawn = DateTime.Now;
            moteurJeu.Start();
            timerProgression.Start();

            // Important : On donne le "Focus" au jeu pour qu'il capte les touches du clavier
            this.Focus();
        }

        // --- 4. GESTION DU CLAVIER (PAUSE + MOUVEMENTS) ---
        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            // A. GESTION DE LA PAUSE (Touche P)
            if (e.Key == Key.P)
            {
                if (estEnPause == false)
                {
                    // Si le jeu tourne, on arrête tout (PAUSE)
                    moteurJeu.Stop();
                    timerProgression.Stop();
                    txtPause.Visibility = Visibility.Visible; // On affiche le texte "PAUSE"
                    estEnPause = true; // On change l'état
                }
                else
                {
                    // Si le jeu est en pause, on relance tout (PLAY)
                    moteurJeu.Start();
                    timerProgression.Start();
                    txtPause.Visibility = Visibility.Collapsed; // On cache le texte
                    estEnPause = false; // On change l'état
                }
            }

            // SÉCURITÉ : Si on est en pause, on sort de la méthode ICI.
            // Le code en dessous ne sera pas lu, donc le cône ne bougera pas.
            if (estEnPause == true) return;


            // B. GESTION DES DÉPLACEMENTS (Flèches)
            double xActuel = Canvas.GetLeft(imgConeChoisi); // Où est le cône maintenant ?

            if (e.Key == Key.Left)
            {
                // On va à gauche (-). Math.Max(0, ...) empêche de sortir de l'écran à gauche.
                xActuel = Math.Max(0, xActuel - vitesseCone);
            }

            if (e.Key == Key.Right)
            {
                // On va à droite (+). Math.Min(...) empêche de sortir de l'écran à droite.
                xActuel = Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, xActuel + vitesseCone);
            }

            // On applique la nouvelle position au cône
            Canvas.SetLeft(imgConeChoisi, xActuel);

            // C. DÉPLACEMENT DES BOULES EMPILEES
            // Il faut que les boules déjà gagnées suivent le cône !
            foreach (Image boule in boulesEmpilees)
            {
                // On centre chaque boule par rapport au cône
                double decalage = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, xActuel + decalage);
            }
        }

        // --- 5. LOGIQUE DU TEMPS (Chronomètre) ---
        private void Chronometre_Tick(object sender, EventArgs e)
        {
            // Calcul du temps écoulé depuis le début
            TimeSpan tempsEcoule = DateTime.Now - debutPartie;

            // Affichage formaté (Ex: 01:30)
            txtTemps.Text = $"Temps: {tempsEcoule.Minutes:D2}:{tempsEcoule.Seconds:D2}";

            // Augmentation progressive de la difficulté
            vitesseChute += 0.1; // Les boules tombent plus vite
            if (delaiApparition > 500) delaiApparition -= 20; // Les boules apparaissent plus souvent
        }

        // --- 6. BOUCLE PRINCIPALE (Le cœur du jeu) ---
        private void BoucleDeJeu_Tick(object sender, EventArgs e)
        {
            // Étape 1 : Créer une nouvelle boule si le délai est passé
            if (DateTime.Now >= prochainSpawn)
            {
                CreerBoule();
                // On programme la prochaine apparition
                prochainSpawn = DateTime.Now.AddMilliseconds(delaiApparition);
            }

            // Étape 2 : Faire bouger toutes les boules qui tombent
            // On utilise une boucle à l'envers (i--) car on va peut-être supprimer des éléments
            for (int i = boulesTombantes.Count - 1; i >= 0; i--)
            {
                Image boule = boulesTombantes[i];

                // On calcule la nouvelle position verticale (Y)
                double newY = Canvas.GetTop(boule) + vitesseChute;
                Canvas.SetTop(boule, newY);

                // CAS A : La boule touche le sol (Raté !)
                if (newY >= canvasJeu.ActualHeight - boule.Height)
                {
                    viesPerdues++; // On perd une vie
                    canvasJeu.Children.Remove(boule); // On l'enlève visuellement
                    boulesTombantes.RemoveAt(i);      // On l'enlève de la liste en mémoire

                    if (viesPerdues >= 3) FinDePartie(); // Si 3 erreurs, c'est fini
                    continue; // On passe à la boule suivante
                }

                // CAS B : La boule touche le cône (Gagné !)
                if (DetecterCollision(boule))
                {
                    boulesTombantes.RemoveAt(i); // Elle ne tombe plus
                    EmpilerLaBoule(boule);       // Elle est empilée

                    score += 10; // Gain de points
                    txtScore.Text = $"Score: {score}";

                    VerifierHauteurPile(); // Est-ce qu'on a atteint le haut ?
                }
            }
        }

        // --- 7. CRÉATION D'UNE BOULE ---
        private void CreerBoule()
        {
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            // On tire un numéro d'image au hasard (entre 1 et 5)
            int numero = generateurAleatoire.Next(1, 6);
            string chemin = $"pack://application:,,,/Images/image{numero}.png";

            try
            {
                boule.Source = new BitmapImage(new Uri(chemin));
            }
            catch { } // Sécurité anti-plantage

            // On tire une position X au hasard sur la largeur de l'écran
            double x = generateurAleatoire.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60); // Elle commence juste au-dessus de l'écran

            canvasJeu.Children.Add(boule); // Ajout dans le Canvas (Visuel)
            boulesTombantes.Add(boule);    // Ajout dans la Liste (Logique)
        }

        // --- 8. DÉTECTION DE COLLISION ---
        private bool DetecterCollision(Image boule)
        {
            // On définit la cible : soit le cône vide, soit la dernière boule posée
            Image obstacle;
            if (boulesEmpilees.Count == 0)
                obstacle = imgConeChoisi;
            else
                obstacle = boulesEmpilees[boulesEmpilees.Count - 1]; // La dernière de la liste

            // On récupère les coordonnées de la boule qui tombe
            double bouleY = Canvas.GetTop(boule) + boule.Height; // Le bas de la boule
            double bouleX = Canvas.GetLeft(boule);

            // On récupère les coordonnées de l'obstacle
            double obstacleY = Canvas.GetTop(obstacle); // Le haut de l'obstacle
            double obstacleX = Canvas.GetLeft(obstacle);

            // TEST 1 : Est-ce qu'on touche en hauteur ? (avec une marge de 15px)
            bool toucheHauteur = (bouleY >= obstacleY && bouleY <= obstacleY + 15);

            // TEST 2 : Est-ce qu'on est aligné horizontalement ?
            bool toucheLargeur = (bouleX + boule.Width > obstacleX && bouleX < obstacleX + obstacle.Width);

            // Il faut que les 2 conditions soient vraies
            return toucheHauteur && toucheLargeur;
        }

        // --- 9. EMPILER LA BOULE ---
        private void EmpilerLaBoule(Image boule)
        {
            // On centre la nouvelle boule par rapport au cône
            double centreX = Canvas.GetLeft(imgConeChoisi) + (imgConeChoisi.Width - boule.Width) / 2;

            // On calcule sa hauteur finale
            double cibleY;
            if (boulesEmpilees.Count == 0)
                // Si c'est la première, on la met sur le cône (-15 pour l'effet d'emboîtement)
                cibleY = Canvas.GetTop(imgConeChoisi) - boule.Height + 15;
            else
                // Sinon, on la met sur la dernière boule
                cibleY = Canvas.GetTop(boulesEmpilees[boulesEmpilees.Count - 1]) - boule.Height + 15;

            // On fixe sa position
            Canvas.SetLeft(boule, centreX);
            Canvas.SetTop(boule, cibleY);

            // On l'ajoute à la liste des boules "fixes"
            boulesEmpilees.Add(boule);
        }

        // --- 10. BONUS DE HAUTEUR ---
        private void VerifierHauteurPile()
        {
            if (boulesEmpilees.Count > 0)
            {
                // On regarde la boule tout en haut de la pile
                Image bouleDuHaut = boulesEmpilees[boulesEmpilees.Count - 1];

                // Si elle atteint le haut de l'écran (hauteurMax)
                if (Canvas.GetTop(bouleDuHaut) <= hauteurMax)
                {
                    score += 100; // Bonus !
                    txtScore.Text = $"Score: {score}";

                    // On supprime toutes les boules visuellement
                    foreach (Image b in boulesEmpilees)
                    {
                        canvasJeu.Children.Remove(b);
                    }
                    // On vide la liste
                    boulesEmpilees.Clear();
                }
            }
        }

        // --- 11. FIN DE PARTIE ---
        private void FinDePartie()
        {
            // On arrête les timers pour figer le jeu
            moteurJeu.Stop();
            timerProgression.Stop();

            // On formate le temps final
            TimeSpan fin = DateTime.Now - debutPartie;
            string tempsStr = $"{fin.Minutes:D2}:{fin.Seconds:D2}";

            // On ouvre la fenêtre de Game Over en lui passant le score et le temps
            GameOver fenetreFin = new GameOver(score, tempsStr);
            fenetreFin.Show();

            // On ferme la fenêtre de jeu actuelle
            this.Close();
        }
    }
}