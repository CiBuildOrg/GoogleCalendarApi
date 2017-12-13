using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System.Threading.Tasks;

namespace Mvc.Server.Contracts
{
    public interface ICalendarHelperService
    {
        Task<CalendarsResource.ClearRequest> ClearPrimaryCalendarAsync(string id);
        Task<Calendar> CreateSecondaryCalendarAsync(Calendar body);
        Task<string> DeleteCalendarAsync(string id);
        Task<Calendar> GetCalendarByIdAsync(string id);
        Task<Calendar> PatchCalendarByIdAsync(string id, Calendar body);
        Task<Calendar> UpdateCalendarByIdAsync(string id, Calendar body);
    }
}
