using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using Mvc.Server.DataObjects.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class CalendarListService : ICalendarListService
    {
        private readonly IGoogleCalendarFactory _factory;
        private AppOptions _appOptions;
        public CalendarListService(IGoogleCalendarFactory factory, IOptions<AppOptions> configuration)
        {
            _factory = factory;
            _appOptions = configuration.Value;
        }

        public async Task<CalendarList> ListCalendarsAsync(CalendarOptionalValues optionalValues)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
                _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

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
        public async Task<CalendarListEntry> GetCalendarByIdAsync(string id)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
              _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

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
        public async Task<CalendarListEntry> UpdateCalendarAsync(string id, CalendarListEntry body)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
              _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

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
        public async Task<CalendarListEntry> UpdateAsync(string id, CalendarListEntry body)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
              _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

            return service.CalendarList.Update(body, id).Execute();
        }

        /// <summary>
        /// Watch for changes to CalendarList resources.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/watch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="channel">Channel definaing what is to be watched.</param>
        /// <returns>Channel being watched </returns>
        public async Task<Channel> WatchCalendarListChangesAsync(Channel channel)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
                _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

            return service.CalendarList.Watch(channel).Execute();
        }

        /// <summary>
        /// Adds an entry to the user's calendar list.  
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/insert
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="body">New calendar information</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public async Task<CalendarListEntry> CreateNewCalendarAsync(CalendarListEntry body)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
              _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

            return service.CalendarList.Insert(body).Execute();
        }

        /// <summary>
        /// Deletes an entry on the user's calendar list.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/delete
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public async Task<string> DeleteCalendarAsync(string id)
        {
            var service = await _factory.GetClientAsync(_appOptions.GoogleCalendar.ClientId,
                 _appOptions.GoogleCalendar.ClientSecret, _appOptions.GoogleCalendar.Username);

            return service.CalendarList.Delete(id).Execute();
        }
    }


    class CalendarHelperService
    {
       
        /// <summary>
        /// Clears a primary calendar. This operation deletes all data associated with the primary calendar of an account and cannot be undone.
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendars/clear
        /// </summary>
        /// <param name="service">Calendar identifier</param>
        /// <param name="id">If successful, this method returns an empty response body.</param>
        /// <returns></returns>
        public static CalendarsResource.ClearRequest ClearPrimaryCalendar(CalendarService service, string id)
        {
            try
            {
                return service.Calendars.Clear(id);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Deletes a secondary calendar.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/delete
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public static string DeleteCalendar(CalendarService service, string id)
        {
            try
            {
                return service.Calendars.Delete(id).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Returns metadata for a calendar. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/get
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public static Calendar GetCalendarById(CalendarService service, string id)
        {
            try
            {
                return service.Calendars.Get(id).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates a secondary calendar. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/insert
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="body">Secondary calendar information</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public static Calendar CreateSecondaryCalendar(CalendarService service, Calendar body)
        {
            try
            {
                return service.Calendars.Insert(body).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates metadata for a calendar. This method supports patch semantics.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/patch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoCalendarHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>Calendar resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendars#resource </returns>
        public static Calendar PatchCalendarById(CalendarService service, string id, Calendar body)
        {

            try
            {
                return service.Calendars.Patch(body, id).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates metadata for a calendar.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendars/update
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoCalendarHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>CalendarList resorce: https://developers.google.com/google-apps/calendar/v3/reference/calendarList#resource </returns>
        public static Calendar UpdateCalendarById(CalendarService service, string id, Calendar body)
        {

            try
            {
                return service.Calendars.Update(body, id).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
