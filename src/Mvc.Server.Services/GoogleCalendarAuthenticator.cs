using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Mvc.Server.Contracts;
using Mvc.Server.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Mvc.Server.Services
{
    public class GoogleCalendarFactory : IGoogleCalendarFactory
    {
        private const string AppName = "Calendar API Sample";
        private readonly IAsyncLock Lock = new AsyncLock();
        private CalendarService _calendarService;

        private readonly string[] scopes = {
                CalendarService.Scope.Calendar  ,  // Manage your calendars
                CalendarService.Scope.CalendarReadonly    // View your Calendars
            };

        private readonly IDataStore _store;
        private readonly IAsyncLock _asyncLock;

        public GoogleCalendarFactory(IDataStore store, IAsyncLock asyncLock)
        {
            _store = store;
            _asyncLock = asyncLock;
        }

        /// <summary>
        /// Authenticate to Google Using Oauth2
        /// Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">From Google Developer console https://console.developers.google.com</param>
        /// <param name="clientSecret">From Google Developer console https://console.developers.google.com</param>
        /// <param name="userName">A string used to identify a user.</param>
        /// <returns></returns>
        public async Task<CalendarService> GetClientAsync(string clientId, string clientSecret, string userName)
        {
            if(_calendarService == null)
            {
                using(Lock.Lock())
                {
                    if (_calendarService == null)
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

                        _calendarService = new CalendarService(initializer);
                    }
                }
            }

            return _calendarService;
        }
    }
}
