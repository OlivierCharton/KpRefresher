using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
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


        private TextBox _kpIdTextBox { get; set; }
        private StandardButton _refreshRaidClears { get; set; }
        private LoadingSpinner _loadingSpinner { get; set; }
        private StandardButton _displayRaidDifference { get; set; }
        private StandardButton _stopRetry { get; set; }
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
            var configPannel = new Panel()
            {
                Parent = this,
                Location = new Point(0, 0),
                WidthSizingMode = SizingMode.Fill,
                Height = 200,
                Title = "Configuration",
                ShowBorder = true
            };

            var kpIdLabel = new Label()
            {
                Parent = configPannel,
                Location = new Point(15, 15),
                AutoSizeWidth = true,
                Height = 25,
                Text = "Killproof.me Id : "
            };

            _kpIdTextBox = new TextBox()
            {
                Parent = configPannel,
                Location = new Point(kpIdLabel.Right + 5, kpIdLabel.Top),
                Text = _moduleSettings.KpMeId.Value
            };

            _kpIdTextBox.EnterPressed += SaveKpId;
            #endregion Config

            #region Actions
            var actionPannels = new Panel()
            {
                Parent = this,
                Location = new Point(0, configPannel.Bottom + 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = "Actions",
                ShowBorder = true
            };

            _refreshRaidClears = new StandardButton()
            {
                Parent = actionPannels,
                Location = new Point(0, 0),
                Size = new Point(150, 30),
                Text = "Refresh killproof.me",
                BasicTooltipText = "Attempts to refresh killproof.me; if it fails, will try again 5 minutes later",
            };
            _refreshRaidClears.Click += RefreshRaidClears;

            _loadingSpinner = new LoadingSpinner()
            {
                Parent = actionPannels,
                Location = new Point(_refreshRaidClears.Right + 2, 2),
                Size = new Point(29, 29),
                Visible = false,
            };

            _displayRaidDifference = new StandardButton()
            {
                Parent = actionPannels,
                Location = new Point(_refreshRaidClears.Right + 30, 0),
                Size = new Point(150, 30),
                Text = "Show new clears",
                BasicTooltipText = "Displays new kills made since KpRefresher start or last successful killproof.me refresh",
            };
            _displayRaidDifference.Click += DisplayRaidDifference;

            _stopRetry = new StandardButton()
            {
                Parent = actionPannels,
                Location = new Point(_displayRaidDifference.Right + 30, 0),
                Size = new Point(150, 30),
                Text = "Stop retry",
                BasicTooltipText = "Resets any pending refresh",
            };
            _stopRetry.Click += StopRetry;
            #endregion Actions

            #region Notifications
            var notificationsPannel = new Panel()
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

        protected override void DisposeControl()
        {
            _kpIdTextBox.EnterPressed -= SaveKpId;
            _refreshRaidClears.Click -= RefreshRaidClears;
            _displayRaidDifference.Click -= DisplayRaidDifference;
            _stopRetry.Click -= StopRetry;
        }

        private void SaveKpId(object s, EventArgs e)
        {
            var scopeTextBox = s as TextBox;
            var value = scopeTextBox.Text;

            _moduleSettings.KpMeId.Value = value;
        }

        private async void RefreshRaidClears(object sender, MouseEventArgs e)
        {
            _loadingSpinner.Visible = true;

            await _raidService.RefreshKillproofMe();

            //Keeps the spinner visible in refresh in auto-retry
            _loadingSpinner.Visible = _raidService.RefreshTriggered;
        }

        private async void DisplayRaidDifference(object sender, MouseEventArgs e)
        {
            var data = await _raidService.GetDelta();
            ShowInsideNotification(data);
        }

        private void StopRetry(object sender, MouseEventArgs e)
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
