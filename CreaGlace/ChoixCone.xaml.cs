using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CreaGlace
{
    public partial class ChoixCone : Window
    {
        private Image selectedImage = null;

        public ChoixCone()
        {
            InitializeComponent();
        }

        private void Cone_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Image img = btn.Content as Image;

            // Reset l'ancien cône si différent
            if (selectedImage != null && selectedImage != img)
            {
                AnimateToNormal(selectedImage);
            }

            selectedImage = img;

            // Agrandir le cône sélectionné
            AnimateToBig(img);
        }

        private void AnimateToBig(Image img)
        {
            ScaleTransform scale = img.RenderTransform as ScaleTransform;

            DoubleAnimation anim = new DoubleAnimation()
            {
                To = 1.3,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void AnimateToNormal(Image img)
        {
            ScaleTransform scale = img.RenderTransform as ScaleTransform;

            DoubleAnimation anim = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            if (selectedImage == null)
            {
                MessageBox.Show("Choisis un cône avant de continuer !");
                return;
            }

            // Passer directement le ImageSource au jeu
            ImageSource selectedConeSource = selectedImage.Source;

            Game game = new Game(selectedConeSource);
            game.Show();
            this.Close();
        }
    }
}
