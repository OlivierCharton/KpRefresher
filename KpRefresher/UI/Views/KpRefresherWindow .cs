using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using KpRefresher.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpRefresher.UI.Views
{
    public class KpRefresherWindow : StandardWindow
    {
        private ModuleSettings _moduleSettings { get; set; }
        private RaidService _raidService { get; set; }


        private TextBox _textBox { get; set; }
        private StandardButton _refreshRaidClears { get; set; }
        private StandardButton _displayRaidDifference { get; set; }
        private StandardButton _stopRetry { get; set; }

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

            var configPannel = new Panel()
            {
                ShowBorder = true,
                Title = "Configuration",
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(0, 0),
                Parent = this,
            };

            var label = new Label()
            {
                Text = "Killproof.me Id : ",
                Parent = configPannel,
                AutoSizeWidth = true
            };

            _textBox = new TextBox()
            {
                Parent = configPannel,
                Location = new Point(label.Right + 5, label.Top),
                Text = _moduleSettings.KpMeId.Value
            };

            _textBox.EnterPressed += SaveKpId;

            var actionPannels = new Panel()
            {
                ShowBorder = true,
                Title = "Actions",
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                //Location = new Point(0, configPannel.Bottom + 15),
                Location = new Point(0, 500),
                Parent = this,
            };

            _refreshRaidClears = new StandardButton()
            {
                Text = "Refresh killproof.me",
                BasicTooltipText = "Attempts to refresh killproof.me; if it fails, will try again 5 minutes later",
                Size = new Point(150, 30),
                Location = new Point(0, 0),
                Parent = actionPannels
            };
            _refreshRaidClears.Click += RefreshRaidClears;

            _displayRaidDifference = new StandardButton()
            {
                Text = "Show new clears",
                BasicTooltipText = "Displays in a notification new kills made since KpRefresher start or last successful kp.me refresh",
                Size = new Point(150, 30),
                Location = new Point(_refreshRaidClears.Right + 30, 0),
                Parent = actionPannels
            };
            _displayRaidDifference.Click += DisplayRaidDifference;

            _stopRetry = new StandardButton()
            {
                Text = "Stop retry",
                BasicTooltipText = "Resets any pending refresh",
                Size = new Point(150, 30),
                Location = new Point(_displayRaidDifference.Right + 30, 0),
                Parent = actionPannels
            };
            _stopRetry.Click += StopRetry;
        }

        protected override void DisposeControl()
        {
            _textBox.EnterPressed -= SaveKpId;
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
            await _raidService.Refresh();
        }

        private async void DisplayRaidDifference(object sender, MouseEventArgs e)
        {
            await _raidService.ShowDelta();
        }

        private void StopRetry(object sender, MouseEventArgs e)
        {
             _raidService.StopRetry();
        }
    }
}
