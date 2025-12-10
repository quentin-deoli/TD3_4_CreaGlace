using System;
using System.Windows.Media;

namespace CreaGlace
{
    public static class AudioManager
    {
        private static MediaPlayer musiquePlayer = new MediaPlayer();
        private static double musiqueVolume = 1.0;
        private static double sfxVolume = 1.0;

        // Initialiser la musique de fond
        public static void InitMusique(string musiqueFile)
        {
            musiquePlayer.Open(new Uri($"pack://application:,,,/Resources/Sons/{musiqueFile}"));
            musiquePlayer.Volume = musiqueVolume;
            musiquePlayer.MediaEnded += (s, e) => musiquePlayer.Position = TimeSpan.Zero; // boucle
            musiquePlayer.Play();
        }

        // Musique
        public static void SetMusicVolume(double volume)
        {
            musiqueVolume = volume;
            musiquePlayer.Volume = musiqueVolume;
        }

        public static double GetMusicVolume() => musiqueVolume;

        // SFX
        public static void SetSFXVolume(double volume)
        {
            sfxVolume = volume;
        }

        public static double GetSFXVolume() => sfxVolume;

        public static void PlaySFX(string nomFichier)
        {
            MediaPlayer sfxPlayer = new MediaPlayer();
            sfxPlayer.Open(new Uri($"pack://application:,,,/Resources/Sounds/{nomFichier}"));
            sfxPlayer.Volume = sfxVolume;
            sfxPlayer.Play();
        }
    }
}
