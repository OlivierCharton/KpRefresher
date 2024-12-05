﻿using Blish_HUD;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using KpRefresher.Domain.Attributes;
using KpRefresher.Extensions;
using KpRefresher.Ressources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Blish_HUD.Controls.ScreenNotification;
using Controls = KpRefresher.UI.Controls;

namespace KpRefresher.Services
{
    public class BusinessService
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly Gw2ApiService _gw2ApiService;
        private readonly KpMeService _kpMeService;
        private readonly Func<LoadingSpinner> _getSpinner;
        private readonly Controls.CornerIcon _cornerIcon;
        private readonly Logger _logger;

        private string _accountName { get; set; }
        private string _kpId { get; set; }

        private bool _isRefreshingKpData { get; set; }

        private List<RaidBoss> _raidBossNames { get; set; }

        private DateTime _nextRefresh { get; set; }

        private List<int> _raidMapIds { get; set; }
        private List<int> _strikeMapIds { get; set; }
        private bool _playerWasInInstance { get; set; }

        private TimeSpan _nextRefreshInterval => _nextRefresh - DateTime.UtcNow;
        private bool _canRefreshByDates => _nextRefreshInterval.Ticks < 0;
        private double _nextRefreshSeconds { get; set; }

        public List<string> LinkedKpId { get; set; }

        public bool RefreshScheduled { get; set; }
        public double ScheduleTimer { get; set; }
        public double ScheduleTimerEndValue { get; set; }

        public bool NotificationNextRefreshAvailabledActivated { get; set; }
        public double NotificationNextRefreshAvailabledTimer { get; set; }
        public double NotificationNextRefreshAvailabledTimerEndValue { get; set; }

        public string KpId => string.IsNullOrEmpty(_moduleSettings.CustomId.Value) ? _kpId : _moduleSettings.CustomId.Value;

        public BusinessService(ModuleSettings moduleSettings, Gw2ApiService gw2ApiService, KpMeService kpMeService, Func<LoadingSpinner> getSpinner, Controls.CornerIcon cornerIcon, Logger logger)
        {
            _moduleSettings = moduleSettings;
            _gw2ApiService = gw2ApiService;
            _kpMeService = kpMeService;
            _getSpinner = getSpinner;
            _cornerIcon = cornerIcon;
            _logger = logger;

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
            await RefreshAccountName();

            await RefreshKpMeData(true);

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
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return;
            }

            //Prevents spamming KP.me api
            if (!_canRefreshByDates)
            {
                var minutesUntilRefreshAvailable = _nextRefreshInterval.TotalMinutes;

                string baseMsg = string.Format(strings.Notification_NextRefreshAvailableIn, minutesUntilRefreshAvailable.ToString("0"), minutesUntilRefreshAvailable > 1 ? "s" : string.Empty);
                if (_moduleSettings.EnableAutoRetry.Value)
                {
                    ScheduleRefresh(Math.Ceiling(_nextRefreshInterval.TotalSeconds));

                    if (!fromUpdateLoop || _moduleSettings.ShowScheduleNotification.Value)
                        ShowNotification(string.Format(strings.Notification_TryScheduled, baseMsg), NotificationType.Warning);
                }
                else
                {
                    ShowNotification(baseMsg, NotificationType.Warning);
                }

                return;
            }

            if (_moduleSettings.EnableRefreshOnKill.Value)
            {
                var hasNewClear = await CheckRaidClears();
                if (!hasNewClear)
                {
                    ShowNotification(strings.Notification_NoClearRefreshAborted, NotificationType.Info);
                    return;
                }
            }

