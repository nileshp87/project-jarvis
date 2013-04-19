using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using Google.GData.Calendar;
using Google.GData.Extensions;
using Google.GData.Client;
using Google.GData.AccessControl;

namespace Jarvis
{
    class GoogleCalendar : IJModule
    {
        private CalendarService calendar;
        private List<EventEntry> events;
        private bool userPresent;
        private int numHours;
        private LinkedList<string> alertedOf;
        private DateTime lastCleared;
        public GoogleCalendar(string username, string password, int numHours)
        {
            calendar = CalendarHelper.GetService("Jarvis", username, password);
            getCurrentEvents();
            this.numHours = numHours;
            userPresent = true;
            alertedOf = new LinkedList<string>();
            lastCleared = DateTime.Now;
        }
        public string alertFeed(out bool canReply, out string simulateResponse)
        {
            simulateResponse = null;
            canReply = false;
            string s = "";
            getCurrentEvents();
            if (lastCleared.Subtract(DateTime.Now.TimeOfDay).TimeOfDay == TimeSpan.FromHours(numHours))
                alertedOf.Clear();
            if (!userPresent)
                return "";
            foreach (EventEntry e in events)
            {
                if (e.Times.First().StartTime.Subtract(DateTime.Now.TimeOfDay).TimeOfDay < TimeSpan.FromHours(numHours) && !alertedOf.Contains(e.EventId))
                {
                    s += e.Title.Text + " in " + e.Times.First().StartTime.Subtract(DateTime.Now.TimeOfDay).TimeOfDay.ToString() + ". ";
                    alertedOf.AddFirst(e.EventId);
                }
            }
            if (s != "")
                return s;
            else
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
        private void getCurrentEvents(){
            events  = CalendarHelper.GetAllEvents(calendar, DateTime.Now, DateTime.Now.AddHours(24)).ToList();
        }
        private IEnumerable<EventEntry> getFirst(int i)
        {
            return events.Take(i);
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
            return null;
        }
        public string getGrammarFile()
        {
            return null;
        }
        public string moduleName()
        {
            return "GoogleCalendar";
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
    public class CalendarHelper
    {
        public string ApplicationName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public static CalendarService GetService(string applicationName, string userName, string password)
        {
            CalendarService service = new CalendarService(applicationName);
            service.setUserCredentials(userName, password);
            return service;
        }

        public static IEnumerable<EventEntry> GetAllEvents(CalendarService service, DateTime? startData, DateTime? endData)
        {
            // Create the query object:
            EventQuery query = new EventQuery();
            query.Uri = new Uri("http://www.google.com/calendar/feeds/" +
                  service.Credentials.Username + "/private/full");
            if (startData != null)
                query.StartTime = startData.Value;
            if (endData != null)
                query.EndTime = endData.Value;
            // Tell the service to query:
            EventFeed calFeed = service.Query(query);
            return calFeed.Entries.Cast<EventEntry>();
        }
        public static void AddEvent(CalendarService service, string title, string contents, string location, DateTime startTime, DateTime endTime)
        {
            EventEntry entry = new EventEntry();

            // Set the title and content of the entry.
            entry.Title.Text = title;
            entry.Content.Content = contents;

            // Set a location for the event.
            Where eventLocation = new Where();
            eventLocation.ValueString = location;
            entry.Locations.Add(eventLocation);

            When eventTime = new When(startTime, endTime);
            entry.Times.Add(eventTime);

            Uri postUri = new Uri
            ("http://www.google.com/calendar/feeds/default/private/full");

            // Send the request and receive the response:
            AtomEntry insertedEntry = service.Insert(postUri, entry);
        }  
    }
}
