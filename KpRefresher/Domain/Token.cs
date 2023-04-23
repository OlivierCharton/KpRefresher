using KpRefresher.Ressources;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum Token
    {
        #region Raids
        [Display(Description = "Vale", ResourceType = typeof(tokens))]
        Vale = 77705,
        [Display(Description = "Gorseval", ResourceType = typeof(tokens))]
        Gorseval = 77751,
        [Display(Description = "Sabetha", ResourceType = typeof(tokens))]
        Sabetha = 77728,

        [Display(Description = "Slothasor", ResourceType = typeof(tokens))]
        Slothasor = 77706,
        [Display(Description = "Matthias", ResourceType = typeof(tokens))]
        Matthias = 77679,

        [Display(Description = "Escort", ResourceType = typeof(tokens))]
        Escort = 78873,
        [Display(Description = "KeepConstruct", ResourceType = typeof(tokens))]
        KeepConstruct = 78902,
        [Display(Description = "Xera", ResourceType = typeof(tokens))]
        Xera = 78942,

        [Display(Description = "Cairn", ResourceType = typeof(tokens))]
        Cairn = 80623,
        [Display(Description = "MursaatOverseer", ResourceType = typeof(tokens))]
        MursaatOverseer = 80269,
        [Display(Description = "Samarog", ResourceType = typeof(tokens))]
        Samarog = 80087,
        [Display(Description = "Deimos", ResourceType = typeof(tokens))]
        Deimos = 80542,

        [Display(Description = "Desmina", ResourceType = typeof(tokens))]
        Desmina = 85993,
        [Display(Description = "River", ResourceType = typeof(tokens))]
        River = 85785,
        [Display(Description = "Statue", ResourceType = typeof(tokens))]
        Statue = 85800,
        [Display(Description = "Dhuum", ResourceType = typeof(tokens))]
        Dhuum = 85633,

        [Display(Description = "ConjuredAmalgamate", ResourceType = typeof(tokens))]
        ConjuredAmalgamate = 88543,
        [Display(Description = "TwinLargos", ResourceType = typeof(tokens))]
        TwinLargos = 88860,
        [Display(Description = "Qadim", ResourceType = typeof(tokens))]
        Qadim = 88645,

        [Display(Description = "Adina", ResourceType = typeof(tokens))]
        Adina = 91246,
        [Display(Description = "Sabir", ResourceType = typeof(tokens))]
        Sabir = 91270,
        [Display(Description = "QTP", ResourceType = typeof(tokens))]
        QTP = 91175,
        #endregion Raids

        #region Strikes
        [Display(Description = "Boneskinner", ResourceType = typeof(tokens))]
        Boneskinner = 93781,

        [Display(Description = "AetherbladeHideout", ResourceType = typeof(tokens))]
        AetherbladeHideout = 95638,
        [Display(Description = "XunlaiJadeJunkyard", ResourceType = typeof(tokens))]
        XunlaiJadeJunkyard = 95982,
        [Display(Description = "KainengOverlook", ResourceType = typeof(tokens))]
        KainengOverlook = 97451,
        [Display(Description = "HarvestTemple", ResourceType = typeof(tokens))]
        HarvestTemple = 97132,
        [Display(Description = "OldLionsCourt", ResourceType = typeof(tokens))]
        OldLionsCourt = 99165,

        [Display(Description = "AetherbladeHideoutCM", ResourceType = typeof(tokens))]
        AetherbladeHideoutCM = 97269,
        [Display(Description = "XunlaiJadeJunkyardCM", ResourceType = typeof(tokens))]
        XunlaiJadeJunkyardCM = 96638,
        [Display(Description = "KainengOverlookCM", ResourceType = typeof(tokens))]
        KainengOverlookCM = 96419,
        [Display(Description = "HarvestTempleCM", ResourceType = typeof(tokens))]
        HarvestTempleCM = 95986,
        [Display(Description = "OldLionsCourtCM", ResourceType = typeof(tokens))]
        OldLionsCourtCM = 99204
        #endregion Strikes
    }
}
