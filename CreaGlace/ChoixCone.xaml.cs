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

            if (selectedImage != null && selectedImage != img)
                AnimateToNormal(selectedImage);

            selectedImage = img;
            AnimateToBig(img);
        }

        private void AnimateToBig(Image img)
        {
            ScaleTransform scale = img.RenderTransform as ScaleTransform;
            DoubleAnimation anim = new DoubleAnimation(1.3, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void AnimateToNormal(Image img)
        {
            ScaleTransform scale = img.RenderTransform as ScaleTransform;
            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150))
            {
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

            // Passer l'image sélectionnée à Regle pour qu'elle puisse ensuite l'envoyer à Game
            Regle regle = new Regle(selectedImage.Source);
            regle.Show();
            this.Close();
        }
    }
}
