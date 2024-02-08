using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Media;
using System.IO;
using WMPLib;

namespace Slot_Simulator
{
    enum SoundTypes { Empty, HandlePull, ReelLock, Win1, Win2, Win3, Win4, Win5, Win6, Win7, Reel1, Reel2, Reel3, Reel4, Reel5 };
    class Sound
    {
        private string m_soundDirectory;
        private WindowsMediaPlayer m_soundPlayer = new WindowsMediaPlayer();

        public void PlaySound(string _soundFileName, bool _soundMute = true)
        {
            if (!_soundMute)
            {
                if (_soundFileName != null)
                {
                    m_soundDirectory = string.Format(@"{0}\sounds", m.Directory);
                    string soundLocation = string.Format(@"{0}\{1}", m_soundDirectory, _soundFileName);
                    if (!System.IO.File.Exists(soundLocation)) return;
                    m_soundPlayer.URL = soundLocation;
                    m_soundPlayer.settings.volume = (m_soundPlayer.settings.volume / 2);
                    m_soundPlayer.controls.play();
                }
            }
        }

        public void PlaySound(SoundTypes _soundType, bool _soundMute = true)
        {
            if (!_soundMute)
            {
                string soundName = null;
                switch (_soundType)
                {
                    case SoundTypes.Empty: return;
                    case SoundTypes.HandlePull: return;
                    case SoundTypes.ReelLock: soundName = "reellock.wav"; break;
                    case SoundTypes.Reel1: soundName = "reel_stop1.wav"; break;
                    case SoundTypes.Reel2: soundName = "reel_stop2.wav"; break;
                    case SoundTypes.Reel3: soundName = "reel_stop3.wav"; break;
                    case SoundTypes.Reel4: soundName = "reel_stop4.wav"; break;
                    case SoundTypes.Reel5: soundName = "reel_stop5.wav"; break;
                    case SoundTypes.Win1: soundName = "win_1.wav"; break;
                    case SoundTypes.Win2: soundName = "win_2.wav"; break;
                    case SoundTypes.Win3: soundName = "win_3.wav"; break;
                    case SoundTypes.Win4: soundName = "win_4.wav"; break;
                    case SoundTypes.Win5: soundName = "win_5.wav"; break;
                    case SoundTypes.Win6: soundName = "win_6.wav"; break;
                    case SoundTypes.Win7: soundName = "win_7.wav"; break;
                }
                if (soundName != null)
                {
                    m_soundDirectory = string.Format(@"{0}\sounds", m.Directory);
                    string soundLocation = string.Format(@"{0}\{1}", m_soundDirectory, soundName);
                    if (!System.IO.File.Exists(soundLocation)) return;
                    m_soundPlayer.URL = soundLocation;
                    m_soundPlayer.controls.play();
                }
            }
        }
    }
}
