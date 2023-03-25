using Blish_HUD.Settings;

namespace KpRefresher
{
    public class ModuleSettings
    {
        public SettingEntry<string> KpMeId { get; set; }

        public SettingEntry<bool> EnableAutoRetry { get; set; }
        public SettingEntry<bool> ShowScheduleNotification { get; set; }

        public SettingEntry<bool> EnableRefreshOnKill { get; set; }
        public SettingEntry<bool> RefreshOnKillOnlyBoss { get; set; }

        public SettingEntry<bool> RefreshOnMapChange { get; set; }
        public SettingEntry<int> DelayBeforeRefreshOnMapChange { get; set; }

        public ModuleSettings(SettingCollection settings)
        {
            SettingCollection internalSettings = settings.AddSubCollection("Internal");

            KpMeId = internalSettings.DefineSetting(nameof(KpMeId), string.Empty);
            EnableAutoRetry = internalSettings.DefineSetting(nameof(EnableAutoRetry), true);
            ShowScheduleNotification = internalSettings.DefineSetting(nameof(ShowScheduleNotification), true);
            EnableRefreshOnKill = internalSettings.DefineSetting(nameof(EnableRefreshOnKill), false);
            RefreshOnKillOnlyBoss = internalSettings.DefineSetting(nameof(RefreshOnKillOnlyBoss), true);
            RefreshOnMapChange = internalSettings.DefineSetting(nameof(RefreshOnMapChange), false);
            DelayBeforeRefreshOnMapChange = internalSettings.DefineSetting(nameof(DelayBeforeRefreshOnMapChange), 10);
        }
    }
}