            var refreshed = await _kpMeService.RefreshApi(KpId);
            if (refreshed.HasValue && refreshed.Value)
            {
                //Replace clears stored with updated clears and disable auto-retry
                _nextRefresh = DateTime.UtcNow.AddHours(1);

                //Update next refresh date 20s later (and every 20s if refreshed not done)
                _ = Task.Run(async () =>
                {
                    var profileNotRefreshed = true;
                    while (profileNotRefreshed)
                    {
                        await Task.Delay(20000);
                        await UpdateNextRefresh();
                        profileNotRefreshed = _nextRefreshSeconds == 0;
                    }
                });

                ShowNotification(strings.Notification_RefreshOk, NotificationType.Info);
            }
            else if (refreshed.HasValue && !refreshed.Value)
            {
                //Although we checked refresh date, we couldn't update, retry later
                await UpdateNextRefresh(); //Necessary ?

                if (_moduleSettings.EnableAutoRetry.Value)
                {
                    ScheduleRefresh();

                    if (_moduleSettings.ShowScheduleNotification.Value)
                        ShowNotification(strings.Notification_RefreshNotAvailableRetry, NotificationType.Warning);
                }
                else
                {
                    ShowNotification(strings.Notification_RefreshNotAvailable, NotificationType.Warning);
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
            try
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

                    ScheduleRefresh(_moduleSettings.DelayBeforeRefreshOnMapChange.Value * 60);

                    ShowNotification(string.Format(strings.Notification_InstanceExitDetected, _moduleSettings.DelayBeforeRefreshOnMapChange.Value, _moduleSettings.DelayBeforeRefreshOnMapChange.Value > 1 ? "s" : string.Empty), NotificationType.Info);
                }
            }
            catch (Exception e)
            {
                //This may happen when user start gw2 while Blish is already running (eg: gw2 starting with blish, closed gw2 then restarted it)
                _logger.Warn($"Error while changing map : {e.Message}");
            }
        }

        public async Task CopyKpToClipboard()
        {
            if (await DataLoaded())
            {
                Clipboard.SetText($"KillProof.me id : {KpId}");
                ShowNotification(strings.Notification_CopiedToClipboard, NotificationType.Info);
            }
            else
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
            }
        }

        public async Task OpenKpUrl()
        {
            if (!await DataLoaded())
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return;
            }

            var url = $"{_kpMeService.GetBaseUrl()}proof/{KpId}";
            Process.Start(url);
        }

        #region Notification next refresh available
        public async Task<bool> ActivateNotificationNextRefreshAvailable()
        {
            if (!await DataLoaded())
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return false;
            }

            if (_canRefreshByDates)
            {
                ShowNotification(strings.Notification_RefreshAvailable, NotificationType.Info);
                return false;
            }

            var minutesUntilRefreshAvailable = _nextRefreshInterval.TotalMinutes;

            NotificationNextRefreshAvailabledActivated = true;
            NotificationNextRefreshAvailabledTimer = 0;
            NotificationNextRefreshAvailabledTimerEndValue = Math.Ceiling(_nextRefreshInterval.TotalSeconds) * 1000;

            ShowNotification(string.Format(strings.Notification_NotifyScheduled, minutesUntilRefreshAvailable.ToString("0"), minutesUntilRefreshAvailable > 1 ? "s" : string.Empty), NotificationType.Info);

            return true;
        }

        public void ResetNotificationNextRefreshAvailable()
        {
            NotificationNextRefreshAvailabledActivated = false;
            NotificationNextRefreshAvailabledTimer = 0;
            NotificationNextRefreshAvailabledTimerEndValue = double.MaxValue;
        }

        public void NextRefreshIsAvailable()
        {
            ShowNotification(strings.Notification_RefreshAvailable, NotificationType.Info);

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

        public async Task<List<(string, Color?)>> GetFullRaidStatus()
        {
            if (!await DataLoaded())
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return null;
            }

            bool isError = false;
            var baseClears = await _kpMeService.GetClearData(KpId);
            var clears = await _gw2ApiService.GetClears();

            var res = new List<(string, Color?)>();

            if (baseClears == null)
            {
                isError = true;
                res.Add(("Error while fetching Kp.Me API\n", Colors.Error));
            }

            if (clears == null)
            {
                isError = true;
                res.Add(("Error while fetching GW2 API\n", Colors.Error));
            }

            if (isError)
            {
                return res;
            }

            var encounters = _raidBossNames.OrderBy(x => (int)x).ToList();
            foreach (var wingNumber in encounters.Select(ob => ob.GetAttribute<WingAttribute>().WingNumber).Distinct())
            {
                res.Add(($"[{strings.BusinessService_Wing} {wingNumber}]\n", Color.White));

                var bossFromWing = encounters.Where(o => o.GetAttribute<WingAttribute>().WingNumber == wingNumber);

                for (var i = 0; i < bossFromWing.Count(); i++)
                {
                    var boss = bossFromWing.ElementAt(i);

                    Color bossColor = Colors.BaseColor;
                    if (baseClears.Contains(boss))
                        bossColor = Colors.KpRefreshedColor;
                    else if (clears.Contains(boss))
                        bossColor = Colors.OnlyGw2;

                    res.Add((boss.GetDisplayName(), bossColor));
                    res.Add((i < bossFromWing.Count() - 1 ? " - " : string.Empty, Colors.BaseColor));
                }

                res.Add(("\n", null));
            }

            return res;
        }

        public async Task<string> DisplayCurrentKp()
        {
            if (!await DataLoaded())
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return string.Empty;
            }

            var accountKp = await _gw2ApiService.ScanAccountForKp();

            return accountKp;
        }

        public async Task<string> RefreshLinkedAccounts()
        {
            if (!await DataLoaded())
            {
                ShowNotification(strings.Notification_DataNotAvailable, NotificationType.Warning);
                return string.Empty;
            }

            var tasks = new List<Task>();

            var res = string.Empty;
            foreach (var acc in LinkedKpId)
            {
                Task tt = Task.Run(async () =>
                {
                    var refreshRes = await _kpMeService.RefreshApi(acc);
                    res = $"{res}- {acc} : {(refreshRes == true ? strings.BusinessService_Refreshed : refreshRes == false ? strings.BusinessService_RefreshNotAvailable : strings.BusinessService_Error)}\n";
                });
                tasks.Add(tt);
            }

            await Task.WhenAll(tasks);

            return res;
        }

        public async Task<(bool, string)> SetCustomId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _moduleSettings.CustomId.Value = value;
                return (true, strings.BusinessService_CustomIdSet);
            }

            var accountData = await _kpMeService.GetAccountData(value, false);
            if (accountData == null)
            {
                return (false, string.Format(strings.BusinessService_CustomIdNoAccountFound, value));
            }
            else if (accountData.AccountName != _accountName)
            {
                return (false, string.Format(strings.BusinessService_CustomIdAccountNotMatching, value, _accountName));
            }
            else
            {
                _moduleSettings.CustomId.Value = value;
                return (true, strings.BusinessService_CustomIdSet);
            }
        }
        #endregion UI Methods

        private async Task<bool> RefreshAccountName()
        {
            _accountName = await _gw2ApiService.GetAccountName();

            _cornerIcon.UpdateWarningState(string.IsNullOrWhiteSpace(_accountName));

            return !string.IsNullOrWhiteSpace(_accountName);
        }

        private async Task RefreshKpMeData(bool isFromInit = false)
        {
            _isRefreshingKpData = true;

            if (string.IsNullOrWhiteSpace(_accountName))
            {
                if (!await RefreshAccountName())
                {
                    _isRefreshingKpData = false;
                    return;
                }
            }

            //Reset stored data
            _kpId = string.Empty;
            _nextRefresh = DateTime.UtcNow.AddHours(1);
            LinkedKpId = null;

            var accountName = string.IsNullOrEmpty(_moduleSettings.CustomId.Value) ? _accountName : _moduleSettings.CustomId.Value;
            var accountData = await _kpMeService.GetAccountData(accountName);
            if (accountData == null)
            {
                if (isFromInit)
                    ShowNotification(strings.Notification_KPProfileFetchError, NotificationType.Warning);

                _isRefreshingKpData = false;
                return;
            }

            if (!string.IsNullOrEmpty(_moduleSettings.CustomId.Value) && accountData.AccountName != _accountName)
            {
                _moduleSettings.CustomId.Value = string.Empty;
                ShowNotification(string.Format(strings.Notification_CustomIdAccountNotMatching, _moduleSettings.CustomId.Value, _accountName), NotificationType.Warning);
            }

            _kpId = accountData.Id;
            _nextRefresh = accountData.NextRefresh;
            _nextRefreshSeconds = accountData.NextRefreshSeconds;
            LinkedKpId = accountData.LinkedAccounts?.Select(l => l.Id)?.ToList();

            _isRefreshingKpData = false;
        }

        private void ScheduleRefresh(double seconds = 300)
        {
            RefreshScheduled = true;
            ScheduleTimer = 0;
            ScheduleTimerEndValue = seconds * 1000;
        }

        /// <summary>
        /// Compares the base raid clear from <c>Gw2ApiService.BaseRaidClears</c> with the current clear
        /// </summary>
        /// <returns><see langword="true"/> if a new boss has been killed, <see langword="false"/> otherwise.</returns>
        private async Task<bool> CheckRaidClears()
        {
            var baseClears = await _kpMeService.GetClearData(KpId);
            var clears = await _gw2ApiService.GetClears();

            //No data
            if (clears == null || baseClears == null)
                return false;

            //No new clear
            var result = clears.Where(p => !baseClears.Any(p2 => p2 == p));
            if (!result.Any())
                return false;

            //New clear and no check for final boss
            if (!_moduleSettings.RefreshOnKillOnlyBoss.Value)
                return true;

            //Detects if we have a new final boss clear
            foreach (var res in result)
            {
                if (!_raidBossNames.Any(r => r == res))
                {
                    //Boss unknown - what to do ? For now it's a joker
                    return true;
                }

                var raidBoss = _raidBossNames.FirstOrDefault(r => r == res);
                if (raidBoss.HasAttribute<FinalBossAttribute>())
                    return true;
            }

            return false;
        }

        private async Task UpdateNextRefresh()
        {
            var accountData = await _kpMeService.GetAccountData(KpId);
            _nextRefresh = accountData?.NextRefresh ?? DateTime.Now.AddHours(1);
            _nextRefreshSeconds = accountData?.NextRefreshSeconds ?? 3600;
        }

        private async Task<bool> DataLoaded(int retryCount = 0)
        {
            if (!string.IsNullOrWhiteSpace(_kpId))
                return true;

            if (retryCount >= 5)
                return false;

            if (_isRefreshingKpData)
            {
                await Task.Delay(1000);
            }
            else
            {
                if (retryCount > 0)
                    await Task.Delay(1000);

                await RefreshKpMeData();
            }

            retryCount++;

            return await DataLoaded(retryCount);
        }

        private void ShowNotification(string message, NotificationType notificationType)
        {
            if (!_moduleSettings.HideAllMessages.Value)
                ScreenNotification.ShowNotification(message, notificationType);
        }
    }
}
