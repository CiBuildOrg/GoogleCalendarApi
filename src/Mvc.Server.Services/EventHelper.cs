using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Mvc.Server.DataObjects.Internal;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class EventService : BaseService, IEventService
    {
        protected EventService(IGoogleCalendarFactory factory, IOptions<AppOptions> configuration) 
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Deletes an event
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/delete
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventid">Event identifier.</param>
        /// <returns>If successful, this method returns an empty response body. </returns>
        public async Task<string> DeleteEventAsync(string id, string eventid)
        {
            var service = await GetService();
            return service.Events.Delete(id, eventid).Execute();
        }

        /// <summary>
        /// Returns an event.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/get
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventid">Event identifier.</param>
        /// <returns>Events resorce: https://developers.google.com/google-apps/calendar/v3/reference/events#resource </returns>
        public async Task<Event> GetEventByIdAsync(string id, string eventid)
        {
            var service = await GetService();
            return service.Events.Get(id, eventid).Execute();
        }

        /// <summary>
        /// Imports an event. This operation is used to add a private copy of an existing event to a calendar. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/import
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">an event</param>
        /// <returns>Events resorce: https://developers.google.com/google-apps/calendar/v3/reference/events#resource </returns>
        public async Task<Event> GetEventByIdAsync(string id, Event body)
        {
            var service = await GetService();
            return service.Events.Import(body, id).Execute();
        }

        /// <summary>
        /// Adds an entry to the user's calendar list.  
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/calendarList/insert
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="body">an event</param>
        /// <returns>event resorce: https://developers.google.com/google-apps/calendar/v3/reference/events#resource </returns>
        public async Task<Event> AddEventAsync(string id, Event body)
        {
            var service = await GetService();
            return service.Events.Insert(body, id).Execute();
        }

        /// <summary>
        /// Returns instances of the specified recurring event. 
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/instances
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventid">Event id</param>
        /// <returns>If successful, this method returns a response body : https://developers.google.com/google-apps/calendar/v3/reference/events/instances </returns>
        public async Task<Events> GetEventInstancesAsync(string id, string eventid)
        {
            var service = await GetService();
            return service.Events.Instances(id, eventid).Execute();
        }

        public async Task<Events> GetEventListAsync(string id, EventOptionalValues optionalValues)
        {
            var service = await GetService();
            var request = service.Events.List(id);

            if (optionalValues == null)
            {
                request.MaxResults = 100;
            }
            else
            {
                request.MaxResults = optionalValues.MaxResults;
                request.ShowDeleted = optionalValues.ShowDeleted;
                request.TimeMin = optionalValues.StartDate.ToUniversalTime();
                request.TimeMax = optionalValues.EndDate.ToUniversalTime();
            }

            return ProcessResults(request);
        }


        // Just loops though getting all the rows.  
        private Events ProcessResults(EventsResource.ListRequest request)
        {
            var result = request.Execute();
            var allRows = new List<Event>();

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
        /// Moves an event to another calendar, i.e. changes an event's organizer.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/move
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="destination">Calendar identifier of the target calendar where the event is to be moved to.</param>
        /// <returns>event resorce:https://developers.google.com/google-apps/calendar/v3/reference/events#resource </returns>
        public async Task<Event> MoveEventToAnotherCalendarAsync(string id, string eventId,
            string destination)
        {
            var service = await GetService();
            return service.Events.Move(id, eventId, destination).Execute();
        }

        /// <summary>
        /// Updates an event. This method supports patch semantics
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/events/patch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventid">Event identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoEventHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>event resorce:https://developers.google.com/google-apps/calendar/v3/reference/events#resource  </returns>
        public async Task<Event> PathEventAsync(string id, string eventid, Event body)
        {
            var service = await GetService();
            return service.Events.Patch(body, id, eventid).Execute();
        }

        /// <summary>
        /// Creates an event based on a simple text string
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/events/quickAdd
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="text">The text describing the event to be created.</param>
        /// <returns>event resorce:https://developers.google.com/google-apps/calendar/v3/reference/events#resource  </returns>
        public async Task<Event> QuickAddEventAsync(string id, string text)
        {
            var service = await GetService();
            return service.Events.QuickAdd(id, text).Execute();
        }

        /// <summary>
        /// Updates an event.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/update
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="eventid">Event identifier.</param>
        /// <param name="body">Changes you want to make:  Use var body = DaimtoEventHelper.get(service,id);  
        ///                    to get body then change that and pass it to the method.</param>
        /// <returns>event resorce:https://developers.google.com/google-apps/calendar/v3/reference/events#resource  </returns>
        public async Task<Event> UpdateEventAsync(string id, string eventid, Event body)
        {
            var service = await GetService();
            return service.Events.Update(body, id, eventid).Execute();
        }

        /// <summary>
        /// Watch for changes to CalendarList resources.
        /// Documentation:https://developers.google.com/google-apps/calendar/v3/reference/events/watch
        /// </summary>
        /// <param name="service">Valid Autenticated Calendar service</param>
        /// <param name="id">Calendar identifier.</param>
        /// <param name="channel">Channel definaing what is to be watched.</param>
        /// <returns>Channel being watched </returns>
        public async Task<Channel> WatchAsync(string id, Channel channel)
        {
            var service = await GetService();
            return service.Events.Watch(channel, id).Execute();
        }
    }
}
