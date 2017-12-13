using Google.Apis.Calendar.v3;
using System.Threading.Tasks;

namespace Mvc.Server.Services.Contracts
{
    public interface IGoogleCalendarAuthenticator
    {
        Task<CalendarService> AuthenticateOauthAsync(string clientId, string clientSecret, string userName);
    }
}
