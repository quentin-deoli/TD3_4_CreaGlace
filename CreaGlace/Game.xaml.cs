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
        // Générateur de nombres aléatoires
        private Random generateurAleatoire = new Random();

        // Listes pour gérer les boules
        private List<Image> boulesTombantes = new List<Image>();
        private List<Image> boulesEmpilees = new List<Image>();

        // Timers
        private DispatcherTimer moteurJeu;
        private DispatcherTimer timerProgression;

        // Paramètres du jeu
        private double vitesseCone = 15;
        private double vitesseChute = 3;
        private int delaiApparition = 1500;
        private DateTime prochainSpawn;

        // État de la partie
        private DateTime debutPartie;
        private int viesPerdues = 0;
        private int score = 0;
        private const int hauteurMax = 50;
        private bool estEnPause = false;

        // Cône choisi
        private int coneChoisi;

        // ================= CONSTRUCTEUR =================
        public Game(int coneChoisi)
        {
            InitializeComponent();

            this.coneChoisi = coneChoisi;
            ChargerCone();

            moteurJeu = new DispatcherTimer();
            moteurJeu.Interval = TimeSpan.FromMilliseconds(16);
            moteurJeu.Tick += BoucleDeJeu_Tick;

            timerProgression = new DispatcherTimer();
            timerProgression.Interval = TimeSpan.FromSeconds(1);
            timerProgression.Tick += Chronometre_Tick;

            this.Loaded += Game_charger;
            this.KeyDown += Game_touche;
        }

        // ================= CHARGEMENT =================
        private void Game_charger(object sender, RoutedEventArgs e)
        {
            double x = (canvasJeu.ActualWidth - imgConeChoisi.Width) / 2;
            double y = canvasJeu.ActualHeight - imgConeChoisi.Height - 10;

            Canvas.SetLeft(imgConeChoisi, x);
            Canvas.SetTop(imgConeChoisi, y);

            debutPartie = DateTime.Now;
            prochainSpawn = DateTime.Now;

            moteurJeu.Start();
            timerProgression.Start();

            this.Focus();
        }

        // ================= CLAVIER =================
        private void Game_touche(object sender, KeyEventArgs e)
        {
            // Pause
            if (e.Key == Key.P)
            {
                if (!estEnPause)
                {
                    moteurJeu.Stop();
                    timerProgression.Stop();
                    txtPause.Visibility = Visibility.Visible;
                    estEnPause = true;
                }
                else
                {
                    moteurJeu.Start();
                    timerProgression.Start();
                    txtPause.Visibility = Visibility.Collapsed;
                    estEnPause = false;
                }
            }

            if (estEnPause) return;

            double xActuel = Canvas.GetLeft(imgConeChoisi);

            if (e.Key == Key.Left)
                xActuel = Math.Max(0, xActuel - vitesseCone);

            if (e.Key == Key.Right)
                xActuel = Math.Min(canvasJeu.ActualWidth - imgConeChoisi.Width, xActuel + vitesseCone);

            Canvas.SetLeft(imgConeChoisi, xActuel);

            // Les boules empilées suivent le cône
            foreach (Image boule in boulesEmpilees)
            {
                double decalage = (imgConeChoisi.Width - boule.Width) / 2;
                Canvas.SetLeft(boule, xActuel + decalage);
            }
        }

        // ================= CHRONOMÈTRE =================
        private void Chronometre_Tick(object sender, EventArgs e)
        {
            TimeSpan temps = DateTime.Now - debutPartie;
            txtTemps.Text = $"Temps: {temps.Minutes:D2}:{temps.Seconds:D2}";

            vitesseChute += 0.1;
            if (delaiApparition > 500)
                delaiApparition -= 15;
        }

        // ================= BOUCLE DE JEU =================
        private void BoucleDeJeu_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= prochainSpawn)
            {
                CreerBoule();
                prochainSpawn = DateTime.Now.AddMilliseconds(delaiApparition);
            }

            for (int i = boulesTombantes.Count - 1; i >= 0; i--)
            {
                Image boule = boulesTombantes[i];
                double newY = Canvas.GetTop(boule) + vitesseChute;
                Canvas.SetTop(boule, newY);

                // Raté
                if (newY >= canvasJeu.ActualHeight - boule.Height)
                {
                    viesPerdues++;
                    canvasJeu.Children.Remove(boule);
                    boulesTombantes.RemoveAt(i);

                    if (viesPerdues >= 3)
                        FinDePartie();

                    continue;
                }

                // Touché
                if (DetecterCollision(boule))
                {
                    boulesTombantes.RemoveAt(i);
                    EmpilerLaBoule(boule);

                    score += 10;
                    txtScore.Text = $"Score: {score}";

                    VerifierHauteurPile();
                }
            }
        }

        // ================= CRÉER UNE BOULE =================
        private void CreerBoule()
        {
            Image boule = new Image();
            boule.Width = 60;
            boule.Height = 60;

            int numero = generateurAleatoire.Next(1, 6);
            string chemin = $"Images/image{numero}.png";

            boule.Source = new BitmapImage(new Uri(chemin, UriKind.Relative));

            double x = generateurAleatoire.Next(0, (int)(canvasJeu.ActualWidth - boule.Width));

            Canvas.SetLeft(boule, x);
            Canvas.SetTop(boule, -60);

            canvasJeu.Children.Add(boule);
            boulesTombantes.Add(boule);
        }

        // ================= COLLISION =================
        private bool DetecterCollision(Image boule)
        {
            Image obstacle;

            if (boulesEmpilees.Count == 0)
            {
                obstacle = imgConeChoisi;
            }
            else
            {
                obstacle = boulesEmpilees[boulesEmpilees.Count - 1];
            }

            double bouleY = Canvas.GetTop(boule) + boule.Height;
            double bouleX = Canvas.GetLeft(boule);

            double obstacleY = Canvas.GetTop(obstacle);
            double obstacleX = Canvas.GetLeft(obstacle);

            bool toucheHauteur = (bouleY >= obstacleY && bouleY <= obstacleY + 15);
            bool toucheLargeur = (bouleX + boule.Width > obstacleX &&
                                  bouleX < obstacleX + obstacle.Width);

            return toucheHauteur && toucheLargeur;
        }

        // ================= EMPILER =================
        private void EmpilerLaBoule(Image boule)
        {
            double centreX = Canvas.GetLeft(imgConeChoisi)
                            + (imgConeChoisi.Width - boule.Width) / 2;

            double cibleY;
            if (boulesEmpilees.Count == 0)
                cibleY = Canvas.GetTop(imgConeChoisi) - boule.Height + 15;
            else
                cibleY = Canvas.GetTop(boulesEmpilees[boulesEmpilees.Count - 1]) - boule.Height + 15;

            Canvas.SetLeft(boule, centreX);
            Canvas.SetTop(boule, cibleY);

            boulesEmpilees.Add(boule);
        }

        // ================= BONUS HAUTEUR =================
        private void VerifierHauteurPile()
        {
            if (boulesEmpilees.Count == 0) return;

            Image bouleDuHaut = boulesEmpilees[boulesEmpilees.Count - 1];

            if (Canvas.GetTop(bouleDuHaut) <= hauteurMax)
            {
                score += 100;
                txtScore.Text = $"Score: {score}";

                foreach (Image b in boulesEmpilees)
                    canvasJeu.Children.Remove(b);

                boulesEmpilees.Clear();
            }
        }

        // ================= FIN DE PARTIE =================
        private void FinDePartie()
        {
            moteurJeu.Stop();
            timerProgression.Stop();

            TimeSpan fin = DateTime.Now - debutPartie;
            string tempsStr = $"{fin.Minutes:D2}:{fin.Seconds:D2}";

            GameOver finJeu = new GameOver(score, tempsStr);
            finJeu.Show();

            this.Close();
        }

        // ================= CHARGER LE CÔNE =================
        private void ChargerCone()
        {
            string chemin = "";

            if (coneChoisi == 1)
                chemin = "Images/cone1.png";
            else if (coneChoisi == 2)
                chemin = "Images/cone2.png";
            else if (coneChoisi == 3)
                chemin = "Images/cone3.png";
            else if (coneChoisi == 4)
                chemin = "Images/cone4.png";

            imgConeChoisi.Source = (ImageSource)
                new ImageSourceConverter().ConvertFromString(chemin);
        }
    }
}