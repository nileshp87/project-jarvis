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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Kinect;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Runtime.InteropServices;
using Microsoft.Speech;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.IO;

namespace Jarvis
{
    
    public class Jarvis
    {
        private UserPrefs preferences = new UserPrefs("Preferences.xml");
        private LinkedList<IJModule> modules;
        private Thread alertThread;
        private IJModule replyObj;
        private SpeechSynthesizer voice = new SpeechSynthesizer();
        private System.Speech.Recognition.SpeechRecognitionEngine dictation;
        private KinectSensor sensor;
        private bool currentlyDictating;
        private string lastDictated;
        private Microsoft.Speech.Recognition.SpeechRecognitionEngine sre;


        
        public Jarvis()
        {
            modules = new LinkedList<IJModule>();
            /*************** IJModule Instatiation Stuff ****************/
            modules.AddLast(new MusicControl(preferences.mediaplayerprocess, preferences.initialvolume, preferences.volumeincrements));
            if (preferences.usegooglevoice)
                modules.AddLast(new GoogleVoice(preferences.googleemail, preferences.googlepassword, preferences.googleaddressbook));
            if (preferences.facebookrssfeed != null)
                modules.AddLast(new Facebook(preferences.facebookrssfeed));
            if (preferences.usegooglecalendar)
                modules.AddLast(new GoogleCalendar(preferences.googleemail, preferences.googlepassword, preferences.googlecalendaralerttime));
            alertThread = new Thread(new ThreadStart(alertFunction));
            alertThread.Name = "Alert Thread";
            alertThread.Start();

            /****************Get Grammar From Modules*********************/
            var grammars = new LinkedList<Microsoft.Speech.Recognition.Grammar>();
            foreach (IJModule module in modules)
            {
                if(module.getGrammarFile() != null)
                {
                    var gb = new Microsoft.Speech.Recognition.GrammarBuilder();
                    gb.AppendRuleReference("file://" + System.Environment.CurrentDirectory + "\\" + module.getGrammarFile());
                    Console.WriteLine("file://"+System.Environment.CurrentDirectory+"\\" + module.getGrammarFile());
                    grammars.AddLast(new Microsoft.Speech.Recognition.Grammar(gb));
                }
            }
            
            /************ Speech Recognition Stuff **********************/
            
            dictation = new System.Speech.Recognition.SpeechRecognitionEngine();
            dictation.SetInputToDefaultAudioDevice();
            dictation.LoadGrammar(new DictationGrammar());
            dictation.SpeechRecognized += SreSpeechRecognized;
            
            sensor = (from sensorToCheck in KinectSensor.KinectSensors where sensorToCheck.Status == KinectStatus.Connected select sensorToCheck).FirstOrDefault();

            if (sensor == null)
            {
                Console.WriteLine(
                        "No Kinect sensors are attached to this computer or none of the ones that are\n" +
                        "attached are \"Connected\".\n" +
                        "Press any key to continue.\n");

                Console.ReadKey(true);
                return;
            }

            sensor.Start();

            KinectAudioSource source = sensor.AudioSource;

            source.EchoCancellationMode = EchoCancellationMode.CancellationOnly; 
            source.AutomaticGainControlEnabled = false; 

            Microsoft.Speech.Recognition.RecognizerInfo ri = GetKinectRecognizer();
            Debug.WriteLine(ri.Id);
            if (ri == null)
            {
                Console.WriteLine("Could not find Kinect speech recognizer. Please refer to the sample requirements.");
                return;
            }

            int wait = 4;
            while (wait > 0)
            {
                Console.Write("Device will be ready for speech recognition in {0} second(s).\r", wait--);
                Thread.Sleep(1000);
            }
            //sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sre = new Microsoft.Speech.Recognition.SpeechRecognitionEngine(ri.Id);
            
                foreach(Microsoft.Speech.Recognition.Grammar g in grammars){
                    sre.LoadGrammar(g);
                }
                sre.SpeechRecognized += SreSpeechRecognized;

                using (Stream s = source.Start())
                {
                    sre.SetInputToAudioStream(
                        s, new Microsoft.Speech.AudioFormat.SpeechAudioFormatInfo(Microsoft.Speech.AudioFormat.EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                    Console.WriteLine("Recognizing speech. Say: 'red', 'green' or 'blue'. Press ENTER to stop");
                    sre.RecognizeAsync(Microsoft.Speech.Recognition.RecognizeMode.Multiple);




                Console.ReadLine();
                Console.WriteLine("Stopping recognizer ...");
                sre.RecognizeAsyncStop();

                }


                source.Stop();
                alertThread.Abort();
            
        }

        private void controllerFunction(object sender, Microsoft.Speech.Recognition.SpeechDetectedEventArgs e)
        {
            Console.WriteLine(1);
        }


        private void alertFunction()
        {
            while (sre == null) ;
            string s;
            string userSaid;
            int updateinterval = preferences.updateinterval;
            bool reply;
            while (true)
            {
                foreach (IJModule module in modules)
                {
                    s = module.alertFeed(out reply, out userSaid);
                    if (reply)
                        replyObj = module;
                    if (s != null)
                        say(s);
                }
                Thread.Sleep(updateinterval * 1000);
                replyObj = null;
            }
        }

        private void SreSpeechRecognized(object sender, Microsoft.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            string userSaid;
            if (!currentlyDictating && e.Result.Confidence > 0.7)
            {
                bool endConversation = true;
                bool passNextDictation = false;
                string result = e.Result.Semantics.Value.ToString();
                Console.WriteLine(result);
                string talkingTo = e.Result.Semantics.Value.ToString().Split(' ')[0];
                foreach (IJModule module in modules)
                {
                    //module.userSaid(e.Result.Semantics.Value.ToString().Split(' '),out endConversation);
                    if (talkingTo == module.moduleName())
                        say(module.userSaid(result, out endConversation, out passNextDictation, out userSaid));
                    while (passNextDictation)
                        say(module.userSaid(getNextDictation(), out endConversation, out passNextDictation, out userSaid));
                }
            }
        }

        private void startConversation(IJModule module)
        {
            
        }

        private void say(string s)
        {
            Console.WriteLine("Saying: " + s);
            if (s == null)
                return;
            foreach (IJModule module in modules)
            {
                Console.WriteLine(module.moduleName());
                module.quietMode();
            }
            Console.WriteLine("Stopping Async Recognition");
            sre.RecognizeAsyncCancel();
            Console.WriteLine("Speaking command sent...");
            voice.Speak(s);
            sre.RecognizeAsync(Microsoft.Speech.Recognition.RecognizeMode.Multiple);
            foreach (IJModule module in modules)
            {
                module.loudMode();
            }
        }

        private Microsoft.Speech.Recognition.RecognizerInfo GetKinectRecognizer()
        {

            Func<Microsoft.Speech.Recognition.RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return Microsoft.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        public void SreSpeechRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            lastDictated = e.Result.Text;
            Console.WriteLine(e.Result.Text);
        }

        private string getNextDictation()
        {
            currentlyDictating = true;
            dictation.RecognizeAsync(System.Speech.Recognition.RecognizeMode.Single);
            lastDictated = "";
            while (lastDictated == "") ;
            currentlyDictating = false;
            return lastDictated;
        }
    }
}


