using Google.Apis.Calendar.v3;
using System.Threading.Tasks;

namespace Mvc.Server.Contracts
{
    public interface IGoogleCalendarFactory
    {
        Task<CalendarService> GetClientAsync(string clientId, string clientSecret, string userName);
    }
}
