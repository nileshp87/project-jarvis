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
    public class MusicControl : IJModule
    {
        /************CONSTANTS**************/
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;
        private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
        private const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
        private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int HWND_BROADCAST = 0xffff;
        /************DLL IMPORTS************/
               [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
               [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
                [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
               [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);
              [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr hWnd);
                [DllImport("User32.DLL")]
        static extern int SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
             [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
                [DllImport("User32.dll")]
        public static extern string GetWindowText(int hWnd);

        /************Local Variables*************/
        private static String playername;
        private static bool playing;
        private IntPtr playerHandle;
        private VolumeManager volumeControl;
        private bool wasPlaying;
        private bool wasMuted;
        /***************************************/

        public MusicControl(String player, int initialVolume, int volumeIncrements)
        {
            playing = false;
            volumeControl = new VolumeManager(initialVolume, volumeIncrements);
            playername = player;
            getPlayerHandle();
        }

        private void getPlayerHandle(){
           Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                if (proc.MainWindowHandle != IntPtr.Zero && proc.ProcessName == playername)
                {
                    Debug.WriteLine(proc.ProcessName);
                    playerHandle = proc.MainWindowHandle;
                }
            } 
    
        }
        private void next()
        {
            if (playerHandle != null)
            {
                SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
                PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.N), IntPtr.Zero);
            }
            else
            {
                getPlayerHandle();
                SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
                PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.N), IntPtr.Zero);
            }
        }
        private void previous()
        {
            if (playerHandle != null)
            {
                SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
                PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.P), IntPtr.Zero);
            }
            else
            {
                getPlayerHandle();
                SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
                PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.N), IntPtr.Zero);
            }
        }
        private void play()
        {
            if (!playing)
            {           
                if (playerHandle != null)
                {
                    PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.Y), IntPtr.Zero);
                    SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
                }
                else
                {

                        getPlayerHandle();
                        SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
                        PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.Y), IntPtr.Zero);
                }
                playing = true;
            }
        }
        private void pause()
        {
            if (playing)
            {
                if (playerHandle != null)
                {
                    PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.U), IntPtr.Zero);
                    SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
                }
                else
                {
                
                        getPlayerHandle();
                        SendMessageW(playerHandle, WM_APPCOMMAND, playerHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
                        PostMessage(playerHandle, WM_KEYDOWN, ((IntPtr)Key.U), IntPtr.Zero);
                }
                playing = false;
            }
        }
        private int lowerVolume()
        {
            return volumeControl.lowerVolume();
        }
        private int raiseVolume()
        {
            return volumeControl.raiseVolume();
        }
        private bool isPlaying()
        {
            return playing;
        }
        private bool isMuted()
        {
            return volumeControl.isMuted();
        }
        private bool toggleMute()
        {
            return volumeControl.toggleMute();
        }

        public string alertFeed(out bool canReply, out string simulateResponse)
        {
            simulateResponse = null;
            canReply = false;
            return null;
        }
        public void userEntered()
        {
            if (wasPlaying)
                play();
            if (!wasMuted)
                toggleMute();
        }
        public void userLeft()
        {
            wasPlaying = isPlaying();
            wasMuted = isMuted();
            if(isPlaying())
                pause();
            if (!wasMuted)
                toggleMute();
        }
        public void nightMode()
        {
            userLeft();
        }
        public void dayMode()
        {
            userEntered();
        }
        public string userSaid(string s, out bool endConversation, out bool passNextDictation, out string simulateResponse)
        {
            simulateResponse = null;
            passNextDictation = false;
            endConversation = true;
            s = s.Substring(13);
            Console.WriteLine(s);
            switch(s){
                case("play"):
                    play();
                    break;
                case("pause"):
                    pause();
                    break;
                case("previous"):
                    previous();
                    break;
                case("next"):
                    next();
                    break;
                case("mute"):
                    toggleMute();
                    break;
                case("louder"):
                    volumeControl.raiseVolume();
                    break;
                case("quieter"):
                    volumeControl.lowerVolume();
                    break;
                default:
                    break;
            }
            return null;
        }
        public string moduleName()
        {
            return "MusicControl";
        }
        public string getGrammarFile()
        {
            return "MusicControl.srgs";
        }
        public void otherConversation()
        {
            wasPlaying = isPlaying();
            if (isPlaying())
                pause();
        }
        public void endOtherConversation()
        {
            if (wasPlaying)
                play();
        }
        public void quietMode()
        {
            otherConversation();
        }
        public void loudMode()
        {
            endOtherConversation();
        }
    }
}
