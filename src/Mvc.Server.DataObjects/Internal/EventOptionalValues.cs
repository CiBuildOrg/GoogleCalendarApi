using System;

namespace Mvc.Server.DataObjects.Internal
{
    /// <summary>
    /// There are several query Parameters that are optional this will allow you to send the ones you want.
    /// </summary>
    public class EventOptionalValues
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// Whether to include deleted events (with status equals "cancelled") in the result. Cancelled instances of recurring events
        /// (but not the underlying recurring event) will still be included if showDeleted and singleEvents are both False.
        /// If showDeleted and singleEvents are both True, only single instances of deleted events (but not the underlying 
        /// recurring events) are returned. Optional. The default is False.
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/events/list
        /// </summary>            
        public Boolean ShowDeleted { get; set; }


        /// <summary>
        /// Maximum number of entries returned on one result page. By default the value is 100 entries. The page size can never be larger than 2500 entries. Optional. 
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/events/list
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// Constructor sets up the default values, for things that can't be null.
        /// </summary>
        public EventOptionalValues()
        {
            MaxResults = 500;
            ShowDeleted = false;
        }
    }
}
