using Blish_HUD;
using Gw2Sharp.WebApi;
using KpRefresher.Interfaces;
using KpRefresher.Services;
using System;

namespace KpRefresher.UI.Controls
{
    public class FlowPanel : Blish_HUD.Controls.FlowPanel, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;
        private Func<string> _setLocalizedTitle;

        public FlowPanel()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
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

        public Func<string> SetLocalizedTitle
        {
            get => _setLocalizedTitle;
            set
            {
                _setLocalizedTitle = value;
                Title = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
            if (SetLocalizedTitle != null) Title = SetLocalizedTitle?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}