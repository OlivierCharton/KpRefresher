using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
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
        private Gw2ApiManager _gw2ApiManager { get; set; }


        private TextBox _textBox { get; set; }
        private StandardButton _refreshRaidClears { get; set; }

        public KpRefresherWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion,
            AsyncTexture2D cornerIconTexture, ModuleSettings moduleSettings, Gw2ApiManager gw2ApiManager) : base(background, windowRegion, contentRegion)
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Kp Refresher";
            Emblem = cornerIconTexture;
            Location = new Point(300, 300);
            SavesPosition = true;

            _moduleSettings = moduleSettings;
            _gw2ApiManager = gw2ApiManager;

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
                Text = "Identifiant kp.me : ",
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
                Text = "Refresh raid clears",
                Size = new Point(110, 30),
                Location = new Point(0, 0),
                Parent = actionPannels
            };
            _refreshRaidClears.Click += RefreshRaidClears;
        }

        protected override void DisposeControl()
        {
            _textBox.EnterPressed -= SaveKpId;
            _refreshRaidClears.Click -= RefreshRaidClears;
        }

        private void SaveKpId(object s, EventArgs e)
        {
            var scopeTextBox = s as TextBox;
            var value = scopeTextBox.Text;

            _moduleSettings.KpMeId.Value = value;
        }

        private async void RefreshRaidClears(object sender, MouseEventArgs e)
        {
            var r = await _gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();
        }
    }
}
