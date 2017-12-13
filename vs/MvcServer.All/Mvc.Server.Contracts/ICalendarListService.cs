using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Mvc.Server.DataObjects.Internal;
using System.Threading.Tasks;

namespace Mvc.Server.Contracts
{
    public interface ICalendarListService
    {
        Task<CalendarListEntry> CreateNewCalendarAsync(CalendarListEntry body);
        Task<string> DeleteCalendarAsync(string id);
        Task<CalendarListEntry> GetCalendarByIdAsync(string id);
        Task<CalendarList> ListCalendarsAsync(CalendarOptionalValues optionalValues);
        Task<CalendarListEntry> UpdateAsync(string id, CalendarListEntry body);
        Task<CalendarListEntry> UpdateCalendarAsync(string id, CalendarListEntry body);
        Task<Channel> WatchCalendarListChangesAsync(Channel channel);
    }
}
