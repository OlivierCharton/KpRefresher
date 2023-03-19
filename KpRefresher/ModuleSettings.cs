using Blish_HUD.Settings;

namespace KpRefresher
{
    public class ModuleSettings
    {
        public SettingEntry<string> KpMeId { get; set; }

        public SettingEntry<bool> ShowAutoRetryNotification { get; set; }
        public SettingEntry<bool> AutoRefresh { get; set; }


        public ModuleSettings(SettingCollection settings)
        {
            SettingCollection internalSettings = settings.AddSubCollection("Internal");

            KpMeId = internalSettings.DefineSetting(nameof(KpMeId), string.Empty);
            ShowAutoRetryNotification = internalSettings.DefineSetting(nameof(ShowAutoRetryNotification), true);
            AutoRefresh = internalSettings.DefineSetting(nameof(AutoRefresh), false);
        }
    }
}
