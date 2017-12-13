using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public abstract class BaseService
    {
        private readonly IGoogleCalendarFactory _factory;
        private AppOptions _appOptions;

        protected BaseService(IGoogleCalendarFactory factory, IOptions<AppOptions> configuration)
        {
            _factory = factory;
            _appOptions = configuration.Value;
        }

        protected async Task<CalendarService> GetService()
        {
            return await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
                _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);
        }
    }
}
