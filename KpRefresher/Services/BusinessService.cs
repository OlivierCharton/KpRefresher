using Blish_HUD;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using KpRefresher.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpRefresher.Services
{
    public class BusinessService
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly Gw2ApiService _gw2ApiService;
        private readonly KpMeService _kpMeService;

        private List<RaidBoss> _raidBossNames { get; set; }

        private DateTime? _lastRefresh { get; set; }
        private DateTime? _refreshAvailable => _lastRefresh?.AddMinutes(61);

        private List<int> _raidMapIds { get; set; }
        private List<int> _strikeMapIds { get; set; }
        private bool _playerWasInInstance { get; set; }

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

            _raidBossNames = Enum.GetValues(typeof(RaidBoss))
                            .Cast<RaidBoss>()
                            .ToList();

            _raidMapIds = Enum.GetValues(typeof(RaidMap))
                            .Cast<RaidMap>()
                            .Select(m => (int)m)
                            .ToList();

            _strikeMapIds = Enum.GetValues(typeof(StrikeMap))
                            .Cast<StrikeMap>()
                            .Select(m => (int)m)
                            .ToList();
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns><see langword="true"/> if a new boss has been killed, <see langword="false"/> otherwise.</returns>
        public async Task<bool> CheckRaidClears()
        {
            var clears = await _gw2ApiService.GetRaidClears();

            //No data
            if (clears == null || _gw2ApiService.BaseRaidClears == null)
                return false;

            //No new clear
            var result = clears.Where(p => !_gw2ApiService.BaseRaidClears.Any(p2 => p2 == p));
            if (!result.Any())
                return false;

            //New clear and no check for final boss
            if (!_moduleSettings.RefreshOnKillOnlyBoss.Value)
                return true;

            //Detects if we have a new final boss clear
            foreach (var res in result)
            {
                if (Enum.TryParse(res, out RaidBoss raidBoss))
                {
                    if (raidBoss.HasAttribute<FinalBossAttribute>())
                        return true;
                }
                else
                {
                    //Boss unknown - what to do ? For now it's a joker
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Refresh KillProof.me data
        /// </summary>
        /// <returns></returns>
        public async Task RefreshKillproofMe(bool fromUpdateLoop = false)
        {
            CancelSchedule();

            if (!_refreshAvailable.HasValue)
            { 
                ScreenNotification.ShowNotification("[KpRefresher] Data not yet loaded, please retry later !", ScreenNotification.NotificationType.Error);
                return;
            }

            //Prevents spamming KP.me api
            if (DateTime.UtcNow < _refreshAvailable.Value)
            {
                //Rounding up is a safety mesure to prevent early refresh
                var minutesUntilRefreshAvailable = Math.Ceiling((_refreshAvailable.Value - DateTime.UtcNow).TotalMinutes);

                string baseMsg = $"[KpRefresher] Next refresh available in {minutesUntilRefreshAvailable} minutes";
                if (_moduleSettings.EnableAutoRetry.Value)
                {
                    ScheduleRefresh(minutesUntilRefreshAvailable);

                    if (!fromUpdateLoop || _moduleSettings.ShowScheduleNotification.Value)
                        ScreenNotification.ShowNotification($"{baseMsg}\nA new try has been scheduled.", ScreenNotification.NotificationType.Warning);
                }
                else
                {
                    ScreenNotification.ShowNotification(baseMsg, ScreenNotification.NotificationType.Warning);
                }

                return;
            }

            if (_moduleSettings.EnableRefreshOnKill.Value)
            {
                var hasNewClear = await CheckRaidClears();
                if (!hasNewClear)
                {
                    ScreenNotification.ShowNotification("[KpRefresher] No new kill, refresh aborted !", ScreenNotification.NotificationType.Info);
                    return;
                }
            }

            var refreshed = await _kpMeService.RefreshApi();
            if (refreshed.HasValue && refreshed.Value)
            {
                //Replace clears stored with updated clears and disable auto-retry
                _lastRefresh = DateTime.UtcNow;
                await _gw2ApiService.RefreshBaseRaidClears();

                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh successful !", ScreenNotification.NotificationType.Info);
            }
            else if (refreshed.HasValue && !refreshed.Value)
            {
                //Although we checked refresh date, we couldn't update, retry later
                await UpdateLastRefresh(); //Necessary ?

                if (_moduleSettings.EnableAutoRetry.Value)
                {
                    ScheduleRefresh();

                    if (_moduleSettings.ShowScheduleNotification.Value)
                        ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nAuto-retry in 5 minutes.", ScreenNotification.NotificationType.Warning);
                }
                else
                {
                    ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
                }
            }
        }

        public async Task RefreshBaseData()
        {
            var accountName = await _gw2ApiService.GetAccountName();
            _kpMeService.SetAccountName(accountName);

            await _gw2ApiService.RefreshBaseRaidClears();

            var accountData = await _kpMeService.GetAccountData();
            _kpMeService.SetKpId(accountData?.Id);

            await UpdateLastRefresh(accountData?.LastRefresh);
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

            string msgToDisplay;
            if (!result.Any())
            {
                msgToDisplay = "No new kill.";
            }
            else
            {
                msgToDisplay = "New kills :\n\n";
                foreach (var res in result)
                {
                    if (Enum.TryParse(res, out RaidBoss raidBoss))
                        msgToDisplay = $"{msgToDisplay}{raidBoss.GetDisplayName()}\n";
                    else
                        msgToDisplay = $"{msgToDisplay}{res}\n";
                }
            }

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

        public TimeSpan GetNextScheduledTimer()
        {
            if (!RefreshScheduled)
                return TimeSpan.Zero;

            var seconds = (ScheduleTimerEndValue - ScheduleTimer) / 1000;
            return new TimeSpan(0, 0, (int)seconds);
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

        public async Task CopyKpToClipboard(int retryCount = 0)
        {
            //Loop to wait for id fetch if data not yet loaded
            if (string.IsNullOrWhiteSpace(_kpMeService.KpId) && retryCount < 5)
            {
                await Task.Delay(1000);

                retryCount++;
                await CopyKpToClipboard(retryCount);
            }
            else if (!string.IsNullOrWhiteSpace(_kpMeService.KpId))
            {
                Clipboard.SetText($"KpMe id : {_kpMeService.KpId}");
                ScreenNotification.ShowNotification("[KpRefresher] Id copied to clipboard !", ScreenNotification.NotificationType.Info);
            }
            else
            {
                ScreenNotification.ShowNotification("[KpRefresher] Id could not be loaded\nPlease try again later", ScreenNotification.NotificationType.Warning);
            }
        }

        #region Notification next refresh available
        public void ActivateNotificationNextRefreshAvailable()
        {
            if (!_refreshAvailable.HasValue)
            {
                ScreenNotification.ShowNotification("[KpRefresher] Data not yet loaded, please retry later !", ScreenNotification.NotificationType.Error);
                return;
            }

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

            ScreenNotification.ShowNotification($"[KpRefresher] You will be notified when next refresh is available,\nin approx. {minutesUntilRefreshAvailable - 1} minutes.", ScreenNotification.NotificationType.Info);
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

        #region Map Change
        public void MapChanged()
        {
            var mapId = GameService.Gw2Mumble.CurrentMap.Id;

            if (_raidMapIds.Contains(mapId) || _strikeMapIds.Contains(mapId))
            {
                //Activate the map change watcher
                _playerWasInInstance = true;
            }
            else if (_playerWasInInstance)
            {
                //Trigger refresh on instance exit
                _playerWasInInstance = false;

                ScheduleRefresh(_moduleSettings.DelayBeforeRefreshOnMapChange.Value);

                ScreenNotification.ShowNotification($"[KpRefresher] Instance exit detected, refresh scheduled in {_moduleSettings.DelayBeforeRefreshOnMapChange.Value} minute{(_moduleSettings.DelayBeforeRefreshOnMapChange.Value > 1 ? "s" : string.Empty)}", ScreenNotification.NotificationType.Info);
            }
        }
        #endregion Map Change

        /// <summary>
        /// Unused, developed by mistake
        /// </summary>
        /// <returns></returns>
        private async Task<string> DisplayCurrentKpTokens()
        {
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
