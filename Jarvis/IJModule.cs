using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis
{
    interface IJModule
    {
        string alertFeed(out bool canReply, out string simulateResponse);
        void userEntered();
        void userLeft();
        void nightMode();
        void dayMode();
        string moduleName();
        string userSaid(string s, out bool endConversation, out bool passNextDictation, out string simulateResponse);
        string getGrammarFile();
        void otherConversation();
        void endOtherConversation();
        void quietMode();
        void loudMode();
    }
}
