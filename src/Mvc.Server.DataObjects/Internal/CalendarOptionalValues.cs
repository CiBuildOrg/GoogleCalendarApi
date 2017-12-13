using Google.Apis.Calendar.v3;
using System;

namespace Mvc.Server.DataObjects.Internal
{
    /// <summary>
    /// There are several query Parameters that are optional this will allow you to send the ones you want.
    /// </summary>
    public class CalendarOptionalValues
    {
        /// <summary>
        /// Whether to include deleted calendar list entries in the result. Optional. The default is False. 
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendarList/list
        /// </summary>            
        public Boolean ShowDeleted { get; set; }

        /// <summary>
        /// Dimension or metric filters that restrict the data returned for your request. 
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendarList/list
        /// </summary>
        public Boolean ShowHidden { get; set; }

        /// <summary>
        /// Maximum number of entries returned on one result page. By default the value is 100 entries. The page size can never be larger than 250 entries. Optional. 
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendarList/list
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// The minimum access role for the user in the returned entires. Optional. The default is no restriction. 
        ///Acceptable values are: •"freeBusyReader": The user can read free/busy information. 
        ///•"owner": The user can read and modify events and access control lists. 
        ///•"reader": The user can read events that are not private. 
        ///•"writer": The user can read and modify events. 
        /// Documentation: https://developers.google.com/google-apps/calendar/v3/reference/calendarList/list
        /// </summary>
        public CalendarListResource.ListRequest.MinAccessRoleEnum? MinAccessRole { get; set; }

        /// <summary>
        /// Constructor sets up the default values, for things that can't be null.
        /// </summary>
        public CalendarOptionalValues()
        {
            MaxResults = 100;
            ShowDeleted = false;
            ShowHidden = false;
            MinAccessRole = null;
        }
    }
}
