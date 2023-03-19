using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using KpRefresher.Services;
using Microsoft.Xna.Framework;
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
            Title = "Kp Refresher";
            Emblem = cornerIconTexture;
            Location = new Point(300, 300);
            SavesPosition = true;

            _moduleSettings = moduleSettings;
            _raidService = raidService;

            #region Config
            Panel configPannel = new()
            {
                Parent = this,
                Location = new Point(0, 0),
                WidthSizingMode = SizingMode.Fill,
                Height = 200,
                Title = "Configuration",
                ShowBorder = true
            };

            Label kpIdLabel = new()
            {
                Parent = configPannel,
                Location = new Point(15, 15),
                AutoSizeWidth = true,
                Height = 25,
                Text = "Killproof.me Id : ",
            };

            TextBox kpIdTextBox = new()
            {
                Parent = configPannel,
                Location = new Point(kpIdLabel.Right + 5, kpIdLabel.Top),
                Width = 75,
                Text = _moduleSettings.KpMeId.Value,
            };
            kpIdTextBox.EnterPressed += (s, e) =>
            {
                _moduleSettings.KpMeId.Value = kpIdTextBox.Text;
            };

            Label showAutoRetryNotificationLabel = new()
            {
                Parent = configPannel,
                Location = new Point(15, 50),
                AutoSizeWidth = true,
                Height = 25,
                Text = "Show auto-retry notification : ",
                BasicTooltipText = "Display a notification when killproof.me was not available for a refresh",
            };

            Checkbox showAutoRetryNotificationCheckbox = new()
            {
                Parent = configPannel,
                Location = new Point(showAutoRetryNotificationLabel.Right + 5, showAutoRetryNotificationLabel.Top + 4),
                Checked = _moduleSettings.ShowAutoRetryNotification.Value
            };
            showAutoRetryNotificationCheckbox.CheckedChanged += (s, e) =>
            {
                _moduleSettings.ShowAutoRetryNotification.Value = showAutoRetryNotificationCheckbox.Checked;
            };
            #endregion Config

            #region Actions
            Panel actionPannels = new()
            {
                Parent = this,
                Location = new Point(0, configPannel.Bottom + 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = "Actions",
                ShowBorder = true
            };

            StandardButton refreshRaidClears = new()
            {
                Parent = actionPannels,
                Location = new Point(15, 15),
                Size = new Point(150, 30),
                Text = "Refresh killproof.me",
                BasicTooltipText = "Attempts to refresh killproof.me; if it fails, will try again 5 minutes later",
            };
            refreshRaidClears.Click += async (s, e) =>
            {
                await RefreshRaidClears();
            };

            _loadingSpinner = new LoadingSpinner()
            {
                Parent = actionPannels,
                Location = new Point(refreshRaidClears.Right + 2, 17),
                Size = new Point(29, 29),
                Visible = false,
            };

            StandardButton displayRaidDifference = new()
            {
                Parent = actionPannels,
                Location = new Point(refreshRaidClears.Right + 100, 15),
                Size = new Point(150, 30),
                Text = "Show new clears",
                BasicTooltipText = "Displays new kills made since KpRefresher start or last successful killproof.me refresh",
            };
            displayRaidDifference.Click += async (s, e) =>
            {
                await DisplayRaidDifference();
            };

            StandardButton stopRetry = new()
            {
                Parent = actionPannels,
                Location = new Point(displayRaidDifference.Right + 100, 15),
                Size = new Point(150, 30),
                Text = "Stop retry",
                BasicTooltipText = "Resets any pending refresh",
            };
            stopRetry.Click += (s, e) =>
            {
                StopRetry();
            };
            #endregion Actions

            #region Notifications
            Panel notificationsPannel = new()
            {
                Parent = this,
                Location = new Point(0, this.Height - 200),
                Width = this.Width,
                Height = 100,
            };

            _notificationLabel = new Label()
            {
                Parent = notificationsPannel,
                Location = new Point(0, 0),
                Width = notificationsPannel.Width,
                Height = notificationsPannel.Height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font = GameService.Content.DefaultFont32,
                Text = string.Empty
            };
            #endregion Notifications
        }

        private async Task RefreshRaidClears()
        {
            _loadingSpinner.Visible = true;

            await _raidService.RefreshKillproofMe();

            //Keeps the spinner visible in refresh in auto-retry
            _loadingSpinner.Visible = _raidService.RefreshTriggered;
        }

        private async Task DisplayRaidDifference()
        {
            var data = await _raidService.GetDelta();
            ShowInsideNotification(data);
        }

        private void StopRetry()
        {
            _raidService.StopRetry();

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
