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
        // =========================================================
        // 1. CONSTANTES : Ce sont les règles fixes du jeu.
        // On les met ici pour pouvoir changer la difficulté facilement.
        // =========================================================
        const int METTRE_A_JOUR = 20;       // Le Timer "tique" toutes les 20ms (vitesse du jeu).
        const int TICKS_PAR_SECONDE = 50;   // Math : 1000ms / 20ms = 50. Il faut 50 tics pour 1 seconde.
        const int TICKS_APPARITION = 70;    // On attend 70 tics (environ 1,5 sec) avant qu'une boule apparaisse.
        const int VITESSE_CHUTE = 5;        // La boule descend de 5 pixels à chaque tic.
        const int VITESSE_DEPLACEMENT = 20; // Le cône bouge de 20 pixels quand on appuie sur une flèche.
        const int VIES_MAX = 3;             // Le joueur a 3 vies au total.

        // =========================================================
        // 2. OUTILS : Les objets C# qui nous aident.
        // =========================================================
        Random hasard = new Random();               // Outil pour générer des nombres aléatoires (position, couleur).
        MediaPlayer lecteurSon = new MediaPlayer(); // Outil pour jouer les fichiers sons (.mp3).
        DispatcherTimer timer = new DispatcherTimer(); // Le moteur du jeu : il déclenche une action en boucle.

        // =========================================================
        // 3. VARIABLES DU JEU : Ce qui change pendant la partie.
        // =========================================================
        Image bouleEnChute = null; // La boule qui tombe. "null" veut dire "il n'y en a pas encore".

        // --- GESTION DU TABLEAU (Cours Séquence 5) ---
        Image[] tableauBoules = new Image[50]; // On réserve 50 cases en mémoire pour stocker les images.
        int nombreBoules = 0;                  // C'est notre index. Il dit combien de boules on a déjà attrapées.

        int score = 0;          // Le score actuel (commence à 0).
        int viesPerdues = 0;    // Combien de fois on a raté (commence à 0).
        int numeroCone;         // Le numéro du cône choisi dans le menu (1, 2 ou 3).
        bool enPause = false;   // Est-ce que le jeu est stoppé ? (Faux au début).

        // =========================================================
        // 4. VARIABLES DE TEMPS
        // =========================================================
        int secondesEcoulees = 0; // Le temps affiché à l'écran (0, 1, 2...).
        int ticksTemps = 0;       // Compte jusqu'à 50 pour savoir quand ajouter une seconde.
        int ticksBoule = 0;       // Compte jusqu'à 70 pour savoir quand lancer une boule.

        // =========================================================
        // CONSTRUCTEUR : Le point de départ quand la fenêtre s'ouvre.
        // =========================================================
        public Game(int choixDuJoueur)
        {
            InitializeComponent(); // Charge le code XAML (le visuel).

            numeroCone = choixDuJoueur; // On mémorise le choix du joueur.

            // On branche les événements (les "oreilles" du programme) :
            Loaded += DemarrerJeu;  // Quand la fenêtre est prête -> lance la méthode DemarrerJeu.
            KeyDown += AppuiTouche; // Quand on appuie sur une touche -> lance la méthode AppuiTouche.

            // On règle le Timer :
            timer.Interval = TimeSpan.FromMilliseconds(METTRE_A_JOUR); // Vitesse réglée sur 20ms.
            timer.Tick += BoucleDuJeu; // À chaque tic -> lance la méthode BoucleDuJeu.
        }

        // =========================================================
        // DEMARRAGE : Mise en place visuelle.
        // =========================================================
        void DemarrerJeu(object sender, RoutedEventArgs e)
        {
            // On change l'image du cône selon le numéro (concaténation de chaînes).
            imgConeChoisi.Source = new BitmapImage(
                new Uri("Images/cone" + numeroCone + ".png", UriKind.Relative));

            // Calcul mathématique pour centrer : (Largeur Écran - Largeur Cône) / 2.
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;

            // On place le cône graphiquement.
            Canvas.SetLeft(imgConeChoisi, x);                 // Position horizontale (X).
            Canvas.SetTop(imgConeChoisi, canvasJeu.ActualHeight - 100); // Position verticale (Y) en bas.

            timer.Start(); // C'est parti ! On allume le moteur.
        }

        // =========================================================
        // BOUCLE DU JEU : Le cœur du programme (50 fois/sec).
        // =========================================================
        void BoucleDuJeu(object sender, EventArgs e)
        {
            if (enPause) return; // Si pause est Vrai, on arrête la méthode ici (on ne fait rien).

            GererTemps(); // On met à jour l'horloge.
            GererBoule(); // On s'occupe de la boule qui tombe.
        }

        // =========================================================
        // GESTION DU TEMPS
        // =========================================================
        void GererTemps()
        {
            ticksTemps++; // On ajoute 1 au petit compteur.

            // Si on atteint 50 tics (donc 1 seconde réelle).
            if (ticksTemps >= TICKS_PAR_SECONDE)
            {
                ticksTemps = 0;     // On remet le compteur à 0 pour la prochaine seconde.
                secondesEcoulees++; // On ajoute 1 seconde au temps de jeu.

                // On affiche le temps formaté "00:00".
                // / 60 donne les minutes, % 60 donne les secondes restantes.
                // "D2" force l'affichage sur 2 chiffres (ex: 05).
                txtTemps.Text = "Temps : " +
                    (secondesEcoulees / 60).ToString("D2") + ":" +
                    (secondesEcoulees % 60).ToString("D2");
            }
        }

        // =========================================================
        // GESTION DE LA BOULE
        // =========================================================
        void GererBoule()
        {
            ticksBoule++; // On ajoute 1 au compteur d'attente.

            // SCÉNARIO 1 : Il n'y a pas de boule à l'écran.
            if (bouleEnChute == null)
            {
                // Si on a attendu assez longtemps (70 tics).
                if (ticksBoule >= TICKS_APPARITION)
                {
                    ticksBoule = 0;       // Reset du compteur.
                    CreerNouvelleBoule(); // On fabrique une nouvelle boule.
                }
            }
            // SCÉNARIO 2 : Une boule est en train de tomber.
            else
            {
                // On récupère sa hauteur actuelle (Y).
                double y = Canvas.GetTop(bouleEnChute);
                // On la descend de 5 pixels.
                Canvas.SetTop(bouleEnChute, y + VITESSE_CHUTE);

                // Check 1 : Est-ce qu'elle touche le joueur ?
                if (TestCollision())
                {
                    AttraperBoule(); // Oui -> Gagné.
                }
                // Check 2 : Est-ce qu'elle sort de l'écran par le bas ?
                else if (y > canvasJeu.ActualHeight)
                {
                    PerdreVie(); // Oui -> Perdu.
                }
            }
        }

        // =========================================================
        // CRÉATION (Instanciation)
        // =========================================================
        void CreerNouvelleBoule()
        {
            bouleEnChute = new Image(); // On crée un nouvel objet Image vide en mémoire.
            bouleEnChute.Width = 60;    // On fixe sa taille.
            bouleEnChute.Height = 60;

            // On choisit une image au hasard entre 1 et 5.
            int num = hasard.Next(1, 6);
            bouleEnChute.Source = new BitmapImage(new Uri("Images/image" + num + ".png", UriKind.Relative));

            // On choisit une position X au hasard (entre 0 et la largeur max possible).
            double x = hasard.Next(0, (int)(canvasJeu.ActualWidth - bouleEnChute.Width));

            Canvas.SetLeft(bouleEnChute, x);  // On la place en X.
            Canvas.SetTop(bouleEnChute, -60); // On la place en Y (cachée en haut, hors champ).

            canvasJeu.Children.Add(bouleEnChute); // On l'ajoute visuellement à l'écran.
        }

        // =========================================================
        // CLAVIER (Entrées utilisateur)
        // =========================================================
        void AppuiTouche(object sender, KeyEventArgs e)
        {
            // --- GESTION PAUSE (Touche P) ---
            if (e.Key == Key.P)
            {
                enPause = !enPause; // On inverse (Vrai devient Faux, et inversement).
                JouerLeSon("pause.mp3");
                // On affiche ou cache le texte PAUSE.
                if (enPause == true) txtPause.Visibility = Visibility.Visible;
                else txtPause.Visibility = Visibility.Collapsed;

                return; // On arrête là, on ne bouge pas si on met pause.
            }

            if (enPause) return; // Sécurité supplémentaire.

            // --- GESTION DÉPLACEMENT ---
            double x = Canvas.GetLeft(imgConeChoisi); // Où est le cône ?

            // Si flèche gauche, on diminue X.
            if (e.Key == Key.Left) x -= VITESSE_DEPLACEMENT;
            // Si flèche droite, on augmente X.
            if (e.Key == Key.Right) x += VITESSE_DEPLACEMENT;

            // --- LIMITES (Murs invisibles) ---
            // Si on dépasse à gauche, on remet à 0.
            if (x < 0) x = 0;
            // Si on dépasse à droite, on bloque au bord droit.
            if (x + imgConeChoisi.Width > canvasJeu.ActualWidth)
                x = canvasJeu.ActualWidth - imgConeChoisi.Width;

            // On applique la nouvelle position au cône.
            Canvas.SetLeft(imgConeChoisi, x);

            // --- DÉPLACEMENT DE LA PILE (Tableau) ---
            // On parcourt toutes les boules qu'on a déjà gagnées.
            for (int i = 0; i < nombreBoules; i++)
            {
                // On les déplace toutes pour qu'elles suivent le cône (+10 pour centrer).
                Canvas.SetLeft(tableauBoules[i], x + 10);
            }
        }

        // =========================================================
        // COLLISION (La logique mathématique)
        // =========================================================
        bool TestCollision()
        {
            Image cible; // Ce que la boule doit toucher.

            // Si le tableau est vide (compteur à 0), on doit toucher le cône.
            if (nombreBoules == 0)
            {
                cible = imgConeChoisi;
            }
            // Sinon, on doit toucher la dernière boule du tableau (index - 1).
            else
            {
                cible = tableauBoules[nombreBoules - 1];
            }

            // Coordonnées de la boule qui tombe (Hitbox).
            double bouleBas = Canvas.GetTop(bouleEnChute) + bouleEnChute.Height;
            double bouleGauche = Canvas.GetLeft(bouleEnChute);
            double bouleDroite = bouleGauche + bouleEnChute.Width;

            // Coordonnées de la cible (Cône ou Boule du dessus).
            double cibleHaut = Canvas.GetTop(cible);
            double cibleGauche = Canvas.GetLeft(cible);
            double cibleDroite = cibleGauche + cible.Width;

            // TEST 1 : Hauteur -> Est-ce que le bas de la boule touche le haut de la cible ?
            bool toucheHauteur = bouleBas >= cibleHaut && bouleBas <= cibleHaut + 15; // Marge de 15px.

            // TEST 2 : Largeur -> Est-ce qu'elles sont alignées horizontalement ?
            bool toucheLargeur = bouleDroite > cibleGauche && bouleGauche < cibleDroite;

            // Si les deux tests sont vrais, il y a collision.
            return toucheHauteur && toucheLargeur;
        }

        // =========================================================
        // ACTION : ATTRAPER UNE BOULE (Le cœur du gameplay)
        // =========================================================
        void AttraperBoule()
        {
            // 1. GESTION DU SCORE
            JouerLeSon("pop.mp3");
            score += 10;
            txtScore.Text = "Score : " + score;

            // 2. STOCKAGE
            tableauBoules[nombreBoules] = bouleEnChute;

            // 3. PLACEMENT VISUEL (AJUSTÉ)
            // On remonte encore un peu (270 au lieu de 290).
            // Et on garde un décalage de 50px entre chaque boule pour qu'elles se chevauchent un peu.
            double nouvelleHauteur = 270 - (nombreBoules * 50);

            Canvas.SetTop(bouleEnChute, nouvelleHauteur);

            // 4. INCREMENTATION
            nombreBoules++;

            // 5. VICTOIRE
            if (nombreBoules == 50)
            {
                timer.Stop();
                Gagne fenetreVictoire = new Gagne(score, txtTemps.Text);
                fenetreVictoire.Show();
                this.Close();
                return;
            }

            // 6. NETTOYAGE
            bouleEnChute = null;

            // 7. BONUS
            VerifierBonus();
        }
        // =========================================================
        // ACTION : PERDRE (Échec)
        // =========================================================
        void PerdreVie()
        {
            JouerLeSon("fail.mp3");

            canvasJeu.Children.Remove(bouleEnChute); // On supprime l'image de l'écran.
            bouleEnChute = null; // On vide la variable mémoire.

            viesPerdues++; // On ajoute une erreur.

            // Si on atteint la limite de vies (3).
            if (viesPerdues >= VIES_MAX)
            {
                JouerLeSon("gameover.mp3");
                timer.Stop(); // On arrête le jeu.

                // On crée la fenêtre de fin en lui passant le score et le temps.
                GameOver fin = new GameOver(score, txtTemps.Text);
                fin.Show(); // On l'affiche.
                Close();    // On ferme la fenêtre de jeu.
            }
        }

        // =========================================================
        // BONUS (Vider le tableau)
        // =========================================================
        void VerifierBonus()
        {
            if (nombreBoules == 0) return; // Sécurité (si tableau vide, on sort).

            // On regarde la hauteur de la dernière boule ajoutée.
            // Si Y < 50, ça veut dire qu'elle est tout en haut de l'écran.
            if (Canvas.GetTop(tableauBoules[nombreBoules - 1]) < 50)
            {
                score += 100; // Gros bonus.
                JouerLeSon("bonus.mp3");

                // On utilise une boucle pour supprimer toutes les boules de l'écran.
                for (int i = 0; i < nombreBoules; i++)
                {
                    canvasJeu.Children.Remove(tableauBoules[i]); // Suppression visuelle.
                    tableauBoules[i] = null; // Nettoyage de la case mémoire.
                }

                // On remet le compteur à 0 : le tableau est considéré comme vide.
                nombreBoules = 0;
            }
        }

        // =========================================================
        // SON
        // =========================================================
        void JouerLeSon(string fichier)
        {
            lecteurSon.Stop(); // Stop si un son jouait déjà.
            lecteurSon.Open(new Uri("Sons/" + fichier, UriKind.Relative)); // Charge le fichier.
            lecteurSon.Play(); // Joue.
        }
    }
}