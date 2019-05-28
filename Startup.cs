using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MSUSDemos.GraphCalendar.Services;

[assembly: FunctionsStartup(typeof(MSUSDemos.GraphCalendar.Startup))]

namespace MSUSDemos.GraphCalendar
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<GraphCalendarService, GraphCalendarService>();
        }
    }
}