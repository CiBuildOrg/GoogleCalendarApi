using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using Mvc.Server.Contracts;
using Mvc.Server.DataObjects.Configuration;
using Mvc.Server.DataObjects.Request;
using Mvc.Server.DataObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class GoogleService
    {
        private readonly IEventService _eventService;
        private readonly ICalendarListService _calendarListService;
        private readonly ICalendarHelperService _calendarHelperService;
        private readonly string _calendarId;
        public GoogleService(IEventService eventService, ICalendarListService calendarListService,
            ICalendarHelperService calendarHelperService, IOptions<AppOptions> configuration)
        {
            _eventService = eventService;
            _calendarHelperService = calendarHelperService;
            _calendarListService = calendarListService;
            _calendarId = configuration.Value.GoogleCalendar.CalendarId;
        }

        public async Task<ServiceResponse<EventDto>> Create(CreateEventDto createRequest, string timezone)
        {
            var @event = await _eventService.AddEventAsync(_calendarId, new Event
            {
                Summary = createRequest.Title,
                Description = createRequest.Description,
                Start = new EventDateTime
                {
                    DateTime = createRequest.Start,
                    TimeZone = timezone
                },
                End = new EventDateTime
                {
                    DateTime = createRequest.End,
                    TimeZone = timezone
                }
            });

            if(@event == null)
            {
                return ServiceResponse<EventDto>.Fail;
            }

            return new ServiceResponse<EventDto>
            {
                Result = EventsToDto(@event),
                Success = true
            };
        }

        public async Task<EventDto> Get(string id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(_calendarId, id);
            return EventsToDto(eventItem);
        }

        public async Task<List<EventDto>> Get(DateTime start, DateTime end)
        {
            var eventList = await _eventService.GetEventListAsync(_calendarId, new DataObjects.Internal.EventOptionalValues
            {
                EndDate = end,
                StartDate = start
            });

            return eventList.Items.Select(EventsToDto).ToList();
        }

        private EventDto EventsToDto(Event item)
        {
            return new EventDto
            {
                Id = item.Id,
                Description = item.Description,
                End = item.End.DateTime,
                Start = item.Start.DateTime,
                Title = item.Summary
            };
        }
    }
}
