using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpRefresher.Services
{
    public class BusinessService
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly Gw2ApiService _gw2ApiService;
        private readonly KpMeService _kpMeService;

        private static readonly List<string> _raidBossNames = new() { "sabetha", "matthias", "xera", "deimos", "voice_in_the_void", "qadim", "qadim_the_peerless" };

        private DateTime? _lastRefresh { get; set; }
        private DateTime? _refreshAvailable => _lastRefresh?.AddMinutes(61);

        public bool RefreshScheduled { get; set; }
        public double ScheduleTimer { get; set; }
        public double ScheduleTimerEndValue { get; set; }

        public bool NotificationNextRefreshAvailabledActivated { get; set; }
        public double NotificationNextRefreshAvailabledTimer { get; set; }
        public double NotificationNextRefreshAvailabledTimerEndValue { get; set; }

        public BusinessService(ModuleSettings moduleSettings, Gw2ApiService gw2ApiService, KpMeService kpMeService)
        {
            _moduleSettings = moduleSettings;
            _gw2ApiService = gw2ApiService;
            _kpMeService = kpMeService;
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns><see langword="true"/> if a new boss has been killed, <see langword="false"/> otherwise.</returns>
        public async Task<bool> CheckRaidClears()
        {
            bool hasNewClear = false;
            var clears = await _gw2ApiService.GetRaidClears();

            //Detects if we have a new final boss clear
            foreach (var bossName in _raidBossNames)
            {
                if (!_gw2ApiService.BaseRaidClears.Contains(bossName) && clears.Contains(bossName))
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
                ScheduleRefresh(minutesUntilRefreshAvailable);

                if (!fromUpdateLoop || _moduleSettings.ShowScheduleNotification.Value)
                    ScreenNotification.ShowNotification($"[KpRefresher] Next refresh available in {minutesUntilRefreshAvailable} minutes\nA new try has been scheduled.", ScreenNotification.NotificationType.Warning);

                return;
            }

            var refreshed = await _kpMeService.RefreshApi();
            if (refreshed.HasValue && refreshed.Value)
            {
                //Replace clears stored with updated clears and disable auto-retry
                _lastRefresh = DateTime.UtcNow;
                await _gw2ApiService.RefreshBaseRaidClears();

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

        public async Task RefreshBaseData()
        {
            await _gw2ApiService.RefreshBaseRaidClears();

            var accountName = await _gw2ApiService.GetAccountName();
            _kpMeService.SetAccountName(accountName);

            await UpdateLastRefresh();
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns>A list of the new kills formatted in a string</returns>
        public async Task<string> GetDelta()
        {
            var clears = await _gw2ApiService.GetRaidClears();

            if (clears == null || _gw2ApiService.BaseRaidClears == null)
                return string.Empty;

            var result = clears.Where(p => !_gw2ApiService.BaseRaidClears.Any(p2 => p2 == p));

            string msgToDisplay = !result.Any() ? "No new kill." : $"New kills :\n\n{string.Join("\n", result)}";

            return msgToDisplay;
        }

        public async Task UpdateLastRefresh(DateTime? date = null)
        {
            if (date == null)
            {
                var accountData = await _kpMeService.GetAccountData();
                date = accountData?.LastRefresh;
            }

            _lastRefresh = date.GetValueOrDefault();
        }

        public async Task<string> DisplayCurrentKp()
        {
            var accountKp = await _gw2ApiService.ScanAccountForKp();

            return accountKp;
        }

        #region Schedule
        /// <summary>
        /// Disable any scheduled refresh
        /// </summary>
        public void CancelSchedule()
        {
            RefreshScheduled = false;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = double.MaxValue;
        }
        public double GetNextScheduledTimer()
        {
            return !RefreshScheduled ? 0 : ScheduleTimerEndValue - ScheduleTimer;
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
        #endregion Schedule

        public void CopyKpToClipboard()
        {
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
                ScreenNotification.ShowNotification("[KpRefresher] No Kp Id set.", ScreenNotification.NotificationType.Info);
            else
            {
                Clipboard.SetText($"KpMe id : {_moduleSettings.KpMeId.Value}");
                ScreenNotification.ShowNotification("[KpRefresher] Id copied to clipboard !", ScreenNotification.NotificationType.Info);
            }
        }

        #region Notification next refresh available
        public void ActivateNotificationNextRefreshAvailable()
        {
            if (DateTime.UtcNow > _refreshAvailable.Value)
            {
                ScreenNotification.ShowNotification($"[KpRefresher] Next refresh is available !", ScreenNotification.NotificationType.Info);
                return;
            }

            //Rounding up is a safety mesure to prevent early refresh
            var minutesUntilRefreshAvailable = Math.Ceiling((_refreshAvailable.Value - DateTime.UtcNow).TotalMinutes);

            NotificationNextRefreshAvailabledActivated = true;
            NotificationNextRefreshAvailabledTimer = 0;
            NotificationNextRefreshAvailabledTimerEndValue = minutesUntilRefreshAvailable * 60 * 1000;

            ScreenNotification.ShowNotification($"[KpRefresher] You will be notified when next refresh is available,\nin approx. {minutesUntilRefreshAvailable -1} minutes.", ScreenNotification.NotificationType.Info);
        }

        public void NextRefreshIsAvailable()
        {
            ScreenNotification.ShowNotification($"[KpRefresher] Next refresh is available !", ScreenNotification.NotificationType.Info);

            ResetNotificationNextRefreshAvailable();
        }

        public void ResetNotificationNextRefreshAvailable()
        {
            NotificationNextRefreshAvailabledActivated = false;
            NotificationNextRefreshAvailabledTimer = 0;
            NotificationNextRefreshAvailabledTimerEndValue = double.MaxValue;
        }
        #endregion Notification next refresh available

        /// <summary>
        /// Unused, developed by mistake
        /// </summary>
        /// <returns></returns>
        private async Task<string> DisplayCurrentKpTokens()
        {
            //WARNING : 
            if (string.IsNullOrWhiteSpace(_moduleSettings.KpMeId.Value))
                return "No Kp Id set.";

            var accountData = await _kpMeService.GetAccountData();
            if (accountData == null)
                return "Unknown error";

            string msgToDisplay = !accountData.Killproofs.Any() ? "No new kp." : "New kp :\n\n";
            foreach (var kp in accountData.Killproofs)
            {
                msgToDisplay = $"{msgToDisplay}{kp.Amount} {kp.Name}\n";
            }
            return msgToDisplay;
        }
    }
}
