using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Mvc.Server.DataObjects.Internal;

namespace Mvc.Server.Contracts
{
    public interface ICalendarListService
    {
        CalendarListEntry CreateNewCalendar(CalendarService service, CalendarListEntry body);
        string DeleteCalendar(CalendarService service, string id);
        CalendarListEntry GetCalendarById(CalendarService service, string id);
        CalendarList ListCalendars(CalendarService service, CalendarOptionalValues optionalValues);
        CalendarListEntry Update(CalendarService service, string id, CalendarListEntry body);
        CalendarListEntry UpdateCalendar(CalendarService service, string id, CalendarListEntry body);
        Channel WatchCalendarListChanges(CalendarService service, Channel channel);
    }
}
