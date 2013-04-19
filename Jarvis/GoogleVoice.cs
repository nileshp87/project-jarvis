using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpVoice;
using System.IO;
using System.Collections;


namespace Jarvis
{
    class GoogleVoice : IJModule
    {
        Voice voiceConnection;
        SharpVoice.Message mostRecent;
        private Hashtable numbersNames;
        private Hashtable namesNumbers;
        private bool userPresent;
        private bool isSending;
        private string sendingTo;
        
        public GoogleVoice(string username, string password)
        {
            try
            {
                voiceConnection = new Voice(username, password);
                mostRecent = new SharpVoice.Message();
                mostRecent.ID = "";
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed To Login: "+e.Message);
            }
            userPresent = true;
        }

        public GoogleVoice(string username, string password, string addressBook)
        {
            try
            {
                voiceConnection = new Voice(username, password);
                loadAddressBook(addressBook);
                mostRecent = new SharpVoice.Message();
                mostRecent.ID = "";
                generateGrammar();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed To Login: " + e.Message);
            }
            userPresent = true;
        }

        private int loadAddressBook(string s){
            numbersNames = new Hashtable();
            namesNumbers = new Hashtable();
            int count = 0;
            string line;
            FileInfo fi = new FileInfo(s);
            StreamReader reader = fi.OpenText();
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                if (items.Length > 36)
                {
                    if (items[36] == "Mobile")
                    {
                        importContact(items[0],items[37]);
                        count++;
                        continue;
                    }
                    if (items[38] == "Mobile")
                    {
                        importContact(items[0],items[39]);
                        count++;
                        continue;
                    }
                    if (items[40] == "Mobile")
                    {
                        importContact(items[0],items[40]);
                        count++;
                        continue;
                    }
                }
            }
            return count;
        }

        private void importContact(string name, string number)
        {
            if (number.Contains(":::"))
                number = number.Substring(0, number.IndexOf(':'));
            char[] num = number.ToCharArray();
            string formattedNumber = "(";
            number = "";
            foreach(char c in num){
                if (Char.IsDigit(c))
                    number += c;
            }
            if (number.Length == 10)
                number = "1" + number;
            if (number.Length == 11)
                formattedNumber = "(" + number.Substring(1, 3) + ") " + number.Substring(4, 3) + "-"+number.Substring(7, 4);

            namesNumbers.Add(name, formattedNumber);
            numbersNames.Add(formattedNumber, name);

        }

        public string alertFeed(out bool canReply, out string simulateResponse)
        {
            simulateResponse = null;
            canReply = false;
            if (userPresent)
            {
                if (mostRecent.ID != voiceConnection.mostRecent.ID)
                {
                    canReply = true;
                    mostRecent = voiceConnection.mostRecent;
                    return mostRecent.displayNumber + " said " + mostRecent.Text;
                }
                
            }
            return null;
        }
        public void userEntered()
        {
            userPresent = true;
        }
        public void userLeft()
        {
            userPresent = false;
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
            endConversation = true;
            passNextDictation = false;
            if (!isSending)
            {
                string[] arguments = s.Split(' ');
                string command = arguments[1];
                if (command == "send" || command == "reply")
                {
                    passNextDictation = true;
                    endConversation = false;
                    isSending = true;
                }
                if (command == "reply")
                {
                    if(namesNumbers.ContainsKey(mostRecent.displayNumber))
                        sendingTo = (string) namesNumbers[mostRecent.displayNumber];
                }
                if (command == "send")
                    sendingTo = arguments[2];
                if (command == "call")
                    return "Not yet implemented, sorry";
                return "What would you like to " + command + "?";
            }
            if (isSending)
            {
                sendText(sendingTo, s);
                isSending = false;
                sendingTo = "";
                return null;
            }
            return null;
        }
        private bool sendText(string number, string text)
        {
            try
            {
                Console.WriteLine(number + ": " + text);
                voiceConnection.SendSMS(number, text);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Send Text Failed: " + e.Message);
                return false;
            }
        }
        public string moduleName()
        {
            return "GoogleVoice";
        }
        public string getGrammarFile()
        {
            return "GoogleVoice.srgs";
        }
        public void otherConversation()
        {
            userLeft();
        }
        public void endOtherConversation()
        {
            userEntered();
        }
        public void quietMode()
        {
            userLeft();
        }
        public void loudMode()
        {
            userEntered();
        }
        private string unformatNumber(string s)
        {
            return s.Substring(1, 3) + s.Substring(6, 3) + s.Substring(10);
        }
        private void generateGrammar()
        {
            using (System.IO.StreamReader initial = new System.IO.StreamReader(System.Environment.CurrentDirectory + "\\GoogleVoiceInit.srgs"))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.Environment.CurrentDirectory + "\\GoogleVoice.srgs", false))
                {
                    while (!initial.EndOfStream)
                        file.WriteLine(initial.ReadLine());
                    file.WriteLine("<rule id=\"name\"><one-of>");
                    foreach (string name in namesNumbers.Keys)
                        file.WriteLine("<item>" + name + "<tag>out=\" " + unformatNumber((string)namesNumbers[name]) + "\";</tag></item>");
                    file.WriteLine("</one-of></rule></grammar>");
                }
            }
    
        }
    }
}
