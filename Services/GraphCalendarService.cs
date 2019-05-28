using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;

namespace MSUSDemos.GraphCalendar.Services {
    public class GraphCalendarService {
        private GraphServiceClient _Client;

        public GraphCalendarService(IHttpContextAccessor accessor) {
            var authToken = accessor.HttpContext;
        }

        public void Init(string accessToken) {
            _Client = CreateGraphServiceClient(accessToken);
        }

        public async Task<EventSummary> GetEventSummary(DateTime startDate, DateTime endDate) {
            var events = await GetCalendarEventsAsync(startDate, endDate);

            int numberOfEvents = 0;
            TimeSpan elapsed = new TimeSpan(0);
            TimeSpan currentDuration = new TimeSpan(0);
            DateTime currentStart = DateTime.MinValue;
            DateTime currentEnd = DateTime.MinValue; 
            
            var graphCount = new Dictionary<string, int>();

            foreach(var day in Enum.GetNames(typeof(System.DayOfWeek))) {
                graphCount[day] = 0;
            }

            bool hasSatya = false;
            bool hasJPC = false;
            
            foreach (var e in events) {
                // Ignore non-busy and all-day events
                if (e.ShowAs != FreeBusyStatus.Busy || e.IsAllDay.Value) {
                    continue;
                }

                var attendeeEmails = e.Attendees.Select(a => a.EmailAddress.Address.ToLower()).ToList();

                hasSatya = hasSatya || attendeeEmails.Contains("satyan@microsoft.com");
                hasJPC = hasJPC || attendeeEmails.Contains("jeanc@microsoft.com");

                var eventStart = GetDateTimeFromEvent(e.Start);
                var eventEnd = GetDateTimeFromEvent(e.End);

                numberOfEvents++;
                graphCount[Enum.GetName(typeof(System.DayOfWeek), eventStart.DayOfWeek)]++;

                if (eventStart > currentEnd) {
                    // New event sequence
                    elapsed = elapsed.Add(currentDuration);

                    currentStart = eventStart;
                    currentEnd = eventEnd;
                    
                } else if (eventEnd > currentEnd) {
                    // Extend event sequence
                    currentEnd = eventEnd;
                } else {
                    continue;
                }

                currentDuration = eventEnd - eventStart;
            }

            elapsed = elapsed.Add(currentDuration);

            return new EventSummary {
                DurationOfEvents = elapsed.TotalHours,
                NumberOfEvents = numberOfEvents,
                MeetingWithJPC = hasJPC,
                MeetingWithSatya = hasSatya,
                EventGraph = graphCount
            };
        }

        private DateTime GetDateTimeFromEvent(DateTimeTimeZone z) {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(z.TimeZone);
            var rawEventStart = DateTime.Parse(z.DateTime);
            var offset = new DateTimeOffset(rawEventStart, zone.GetUtcOffset(rawEventStart));
            return offset.ToUniversalTime().UtcDateTime;
        }

        public async Task<ICalendarCalendarViewCollectionPage> GetCalendarEventsAsync(DateTime startDate, DateTime endDate) {
            List<QueryOption> options = new List<QueryOption>
            {
                new QueryOption("startdatetime", startDate.ToString()),
                new QueryOption("enddatetime", endDate.ToString()),
                new QueryOption("$orderby", "start/dateTime"),
                new QueryOption("$top", "500")
                
            };

            return await _Client.Me.Calendar.CalendarView.Request(options).GetAsync();
        }

        private GraphServiceClient CreateGraphServiceClient(string accessToken) {
            var authenticationProvider = new DelegateAuthenticationProvider((request) =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                return Task.FromResult(0);
            });

            return new GraphServiceClient(authenticationProvider);
        }
    }
}