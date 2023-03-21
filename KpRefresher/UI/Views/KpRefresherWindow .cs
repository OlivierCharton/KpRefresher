﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using KpRefresher.Services;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace KpRefresher.UI.Views
{
    public class KpRefresherWindow : StandardWindow
    {
        private ModuleSettings _moduleSettings { get; set; }
        private RaidService _raidService { get; set; }

        private LoadingSpinner _loadingSpinner { get; set; }
        private Label _notificationLabel { get; set; }

        public KpRefresherWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion,
            AsyncTexture2D cornerIconTexture, ModuleSettings moduleSettings, RaidService raidService) : base(background, windowRegion, contentRegion)
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = " Kp Refresher";
            Emblem = cornerIconTexture;
            Location = new Point(300, 300);
            SavesPosition = true;

            _moduleSettings = moduleSettings;
            _raidService = raidService;

            //Panel testPanel = new()
            //{
            //    Parent = this,
            //    BackgroundColor = Color.Red,
            //    WidthSizingMode = SizingMode.Fill,
            //    HeightSizingMode = SizingMode.Fill,
            //    ZIndex = 100
            //};
        }

        public void BuildUi()
        {
            FlowPanel mainContainer = new()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                Height = this.Height - 200, //leave some space for notification area
                ControlPadding = new(3, 3)
            };

            #region Config
            FlowPanel configContainer = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                Height = 200,
                Title = "Configuration",
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.TopToBottom
            };

            #region KpId
            var kpIdContainer = new FlowPanel()
            {
                Parent = configContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            Label kpIdLabel = new()
            {
                Parent = kpIdContainer,
                AutoSizeWidth = true,
                Height = 25,
                Text = "Killproof.me Id : ",
            };

            TextBox kpIdTextBox = new()
            {
                Parent = kpIdContainer,
                Width = 75,
                Text = _moduleSettings.KpMeId.Value,
            };
            kpIdTextBox.EnterPressed += async (s, e) =>
            {
                _moduleSettings.KpMeId.Value = kpIdTextBox.Text;
                await _raidService.UpdateLastRefresh();
            };
            #endregion KpId

            #region autoRetryNotification
            var autoRetryNotificationContainer = new FlowPanel()
            {
                Parent = configContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleLeftToRight
            };

            Label showAutoRetryNotificationLabel = new()
            {
                Parent = autoRetryNotificationContainer,
                AutoSizeWidth = true,
                Height = 25,
                Text = "Show auto-retry notifications : ",
                BasicTooltipText = "Display a notification when killproof.me was not available for a refresh",
            };

            Checkbox showAutoRetryNotificationCheckbox = new()
            {
                Parent = autoRetryNotificationContainer,
                Checked = _moduleSettings.ShowScheduleNotification.Value
            };
            showAutoRetryNotificationCheckbox.CheckedChanged += (s, e) =>
            {
                _moduleSettings.ShowScheduleNotification.Value = showAutoRetryNotificationCheckbox.Checked;
            };
            #endregion autoRetryNotification
            #endregion Config

            #region Actions
            FlowPanel actionPannels = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = "Actions",
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(3, 3),
                FlowDirection = ControlFlowDirection.SingleLeftToRight
            };

            #region RefreshRaidClears
            StandardButton refreshRaidClears = new()
            {
                Parent = actionPannels,
                Size = new Point(150, 30),
                Text = "Refresh killproof.me",
                BasicTooltipText = "Attempts to refresh killproof.me; if it fails, will try again 5 minutes later",
            };
            refreshRaidClears.Click += async (s, e) =>
            {
                await RefreshRaidClears();
            };
            #endregion RefreshRaidClears

            #region DisplayRaidDifference
            StandardButton displayRaidDifference = new()
            {
                Parent = actionPannels,
                Size = new Point(150, 30),
                Text = "Show new clears",
                BasicTooltipText = "Displays new kills made since KpRefresher start or last successful killproof.me refresh",
            };
            displayRaidDifference.Click += async (s, e) =>
            {
                await DisplayRaidDifference();
            };
            #endregion DisplayRaidDifference

            #region StopRetry
            StandardButton stopRetry = new()
            {
                Parent = actionPannels,
                Size = new Point(130, 30),
                Text = "Stop retry",
                BasicTooltipText = "Resets any pending refresh",
            };
            stopRetry.Click += (s, e) =>
            {
                StopRetry();
            };
            #endregion StopRetry

            #region Spinner
            _loadingSpinner = new LoadingSpinner()
            {
                Parent = actionPannels,
                Size = new Point(29, 29),
                Visible = false,
            };
            _loadingSpinner.MouseEntered += (s, e) =>
            {
                _loadingSpinner.BasicTooltipText = $"Next retry in {Math.Round(_raidService.GetNextRetryTimer() / 60 / 1000)} minutes.";
            };
            #endregion Spinner
            #endregion Actions

            #region Notifications
            Panel notificationsContainer = new()
            {
                Parent = this,
                Location = new Point(0, this.Height - 200),
                Width = this.Width,
                Height = 100,
            };

            _notificationLabel = new Label()
            {
                Parent = notificationsContainer,
                Width = notificationsContainer.Width,
                Height = notificationsContainer.Height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font = GameService.Content.DefaultFont32
            };
            #endregion Notifications
        }

        private async Task RefreshRaidClears()
        {
            //TODO: maybe disable Refresh btn if we can find a way to auto reactivate it from Service ?

            _loadingSpinner.Visible = true;

            await _raidService.RefreshKillproofMe();

            //Keeps the spinner visible in refresh in auto-retry
            _loadingSpinner.Visible = _raidService.RefreshScheduled;
        }

        private async Task DisplayRaidDifference()
        {
            var data = await _raidService.GetDelta();
            ShowInsideNotification(data);
        }

        private void StopRetry()
        {
            _raidService.CancelSchedule();

            _loadingSpinner.Visible = false;

            ShowInsideNotification("Auto-retry disabled !");
        }

        private void ShowInsideNotification(string message)
        {
            _notificationLabel.Text = message;
            _notificationLabel.Visible = true;

            Task.Run(async delegate
            {
                await Task.Delay(4000);

                _notificationLabel.Text = string.Empty;
                _notificationLabel.Visible = false;

                return;
            });
        }
    }
}
