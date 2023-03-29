using Blish_HUD;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using KpRefresher.Domain.Attributes;
using KpRefresher.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Func<LoadingSpinner> _getSpinner;

        private string _accountName { get; set; }
        private string _kpId { get; set; }

        private bool _isRefreshingKpData { get; set; }

        private List<RaidBoss> _raidBossNames { get; set; }

        private DateTime? _lastRefresh { get; set; }
        private DateTime? _refreshAvailable => _lastRefresh?.AddMinutes(61);

        private List<int> _raidMapIds { get; set; }
        private List<int> _strikeMapIds { get; set; }
        private bool _playerWasInInstance { get; set; }

        public List<string> LinkedKpId { get; set; }

        public bool RefreshScheduled { get; set; }
        public double ScheduleTimer { get; set; }
        public double ScheduleTimerEndValue { get; set; }

        public bool NotificationNextRefreshAvailabledActivated { get; set; }
        public double NotificationNextRefreshAvailabledTimer { get; set; }
        public double NotificationNextRefreshAvailabledTimerEndValue { get; set; }

        public BusinessService(ModuleSettings moduleSettings, Gw2ApiService gw2ApiService, KpMeService kpMeService, Func<LoadingSpinner> getSpinner)
        {
            _moduleSettings = moduleSettings;
            _gw2ApiService = gw2ApiService;
            _kpMeService = kpMeService;
            _getSpinner = getSpinner;

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

        public async Task RefreshBaseData()
        {
            _getSpinner?.Invoke()?.Show();

            //Get accountName to refresh kp.me id
            _accountName = await _gw2ApiService.GetAccountName();

            await RefreshKpMeData();

            _getSpinner?.Invoke()?.Hide();
        }

        /// <summary>
        /// Refresh KillProof.me data
        /// </summary>
        /// <returns></returns>
        public async Task RefreshKillproofMe(bool fromUpdateLoop = false)
        {
            CancelSchedule();

            if (!await DataLoaded())
            {
                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
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
                    ScreenNotification.ShowNotification("[KpRefresher] No new clear validating settings, refresh aborted !", ScreenNotification.NotificationType.Info);
                    return;
                }
            }

            var refreshed = await _kpMeService.RefreshApi(_kpId);
            if (refreshed.HasValue && refreshed.Value)
            {
                //Replace clears stored with updated clears and disable auto-retry
                _lastRefresh = DateTime.UtcNow;

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

        /// <summary>
        /// Disable any scheduled refresh
        /// </summary>
        public void CancelSchedule()
        {
            RefreshScheduled = false;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = double.MaxValue;
        }

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

        public async Task CopyKpToClipboard()
        {
            if (await DataLoaded())
            {
                Clipboard.SetText($"KpMe id : {_kpId}");
                ScreenNotification.ShowNotification("[KpRefresher] Id copied to clipboard !", ScreenNotification.NotificationType.Info);
            }
            else
            {
                ScreenNotification.ShowNotification("[KpRefresher] Id could not be loaded\nPlease try again later", ScreenNotification.NotificationType.Warning);
            }
        }

        #region Notification next refresh available
        public async Task ActivateNotificationNextRefreshAvailable()
        {
            if (!await DataLoaded())
            {
                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
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

        public void ResetNotificationNextRefreshAvailable()
        {
            NotificationNextRefreshAvailabledActivated = false;
            NotificationNextRefreshAvailabledTimer = 0;
            NotificationNextRefreshAvailabledTimerEndValue = double.MaxValue;
        }

        public void NextRefreshIsAvailable()
        {
            ScreenNotification.ShowNotification($"[KpRefresher] Next refresh is available !", ScreenNotification.NotificationType.Info);

            ResetNotificationNextRefreshAvailable();
        }
        #endregion Notification next refresh available

        #region UI Methods
        public TimeSpan GetNextScheduledTimer()
        {
            if (!RefreshScheduled)
                return TimeSpan.Zero;

            var seconds = (ScheduleTimerEndValue - ScheduleTimer) / 1000;
            return new TimeSpan(0, 0, (int)seconds);
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns>A list of the new kills formatted in a string</returns>
        public async Task<string> GetDelta()
        {
            if (!await DataLoaded())
            {
                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
                return string.Empty;
            }

            var baseClears = await _kpMeService.GetClearData(_kpId);
            var clears = await _gw2ApiService.GetClears();

            if (clears == null || baseClears == null)
                return string.Empty;

            var formattedGw2ApiClears = new List<RaidBoss>();
            foreach (var clear in clears)
            {
                Enum.TryParse(clear, out RaidBoss boss);
                formattedGw2ApiClears.Add(boss);
            }

            var result = formattedGw2ApiClears.Where(p => !baseClears.Any(p2 => p2 == p.GetDisplayName()));

            string msgToDisplay;
            if (!result.Any())
            {
                msgToDisplay = "No new clear.";
            }
            else
            {
                msgToDisplay = "New clears :\n";

                var orderedBoss = result.OrderBy(x => (int)x).ToList();

                foreach (var wingNumber in orderedBoss.Select(ob => ob.GetAttribute<WingAttribute>().WingNumber).Distinct())
                {
                    var bossFromWing = orderedBoss.Where(o => o.GetAttribute<WingAttribute>().WingNumber == wingNumber).Select(o => o.GetDisplayName());
                    msgToDisplay = $"{msgToDisplay}\n[Wing {wingNumber}]\n{string.Join("\n", bossFromWing)}\n";
                }
            }

            return msgToDisplay;
        }

        public async Task<string> DisplayCurrentKp()
        {
            if (!await DataLoaded())
            {
                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
                return string.Empty;
            }

            var accountKp = await _gw2ApiService.ScanAccountForKp();

            return accountKp;
        }

        public async Task<string> RefreshLinkedAccounts()
        {
            if (!await DataLoaded())
            {
                ScreenNotification.ShowNotification("[KpRefresher] KillProof.me refresh was not available\nPlease retry later.", ScreenNotification.NotificationType.Warning);
                return string.Empty;
            }

            var tasks = new List<Task>();

            var res = string.Empty;
            foreach (var acc in LinkedKpId)
            {
                Task tt = Task.Run(async () =>
                {
                    var refreshRes = await _kpMeService.RefreshApi(acc);
                    res = $"{res}- {acc} : {(refreshRes == true ? "Refreshed" : refreshRes == false ? "Refresh not available" : "Error")}\n";
                });
                tasks.Add(tt);
            }

            await Task.WhenAll(tasks);

            return res;
        }
        #endregion UI Methods

        private async Task RefreshKpMeData()
        {
            _isRefreshingKpData = true;

            //Reset stored data
            _kpId = string.Empty;
            _lastRefresh = null;
            LinkedKpId = null;

            var accountData = await _kpMeService.GetAccountData(_accountName);
            if (accountData == null)
            {
                ScreenNotification.ShowNotification("[KpRefresher] Error while loading KillProof.me profile.\nPlease retry later.", ScreenNotification.NotificationType.Warning);
                return;
            }

            _kpId = accountData.Id;
            _lastRefresh = accountData.LastRefresh;
            LinkedKpId = accountData.LinkedAccounts?.Select(l => l.Id)?.ToList();

            _isRefreshingKpData = false;
        }

        private void ScheduleRefresh(double minutes = 5)
        {
            RefreshScheduled = true;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = minutes * 60 * 1000;
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns><see langword="true"/> if a new boss has been killed, <see langword="false"/> otherwise.</returns>
        private async Task<bool> CheckRaidClears()
        {
            var baseClears = await _kpMeService.GetClearData(_kpId);
            var clears = await _gw2ApiService.GetClears();

            //No data
            if (clears == null || baseClears == null)
                return false;

            var formattedGw2ApiClears = new List<string>();
            foreach (var clear in clears)
            {
                Enum.TryParse(clear, out RaidBoss boss);
                formattedGw2ApiClears.Add(boss.GetDisplayName());
            }

            //No new clear
            var result = formattedGw2ApiClears.Where(p => !baseClears.Any(p2 => p2 == p));
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

        private async Task UpdateLastRefresh(DateTime? date = null)
        {
            if (date == null)
            {
                var accountData = await _kpMeService.GetAccountData(_kpId);
                date = accountData?.LastRefresh;
            }

            _lastRefresh = date.GetValueOrDefault();
        }

        private async Task<bool> DataLoaded(int retryCount = 0)
        {
            if (!string.IsNullOrWhiteSpace(_kpId))
                return true;

            if (retryCount >= 5)
                return false;

            if (_isRefreshingKpData)
            {
                retryCount++;

                await Task.Delay(1000);
            }
            else
            {
                await RefreshKpMeData();
            }

            return await DataLoaded(retryCount);
        }
    }
}
