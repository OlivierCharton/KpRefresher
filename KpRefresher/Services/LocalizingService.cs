using Blish_HUD;
using Gw2Sharp.WebApi;
using System;

namespace KpRefresher.Services
{
    public static class LocalizingService
    {
        public static bool Enabled { get; set; } = true;

        public static event EventHandler<ValueChangedEventArgs<Locale>> LocaleChanged;

        public static void OnLocaleChanged(object sender, ValueChangedEventArgs<Locale> eventArgs)
        {
            TriggerLocaleChanged(Enabled, sender, eventArgs);
        }

        public static void TriggerLocaleChanged(bool force = false, object sender = null, ValueChangedEventArgs<Locale> eventArgs = null)
        {
            if (force) LocaleChanged?.Invoke(sender, eventArgs);
        }
    }
}