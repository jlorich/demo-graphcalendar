using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;
using MSUSDemos.GraphCalendar.Services;
//D5rh7.rPz6K:O=B-0NXvmuMFhfcQoN??
namespace MSUSDemos.GraphCalendar
{
    public class GetCalendarSummary
    {

        GraphCalendarService _Service;

        public GetCalendarSummary(GraphCalendarService service) {
            _Service = service;
        }

        [FunctionName("GetCalendarSummary")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var token = req.Headers["Authorization"].ToString().Split(" ")[1];

            _Service.Init(token);

            using (StreamReader sr = new StreamReader(req.Body))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();

                var request = serializer.Deserialize<EventSummaryRequest>(reader);

                DateTime start;
                DateTime end;

                start = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                end = DateTime.Now.AddDays(6-(int)DateTime.Now.DayOfWeek);

                return new JsonResult(await _Service.GetEventSummary(start, end));
            }
        }
    }
}
