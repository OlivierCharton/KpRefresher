using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using KpRefresher.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KpRefresher.Services
{
    public class RaidService
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Logger _logger;

        private static readonly List<string> _raidBossNames = new() { "sabetha", "matthias", "xera", "deimos", "voice_in_the_void", "qadim", "qadim_the_peerless" };
        private const string _kpMeBaseUrl = "https://killproof.me/";

        private List<string> _baseRaidClears { get; set; }
        private DateTime? _lastRefresh { get; set; }
        private DateTime? _refreshAvailable => _lastRefresh?.AddHours(1);

        public bool RefreshScheduled { get; set; }
        public double ScheduleTimer { get; set; }
        public double ScheduleTimerEndValue { get; set; }

        public RaidService(ModuleSettings moduleSettings, Gw2ApiManager gw2ApiManager, Logger logger)
        {
            _moduleSettings = moduleSettings;
            _gw2ApiManager = gw2ApiManager;
            _logger = logger;
        }

        /// <summary>
        /// Initialize <c>_baseRaidClears</c> with current raid progression exposed by GW2 API
        /// </summary>
        /// <returns></returns>
        public async Task InitBaseRaidClears()
        {
            _baseRaidClears = await GetApiRaidClears();
        }

        /// <summary>
        /// Compares the base raid clear from <c>_baseRaidClears</c> with the current clear
        /// </summary>
        /// <returns><see langword="true"/> if a new boss has been killed, <see langword="false"/> otherwise.</returns>
        public async Task<bool> CheckRaidClears()
        {
            bool hasNewClear = false;
            var clears = await GetApiRaidClears();

            //Detects if we have a new final boss clear
            foreach (var bossName in _raidBossNames)
            {
                if (!_baseRaidClears.Contains(bossName) && clears.Contains(bossName))
                {
                    hasNewClear = true;
                    break;
                }
            }

            return hasNewClear;
        }

        /// <summary>
        /// Refresh Killproof.me data
        /// </summary>
        /// <returns></returns>
        public async Task RefreshKillproofMe(bool fromUpdateLoop = false)
        {
            CancelSchedule();

            if (!_refreshAvailable.HasValue)
                ScreenNotification.ShowNotification("[KpRefresher] Please check your Id setting !", ScreenNotification.NotificationType.Error);

            //Prevents spamming KP.me api
            if (DateTime.UtcNow < _refreshAvailable.Value)
            {
                //Rounding up is a safety mesure to prevent early refresh
                var minutesUntilRefreshAvailable = Math.Ceiling((_refreshAvailable.Value - DateTime.UtcNow).TotalMinutes);
                ScheduleRefresh(minutesUntilRefreshAvailable + 1);

                if (!fromUpdateLoop || _moduleSettings.ShowScheduleNotification.Value)
                    ScreenNotification.ShowNotification($"[KpRefresher] Next refresh available in {minutesUntilRefreshAvailable} minutes\nA new try has been scheduled.", ScreenNotification.NotificationType.Warning);

                return;
            }

            var refreshed = await KpMeRefresh();
            if (refreshed.HasValue && refreshed.Value)
            {
                //Replace clears stored with updated clears and disable auto-retry
                _lastRefresh = DateTime.UtcNow;
                _baseRaidClears = await GetApiRaidClears();

                ScreenNotification.ShowNotification("[KpRefresher] Killproof.me refresh successful !", ScreenNotification.NotificationType.Info);
            }
            else if (refreshed.HasValue && !refreshed.Value)
            {
                //Although we checked refresh date, we couldn't update, retry later
                await UpdateLastRefresh(); //Necessary ?

                ScheduleRefresh();

                if (_moduleSettings.ShowScheduleNotification.Value)
                    ScreenNotification.ShowNotification("[KpRefresher] Killproof.me refresh was not available\nAuto-retry in 5 minutes.", ScreenNotification.NotificationType.Warning);
            }
        }

        /// <summary>
        /// Compares the base raid clear from <c>_baseRaidClears</c> with the current clear
        /// </summary>
        /// <returns>A list of the new kills formatted in a string</returns>
        public async Task<string> GetDelta()
        {
            var clears = await GetApiRaidClears();

            if (clears == null || _baseRaidClears == null)
                return string.Empty;

            var result = clears.Where(p => !_baseRaidClears.Any(p2 => p2 == p));

            string msgToDisplay = !result.Any() ? "No new kill." : $"New kills :\n\n{string.Join("\n", result)}";

            return msgToDisplay;
        }

        /// <summary>
        /// Disable any scheduled refresh
        /// </summary>
        public void CancelSchedule()
        {
            RefreshScheduled = false;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = double.MaxValue;
        }

        public async Task UpdateLastRefresh(DateTime? date = null)
        {
            if (date == null)
            {
                var accountData = await GetAccountData();
                date = accountData?.LastRefresh;
            }

            _lastRefresh = date.GetValueOrDefault();
        }

        public async Task<KpApiModel> GetAccountData()
        {
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
                return null;

            var url = $"{_kpMeBaseUrl}api/kp/{_moduleSettings.KpMeId.Value}?lang=en";
            try
            {
                using var client = new HttpClient();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<KpApiModel>(content);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification($"[KpRefresher] Killproof.me Id {_moduleSettings.KpMeId.Value} does not exist !", ScreenNotification.NotificationType.Error);
                    else
                        _logger.Error($"Unknown status while getting account data : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while getting account info : {ex.Message}");
            }

            return null;
        }

        public double GetNextRetryTimer()
        {
            return !RefreshScheduled ? 0 : ScheduleTimerEndValue - ScheduleTimer;
        }

        public async Task<string> DisplayCurrentKp()
        {
            return string.Empty;
            
            //Scan with GW2 API all KP in inventory & display them
            // -> Need to add Inventory auth
            // -> Need to fetch kp internal IDs
        }

        /// <summary>
        /// Unused, developed by mistake
        /// </summary>
        /// <returns></returns>
        private async Task<string> DisplayCurrentKpTokens()
        {
            //WARNING : 
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
                return "No Kp Id set.";

            var accountData = await GetAccountData();
            if (accountData == null)
                return "Unknown error";

            string msgToDisplay = !accountData.Killproofs.Any() ? "No new kp." : "New kp :\n\n";
            foreach (var kp in accountData.Killproofs)
            {
                msgToDisplay = $"{msgToDisplay}{kp.Amount} {kp.Name}\n";
            }
            return msgToDisplay;
        }

        private async Task<bool?> KpMeRefresh()
        {
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
            {
                ScreenNotification.ShowNotification("[KpRefresher] Killproof.me Id not set !", ScreenNotification.NotificationType.Error);
                return null;
            }

            var url = $"{_kpMeBaseUrl}proof/{_moduleSettings.KpMeId.Value}/refresh";

            try
            {
                using var client = new HttpClient();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return true;
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                        return false;
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification($"[KpRefresher] Killproof.me Id {_moduleSettings.KpMeId.Value} does not exist !", ScreenNotification.NotificationType.Error);
                    else
                        _logger.Error($"Unknown status while refreshing kp.me : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while refreshing kp.me : {ex.Message}");
            }

            return null;
        }

        private async Task<List<string>> GetApiRaidClears()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return null;
            }

            try
            {
                var data = await _gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();
                return data?.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while getting raid clears : {ex.Message}");
                return null;
            }
        }

        private void ScheduleRefresh(DateTime target)
        {
            var waitingTime = target < DateTime.UtcNow ? 0 : (target - DateTime.UtcNow).TotalMinutes;
            ScheduleRefresh(waitingTime);
        }

        private void ScheduleRefresh(double minutes = 5)
        {
            RefreshScheduled = true;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = minutes * 60 * 1000;
        }
    }
}
