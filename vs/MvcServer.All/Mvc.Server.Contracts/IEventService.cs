using Google.Apis.Calendar.v3.Data;
using Mvc.Server.DataObjects.Internal;
using System.Threading.Tasks;

namespace Mvc.Server.Contracts
{
    public interface IEventService
    {
        Task<Event> AddEventAsync(string id, Event body);
        Task<string> DeleteEventAsync(string id, string eventid);
        Task<Event> GetEventByIdAsync(string id, Event body);
        Task<Event> GetEventByIdAsync(string id, string eventid);
        Task<Events> GetEventInstancesAsync(string id, string eventid);
        Task<Events> GetEventListAsync(string id, EventOptionalValues optionalValues);
        Task<Event> MoveEventToAnotherCalendarAsync(string id, string eventId, string destination);
        Task<Event> PathEventAsync(string id, string eventid, Event body);
        Task<Event> QuickAddEventAsync(string id, string text);
        Task<Event> UpdateEventAsync(string id, string eventid, Event body);
        Task<Channel> WatchAsync(string id, Channel channel);
    }
}
