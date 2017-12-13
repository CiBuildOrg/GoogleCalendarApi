using Mvc.Server.DataObjects.Request;
using System;

namespace Mvc.Server.DataObjects.Response
{
    public class EventDto : CreateEventDto
    {
        public string Id { get; set; }
    }
}
