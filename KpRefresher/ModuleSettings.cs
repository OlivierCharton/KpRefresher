using Blish_HUD.Settings;

namespace KpRefresher
{
    public class ModuleSettings
    {
        public SettingEntry<string> KpMeId { get; set; }


        public ModuleSettings(SettingCollection settings)
        {
            KpMeId = settings.DefineSetting(nameof(KpMeId), string.Empty, () => "Killproof.me Id", () => "The id of your Killproof.me");
        }
    }
}
