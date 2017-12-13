using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using Mvc.Server.DataObjects.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class CalendarListService : BaseService,  ICalendarListService
    {
        public CalendarListService(IGoogleCalendarFactory factory, IOptions<AppOptions> configuration)
            : base(factory, configuration)
        {
        }

        public async Task<CalendarList> ListCalendarsAsync(CalendarOptionalValues optionalValues)
        {
            var service = await GetService();
            var request = service.CalendarList.List();

            if (optionalValues == null)
            {
                request.MaxResults = 100;
            }
            else
            {
                request.MaxResults = optionalValues.MaxResults;
                request.ShowDeleted = optionalValues.ShowDeleted;
                request.ShowHidden = optionalValues.ShowHidden;
                request.MinAccessRole = optionalValues.MinAccessRole;
            }

            return ProcessResults(request);
        }

        // Just loops though getting all the rows.  
        private CalendarList ProcessResults(CalendarListResource.ListRequest request)
        {
            var result = request.Execute();
            var allRows = new List<CalendarListEntry>();

            //// Loop through until we arrive at an empty page
            while (result.Items != null)
            {
                //Add the rows to the final list
                allRows.AddRange(result.Items);

                // We will know we are on the last page when the next page token is
                // null.
                // If this is the case, break.
                if (result.NextPageToken == null)
                {
                    break;
                }
                // Prepare the next page of results
                request.PageToken = result.NextPageToken;

                // Execute and process the next page request
                result = request.Execute();

            }

            var allData = result;

            allData.Items = allRows;
            return allData;
        }

        /// <summary>
        /// Returns an entry on the user's calendar list. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/get
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<CalendarListEntry> GetCalendarByIdAsync(string id)
        {
            var service = await GetService();
            return service.CalendarList.Get(id).Execute();
        }

        /// <summary>
        /// Updates an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/patch
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = CalendarListHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<CalendarListEntry> UpdateCalendarAsync(string id, CalendarListEntry body)
        {
            var service = await GetService();
            return service.CalendarList.Patch(body, id).Execute();
        }

        /// <summary>
        /// Updates an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/update
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = CalendarListHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<CalendarListEntry> UpdateAsync(string id, CalendarListEntry body)
        {
            var service = await GetService();
            return service.CalendarList.Update(body, id).Execute();
        }

        /// <summary>
        /// Watch for changes to CalendarList resources.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/watch
        /// </summary>
        /// <param name="channel">Channel definaing what is to be watched.</param>
        /// <returns>Channel being watched </returns>
        public async Task<Channel> WatchCalendarListChangesAsync(Channel channel)
        {
            var service = await GetService();
            return service.CalendarList.Watch(channel).Execute();
        }

        /// <summary>
        /// Adds an entry to the user's calendar list.  
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/insert
        /// </summary>
        /// <param name="body">New calendar information</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<CalendarListEntry> CreateNewCalendarAsync(CalendarListEntry body)
        {
            var service = await GetService();
            return service.CalendarList.Insert(body).Execute();
        }

        /// <summary>
        /// Deletes an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/delete
        /// </summary>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public async Task<string> DeleteCalendarAsync(string id)
        {
            var service = await GetService();
            return service.CalendarList.Delete(id).Execute();
        }
    }
}
