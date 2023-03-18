using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
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
        private const string _kpMeBaseUrl = "https://killproof.me/proof/";

        private List<string> _baseRaidClears { get; set; }

        public bool RefreshTriggered { get; set; }
        public double TriggerTimer { get; set; }

        public RaidService(ModuleSettings moduleSettings, Gw2ApiManager gw2ApiManager, Logger logger)
        {
            _moduleSettings = moduleSettings;
            _gw2ApiManager = gw2ApiManager;
            _logger = logger;
        }

        public async Task InitBaseRaidClears()
        {
            _baseRaidClears = await GetRaidClears();
        }

        public async Task<bool> CheckRaidClears()
        {
            bool hasNewClear = false;
            var clears = await GetRaidClears();

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

        public async Task Refresh()
        {
            TriggerTimer = 0;

            var refreshed = await CallKpMeRefresh();
            if (refreshed.HasValue && refreshed.Value)
            {
                _baseRaidClears = await GetRaidClears();
                RefreshTriggered = false;

                ScreenNotification.ShowNotification("Killproof.me refresh successful !", ScreenNotification.NotificationType.Info);
            }
            else if (refreshed.HasValue && !refreshed.Value)
            {
                RefreshTriggered = true;
                ScreenNotification.ShowNotification("Killproof.me refresh was not available, retry in 5 minutes.", ScreenNotification.NotificationType.Warning);
            }
        }

        public async Task ShowDelta()
        {
            //TEST PURPOSE
            _baseRaidClears.Remove("sabetha");
            _baseRaidClears.Remove("gorseval");
            //TEST PURPOSE

            var clears = await GetRaidClears();
            var result = clears.Where(p => !_baseRaidClears.Any(p2 => p2 == p));

            string msgToDisplay;

            if (result.Any())
            {
                msgToDisplay = $"New kills : {string.Join(", ", result)}";
            }
            else
                msgToDisplay = "No new kill.";

            ScreenNotification.ShowNotification(msgToDisplay, ScreenNotification.NotificationType.Info);
        }

        public void StopRetry()
        {
            TriggerTimer = 0;
            RefreshTriggered = false;

            ScreenNotification.ShowNotification("Auto-retry disabled !", ScreenNotification.NotificationType.Info);
        }

        private async Task<bool?> CallKpMeRefresh()
        {
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
            {
                ScreenNotification.ShowNotification("Killproof.me Id not set !", ScreenNotification.NotificationType.Error);
                return null;
            }

            var url = $"{_kpMeBaseUrl}{_moduleSettings.KpMeId.Value}/refresh";

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


        private async Task<List<string>> GetRaidClears()
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
    }
}
