using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CreaGlace
{
    /// <summary>
    /// Logique d'interaction pour Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            BoutonRetour.Click += BoutonRetour_Click;
            BoutonSauvegarder.Click += BoutonSauvegarder_Click;
            //"Quand on clique sur ce bouton, lance cette fonction"
        }
        private void BoutonRetour_Click(object sender, RoutedEventArgs e)
        {
            this.Close();// Ferme la fenêtre
        }

        private void BoutonSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            // On récupère les valeurs de tes sliders (j'utilise tes noms exacts)
            double musique = SliderMusique.Value;
            double sfx = SlidersSFX.Value;

            // Message de confirmation (tu pourras l'enlever plus tard)
            string message = $"C'est noté !\nMusique : {musique:0}%\nSons : {sfx:0}%";

            MessageBox.Show(message, "Options Glace", MessageBoxButton.OK, MessageBoxImage.Information);

            // Optionnel : Ferme la fenêtre après la sauvegarde
            this.Close();
        }
    }
}
