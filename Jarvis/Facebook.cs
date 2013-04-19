using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Jarvis
{
    class Facebook : IJModule
    {

        private string rssFeedURL;
        private string latest;
        private bool userPresent;
        public Facebook(string rss)
        {
            rssFeedURL = rss;
            latest = getLatest();
        }

        private string getLatest()
        {
            
            var req = (HttpWebRequest)WebRequest.Create(rssFeedURL);
            req.KeepAlive = false;
            req.Method = "GET";
            req.UserAgent = "Fiddler";
            using (WebResponse rep = req.GetResponse())
            {
                var rssReader = XmlTextReader.Create(rep.GetResponseStream());

                if (rssReader.ReadToFollowing("item"))
                {
                    rssReader.ReadToFollowing("title");
                    return rssReader.ReadElementContentAsString();
                }
            }
            return latest;
        }

        public string alertFeed(out bool canReply, out string simulateResponse)
        {
            simulateResponse = null;
            canReply = false;
            if (userPresent)
            {
                string re = getLatest();
                if (re.Equals(latest))
                    return null;
                latest = re;
                canReply = true;
                return "Facebook Notification, " + re;
            }
            else
            {
                return null;
            }
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
            return null;
        }
        public string moduleName()
        {
            return "Facebook";
        }
        public string getGrammarFile()
        {
            return null;
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
    }
}
