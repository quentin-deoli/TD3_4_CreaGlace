using System;
using System.IO;
using System.Windows.Media;

namespace CreaGlace
{
    public static class AudioManager
    {
        private static MediaPlayer musiquePlayer = new MediaPlayer();
        private static double musiqueVolume = 1.0;
        private static double sfxVolume = 1.0;

        // Initialiser et lancer la musique en boucle
        public static void InitMusique(string fichier)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sons", fichier);
            if (!File.Exists(path)) return;

            musiquePlayer.Open(new Uri(path, UriKind.Absolute));
            musiquePlayer.Volume = musiqueVolume;
            musiquePlayer.MediaEnded += (s, e) =>
            {
                musiquePlayer.Position = TimeSpan.Zero;
                musiquePlayer.Play();
            };
            musiquePlayer.Play();
        }

        // Jouer un SFX (compatible WAV/MP3)
        public static void PlaySFX(string fichier)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sons", fichier);
            if (!File.Exists(path)) return;

            MediaPlayer sfx = new MediaPlayer();
            sfx.Open(new Uri(path, UriKind.Absolute));
            sfx.Volume = sfxVolume;
            sfx.Play();
        }

        // Modifier le volume
        public static void SetMusicVolume(double volume)
        {
            musiqueVolume = volume;
            musiquePlayer.Volume = musiqueVolume;
        }

        public static void SetSFXVolume(double volume)
        {
            sfxVolume = volume;
        }

        // Getters pour Options.cs
        public static double GetMusicVolume() => musiqueVolume;
        public static double GetSFXVolume() => sfxVolume;
    }
}
