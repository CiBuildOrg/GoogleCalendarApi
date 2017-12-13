using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Internal;
using System;
using System.Collections.Generic;

namespace Mvc.Server.Services
{
    public class CalendarListService : ICalendarListService
    {
        private readonly IGoogleCalendarFactory _factory;

        public CalendarListService(IGoogleCalendarFactory factory)
        {
            _factory = factory;
        }

        public CalendarList ListCalendars(CalendarService service, CalendarOptionalValues optionalValues)
        {
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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Returns an entry on the user's calendar list. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/get
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public CalendarListEntry GetCalendarById(CalendarService service, string id)
        {
            return service.CalendarList.Get(id).Execute();
        }

        /// <summary>
        /// Updates an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/patch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = CalendarListHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public CalendarListEntry UpdateCalendar(CalendarService service, string id, CalendarListEntry body)
        {
            return service.CalendarList.Patch(body, id).Execute();
        }

        /// <summary>
        /// Updates an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/update
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = CalendarListHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public CalendarListEntry Update(CalendarService service, string id, CalendarListEntry body)
        {
            return service.CalendarList.Update(body, id).Execute();
        }

        /// <summary>
        /// Watch for changes to CalendarList resources.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/watch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="channel">Channel definaing what is to be watched.</param>
        /// <returns>Channel being watched </returns>
        public Channel WatchCalendarListChanges(CalendarService service, Channel channel)
        {
            return service.CalendarList.Watch(channel).Execute();
        }

        /// <summary>
        /// Adds an entry to the user's calendar list.  
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/insert
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="body">New calendar information</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public CalendarListEntry CreateNewCalendar(CalendarService service, CalendarListEntry body)
        {
            return service.CalendarList.Insert(body).Execute();
        }

        /// <summary>
        /// Deletes an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/delete
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public string DeleteCalendar(CalendarService service, string id)
        {
            return service.CalendarList.Delete(id).Execute();
        }
    }
}
