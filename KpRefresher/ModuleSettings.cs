﻿using Blish_HUD.Settings;

namespace KpRefresher
{
    public class ModuleSettings
    {
        public SettingEntry<bool> EnableAutoRetry { get; set; }
        public SettingEntry<bool> ShowScheduleNotification { get; set; }

        public SettingEntry<bool> EnableRefreshOnKill { get; set; }
        public SettingEntry<bool> RefreshOnKillOnlyBoss { get; set; }

        public SettingEntry<bool> RefreshOnMapChange { get; set; }
        public SettingEntry<int> DelayBeforeRefreshOnMapChange { get; set; }
        public SettingEntry<string> CustomId { get; set; }
        public SettingEntry<bool> HideAllMessages { get; set; }

        public ModuleSettings(SettingCollection settings)
        {
            SettingCollection internalSettings = settings.AddSubCollection("Internal");

            EnableAutoRetry = internalSettings.DefineSetting(nameof(EnableAutoRetry), true);
            ShowScheduleNotification = internalSettings.DefineSetting(nameof(ShowScheduleNotification), true);
            EnableRefreshOnKill = internalSettings.DefineSetting(nameof(EnableRefreshOnKill), false);
            RefreshOnKillOnlyBoss = internalSettings.DefineSetting(nameof(RefreshOnKillOnlyBoss), true);
            RefreshOnMapChange = internalSettings.DefineSetting(nameof(RefreshOnMapChange), false);
            DelayBeforeRefreshOnMapChange = internalSettings.DefineSetting(nameof(DelayBeforeRefreshOnMapChange), 10);
            CustomId = internalSettings.DefineSetting(nameof(CustomId), string.Empty);
            HideAllMessages = internalSettings.DefineSetting(nameof(HideAllMessages), false);
        }
    }
}
