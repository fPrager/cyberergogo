using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace CyberErgoGo
{
    class SoundManager
    {
        Song StartMusic;
        Song PlayMusic;
        Song FinishMusic;
        SoundEffect EarthQuake;
        SoundEffect CanonExplosion;
        SoundEffect CanonFire;
        SoundEffect InvaderRiding;
        SoundEffect InvaderRolling;
        SoundEffect InvaderFlying;
        SoundEffectInstance EffectSound;
        SoundEffectInstance EarthQuakeSound;
        SoundEffectInstance InvaderSound;
        bool ChangeToNextSong = false;

        bool PlayInvaderSounds = false;
        Song CurrentInvaderSound;

        float MusicVolume = 0.3f;
        float EffectVolume = 0.8f;
        float latestFireVolume = 0;
        Song NextSong;

        public SoundManager()
        {
            MediaPlayer.IsRepeating = true;
        }

        public void LoadSounds()
        {
            Util.GetInstance().LoadFile(ref StartMusic, "Sound", "start_menu");
            Util.GetInstance().LoadFile(ref PlayMusic, "Sound", "game");
            Util.GetInstance().LoadFile(ref FinishMusic, "Sound", "finished");
            Util.GetInstance().LoadFile(ref CanonExplosion, "Sound", "explosion");
            Util.GetInstance().LoadFile(ref CanonFire, "Sound", "shoot");
            Util.GetInstance().LoadFile(ref InvaderRiding, "Sound", "fastinvader1");
            InvaderSound = InvaderRiding.CreateInstance();
            Util.GetInstance().LoadFile(ref InvaderRolling, "Sound", "fastinvader2");
            Util.GetInstance().LoadFile(ref InvaderFlying, "Sound", "fastinvader2");
            Util.GetInstance().LoadFile(ref EarthQuake, "Sound", "earthquake");
        }

        public void PlayFinishMusic()
        {
            ChangeToNextSong = true;
            NextSong = FinishMusic;
        }

        public void PlayGameMusic()
        {
            ChangeToNextSong = true;
            NextSong = PlayMusic;
        }

        public void PlayStartMusic()
        {
            ChangeToNextSong = true;
            NextSong = StartMusic;
        }

        public void Update(float time)
        {
            if (ChangeToNextSong)
            {
                if (MediaPlayer.Volume > 0) MediaPlayer.Volume -= time / 1000f;
                else
                {
                    ChangeToNextSong = false;
                    MediaPlayer.Play(NextSong);
                }
            }
            if (!ChangeToNextSong)
                if (MediaPlayer.Volume < MusicVolume) MediaPlayer.Volume += time / 1000f;
        }

        public void PlayExplosion(float percent = 1)
        {
            EffectSound = CanonExplosion.CreateInstance();
            EffectSound.Volume = (float)(EffectVolume * percent);
            EffectSound.Play();
        }

        public void PlayCanonFire(float percent = 1)
        {
            EffectSound = CanonFire.CreateInstance();
            EffectSound.Volume = (float)(EffectVolume * percent);
            if (EffectSound.State != SoundState.Playing)
            {
                EffectSound.Play();
                latestFireVolume = percent;
            }
            else
            {
                if (percent > latestFireVolume)
                {
                    EffectSound.Play();
                    latestFireVolume = percent;
                }
            }
        }

        public void PlayInvaderSound(float percent = 1)
        {
            InvaderSound.Volume = EffectVolume * percent;
            InvaderSound.Play();
        }

        public void ChangeInvaderSound(int index)
        {
            switch (index)
            { 
                case 1:
                    InvaderSound = InvaderRolling.CreateInstance();
                    break;
                case 2:
                    InvaderSound = InvaderFlying.CreateInstance();
                    break;
                default:
                    InvaderSound = InvaderRiding.CreateInstance();
                    break;
            }
        }

        public void PlayEarthQuake(float percent = 1)
        {
            EarthQuakeSound = EarthQuake.CreateInstance();
            EarthQuakeSound.Volume = (float)(EffectVolume * percent);
            EarthQuakeSound.Play();
        }

        public void ChangeEarthQuakeVolume(float percent)
        {
            EarthQuakeSound.Volume = (float)(EffectVolume * percent);
        }
    }
}
