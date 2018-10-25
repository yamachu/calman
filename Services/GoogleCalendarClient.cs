using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalMan.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CalMan.Services
{
    public class GoogleCalendarClient
    {
        private static GoogleCalendarClient instance = new GoogleCalendarClient();

        public string CalendarId { get; set; } = "primary";
        public void SetCredentials(string clientId, string clientSecret, IDataStore store)
        {
            client = new Lazy<Task<CalendarService>>(async () =>
            {
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                        },
                        new[] { CalendarService.Scope.Calendar },
                        "user", CancellationToken.None, store);

                return new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });
            });
        }

        private Lazy<Task<CalendarService>> client = null;

        public static GoogleCalendarClient Instance
        {
            get
            {
                return instance;
            }
        }
        private GoogleCalendarClient() { }

        public async Task<IEnumerable<string>> InviteUsers(string eventUrl, IEnumerable<string> emails)
        {
            var _client = await client.Value;

            var eventParam = ParseEventParamFromUrl(eventUrl);

            var ev = await _client.Events.Get(this.CalendarId, eventParam.eventId).ExecuteAsync();
            if (ev.Attendees == null) {
                ev.Attendees = new List<EventAttendee>();
            }
            var newEmails = emails.Except(ev.Attendees.Select(v => v.Email)).ToList();
            var newAttendees = newEmails.Select(v => new EventAttendee
            {
                Email = v
            });

            foreach (var attendee in newAttendees)
            {
                ev.Attendees.Add(attendee);
            }

            // ref: https://stackoverflow.com/questions/10332255/google-calendar-api-v3-update-event
            if (ev.Sequence != null) {
                ev.Sequence++;
            } else {
                ev.Sequence = 0;
            }
            var updated = await _client.Events.Update(ev, this.CalendarId, eventParam.eventId).ExecuteAsync();
            var processedEmails = updated.Attendees.Select(v => v.Email).Intersect(newEmails).ToList();

            return processedEmails;
        }

        // Date周りは要確認
        public async Task<IEnumerable<string>> ListingEvents(string targetEmail, DateTimeOffset date)
        {
            var _client = await client.Value;

            var req = _client.Events.List(targetEmail);
            req.TimeMax = date.AddDays(1).DateTime;
            req.TimeMin = date.DateTime;
            var ev = req.Execute();
            foreach (var e in ev.Items)
            {
                System.Console.WriteLine(e.Id);
                System.Console.WriteLine(e.Summary);
            }

            throw new NotImplementedException();
        }

        static string AppendDummyChars(string baseString) => baseString.PadRight(baseString.Length + (4 - baseString.Length % 4), '=');

        static (string eventId, string calendarOwnerEmail) ParseEventParamFromUrl(string url)
        {
            var pathParam = url.Split('/').LastOrDefault()?.Split('?').FirstOrDefault();
            var params_ = Encoding.GetEncoding("UTF-8").GetString(Convert.FromBase64String(AppendDummyChars(pathParam))).Split();
            return (params_[0], params_[1]);
        }
    }
}