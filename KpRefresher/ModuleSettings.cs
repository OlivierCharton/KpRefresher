using Blish_HUD.Settings;

namespace KpRefresher
{
    public class ModuleSettings
    {
        public SettingEntry<string> KpMeId { get; set; }

        public SettingEntry<bool> ShowScheduleNotification { get; set; }
        public SettingEntry<bool> EnableAutoRetry { get; set; }


        public ModuleSettings(SettingCollection settings)
        {
            SettingCollection internalSettings = settings.AddSubCollection("Internal");

            KpMeId = internalSettings.DefineSetting(nameof(KpMeId), string.Empty);
            ShowScheduleNotification = internalSettings.DefineSetting(nameof(ShowScheduleNotification), true);
            EnableAutoRetry = internalSettings.DefineSetting(nameof(EnableAutoRetry), true);
        }
    }
}
