using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using KpRefresher.Interfaces;
using KpRefresher.Ressources;
using KpRefresher.Services;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace KpRefresher.UI.Controls
{
    public class CornerIcon : Blish_HUD.Controls.CornerIcon, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;

        private Texture2D _cornerIconTexture;
        private Texture2D _cornerIconWarningTexture;
        private Texture2D _cornerIconHoverTexture;
        private Texture2D _cornerIconHoverWarningTexture;

        public CornerIcon(ContentsManager contentsManager)
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);

            _cornerIconTexture = contentsManager.GetTexture("corner.png");
            _cornerIconWarningTexture = contentsManager.GetTexture("corner_warn.png");
            _cornerIconHoverTexture = contentsManager.GetTexture("corner-hover.png");
            _cornerIconHoverWarningTexture = contentsManager.GetTexture("corner-hover_warn.png");

            UpdateWarningState(false);
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        public void UpdateWarningState(bool isWarning)
        {
            if (isWarning)
            {
                Icon = _cornerIconHoverWarningTexture;
                HoverIcon = _cornerIconHoverWarningTexture;
                SetLocalizedTooltip = () => strings.CornerIcon_Tooltip_Warning;
            }
            else
            {
                Icon = _cornerIconTexture;
                HoverIcon = _cornerIconHoverTexture;
                SetLocalizedTooltip = () => strings.CornerIcon_Tooltip;
            }
        }

        protected override void DisposeControl()
        {
            _cornerIconTexture?.Dispose();
            _cornerIconWarningTexture?.Dispose();
            _cornerIconHoverTexture?.Dispose();
            _cornerIconHoverWarningTexture?.Dispose();

            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}