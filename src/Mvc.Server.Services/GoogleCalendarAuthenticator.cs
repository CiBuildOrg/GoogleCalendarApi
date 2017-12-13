using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Mvc.Server.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class GoogleCalendarAuthenticator : IGoogleCalendarAuthenticator
    {
        private const string AppName = "Calendar API Sample";

        private readonly string[] scopes = {
                CalendarService.Scope.Calendar  ,  // Manage your calendars
                CalendarService.Scope.CalendarReadonly    // View your Calendars
            };

        private readonly IDataStore _store;
        public GoogleCalendarAuthenticator(IDataStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Authenticate to Google Using Oauth2
        /// Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">From Google Developer console https://console.developers.google.com</param>
        /// <param name="clientSecret">From Google Developer console https://console.developers.google.com</param>
        /// <param name="userName">A string used to identify a user.</param>
        /// <returns></returns>
        public async Task<CalendarService> AuthenticateOauthAsync(string clientId, string clientSecret, string userName)
        {
            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret }
                , scopes
                , userName
                , CancellationToken.None
                , _store);

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName,
            };

            return new CalendarService(initializer);
        }
    }
}
