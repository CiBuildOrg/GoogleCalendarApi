using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using System;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class CalendarHelperService : BaseService, ICalendarHelperService
    {
        public CalendarHelperService(IGoogleCalendarFactory factory, IOptions<AppOptions> configuration)
            : base(factory, configuration)
        {

        }

        /// <summary>
        /// Clears a primary calendar. This operation deletes all data associated with the primary calendar of an account and cannot be undone.
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendars/clear
        /// </summary>
        /// <param name="id">If successful, this method returns an empty response body.</param>
        /// <returns></returns>
        public async Task<CalendarsResource.ClearRequest> ClearPrimaryCalendarAsync(string id)
        {
            var service = await GetService();
            return service.Calendars.Clear(id);
        }

        /// <summary>
        /// Deletes a secondary calendar.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/delete
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public async Task<string> DeleteCalendarAsync(string id)
        {
            var service = await GetService();
            return service.Calendars.Delete(id).Execute();
        }

        /// <summary>
        /// Returns metadata for a calendar. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/get
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public async Task<Calendar> GetCalendarByIdAsync(string id)
        {
            var service = await GetService();
            return service.Calendars.Get(id).Execute();
        }

        /// <summary>
        /// Creates a secondary calendar. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/insert
        /// </summary>
        /// <param name="body">Secondary calendar information</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public async Task<Calendar> CreateSecondaryCalendarAsync(Calendar body)
        {
            var service = await GetService();
            return service.Calendars.Insert(body).Execute();
        }

        /// <summary>
        /// Updates metadata for a calendar. This method supports patch semantics.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/patch
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoCalendarHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public async Task<Calendar> PatchCalendarByIdAsync(string id, Calendar body)
        {
            var service = await GetService();
            return service.Calendars.Patch(body, id).Execute();
        }

        /// <summary>
        /// Updates metadata for a calendar.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/update
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoCalendarHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<Calendar> UpdateCalendarByIdAsync(string id, Calendar body)
        {
            var service = await GetService();
            return service.Calendars.Update(body, id).Execute();
        }
    }
}
