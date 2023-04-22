using Blish_HUD;
using Gw2Sharp.WebApi;
using KpRefresher.Interfaces;
using KpRefresher.Services;
using System;

namespace KpRefresher.UI.Controls
{
    public class ContextMenuStripItem : Blish_HUD.Controls.ContextMenuStripItem, ILocalizable
    {
        private Func<string> _setLocalizedText;

        public ContextMenuStripItem()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Func<string> SetLocalizedText
        {
            get => _setLocalizedText;
            set
            {
                _setLocalizedText = value;
                Text = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedText != null) Text = SetLocalizedText?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}
