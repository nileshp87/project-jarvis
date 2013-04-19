using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;

namespace Jarvis
{
    public class VolumeManager
    {

        /************Local Variables*************/
        int volume;
        bool muted;
        int volumeIncrements;
        /***************************************/

        public VolumeManager(int initialVolume, int increments)
        {
            initializeVolume();
            setVolume(initialVolume);
            volumeIncrements = increments;
        }

        public int raiseVolume()
        {
            setVolume(toPercent(volume) + volumeIncrements);
            return volume;
        }

        public int lowerVolume()
        {
            setVolume(toPercent(volume) - volumeIncrements);
            return volume;
        }

        public void setVolume(int setVolume)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "nircmdc.exe";
            startInfo.Arguments = "setsysvolume " + toArgument(setVolume);
            process.StartInfo = startInfo;
            process.Start();
            volume = toArgument(setVolume);
        }

        public int toPercent(int argumentVolume)
        {
            return (argumentVolume * 100) / 65535;
        }

        public int toArgument(int percentVolume)
        {
            return (percentVolume * 65535) / 100;
        }

        public void unMute()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "nircmdc.exe";
            startInfo.Arguments = "mutesysvolume 0";
            process.StartInfo = startInfo;
            process.Start();
            muted = false;
        }

        public void Mute()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "nircmdc.exe";
            startInfo.Arguments = "mutesysvolume 1";
            process.StartInfo = startInfo;
            process.Start();
            muted = true;
        }

        public bool toggleMute()
        {
            if (muted)
                unMute();
            else
                Mute();
            return muted;
        }

        public void initializeVolume()
        {
            volume = toArgument(30);
            setVolume(30);
            muted = false;
            unMute();
        }

        public bool isMuted()
        {
            return muted;
        }

        public int getVolume()
        {
            return toPercent(volume);
        }

    }  
    
}

