using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
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
        private TextBox _textBox { get; set; }

        public KpRefresherWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, AsyncTexture2D cornerIconTexture, ModuleSettings moduleSettings) : base(background, windowRegion, contentRegion)
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Kp Refresher";
            Emblem = cornerIconTexture;
            Location = new Point(300, 300);
            SavesPosition = true;
            Id = $"{nameof(KpRefresher)}_My_Unique_ID_123";

            _moduleSettings = moduleSettings;

            var pannel = new Panel()
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
                Parent = pannel,
                AutoSizeWidth = true
            };

            _textBox = new TextBox()
            {
                Parent = pannel,
                Location = new Point(label.Right + 5, label.Top),
                Text = _moduleSettings.KpMeId.Value
            };

            _textBox.EnterPressed += SaveKpId;
        }

        protected override void DisposeControl()
        {
            _textBox.EnterPressed -= SaveKpId;
        }

        private void SaveKpId(object s, EventArgs e)
        {
            var scopeTextBox = s as TextBox;
            var value = scopeTextBox.Text;

            _moduleSettings.KpMeId.Value = value;
        }
    }
}
