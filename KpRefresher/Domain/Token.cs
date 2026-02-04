using KpRefresher.Domain.Attributes;
using KpRefresher.Ressources;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum Token
    {
        #region Raids
        [Display(Description = "Vale", ResourceType = typeof(tokens)), Sort(1, 1)]
        Vale = 77705,
        [Display(Description = "Gorseval", ResourceType = typeof(tokens)), Sort(1, 2)]
        Gorseval = 77751,
        [Display(Description = "Sabetha", ResourceType = typeof(tokens)), Sort(1, 3)]
        Sabetha = 77728,

        [Display(Description = "Slothasor", ResourceType = typeof(tokens)), Sort(2, 1)]
        Slothasor = 77706,
        [Display(Description = "Matthias", ResourceType = typeof(tokens)), Sort(2, 2)]
        Matthias = 77679,

        [Display(Description = "Escort", ResourceType = typeof(tokens)), Sort(3, 1)]
        Escort = 78873,
        [Display(Description = "KeepConstruct", ResourceType = typeof(tokens)), Sort(3, 2)]
        KeepConstruct = 78902,
        [Display(Description = "Xera", ResourceType = typeof(tokens)), Sort(3, 3)]
        Xera = 78942,

        [Display(Description = "Cairn", ResourceType = typeof(tokens)), Sort(4, 1)]
        Cairn = 80623,
        [Display(Description = "MursaatOverseer", ResourceType = typeof(tokens)), Sort(4, 2)]
        MursaatOverseer = 80269,
        [Display(Description = "Samarog", ResourceType = typeof(tokens)), Sort(4, 3)]
        Samarog = 80087,
        [Display(Description = "Deimos", ResourceType = typeof(tokens)), Sort(4, 4)]
        Deimos = 80542,

        [Display(Description = "Desmina", ResourceType = typeof(tokens)), Sort(5, 1)]
        Desmina = 85993,
        [Display(Description = "River", ResourceType = typeof(tokens)), Sort(5, 2)]
        River = 85785,
        [Display(Description = "Statue", ResourceType = typeof(tokens)), Sort(5, 3)]
        Statue = 85800,
        [Display(Description = "Dhuum", ResourceType = typeof(tokens)), Sort(5, 4)]
        Dhuum = 85633,

        [Display(Description = "ConjuredAmalgamate", ResourceType = typeof(tokens)), Sort(6, 1)]
        ConjuredAmalgamate = 88543,
        [Display(Description = "TwinLargos", ResourceType = typeof(tokens)), Sort(6, 2)]
        TwinLargos = 88860,
        [Display(Description = "Qadim", ResourceType = typeof(tokens)), Sort(6, 3)]
        Qadim = 88645,

        [Display(Description = "Adina", ResourceType = typeof(tokens)), Sort(7, 1)]
        Adina = 91246,
        [Display(Description = "Sabir", ResourceType = typeof(tokens)), Sort(7, 2)]
        Sabir = 91270,
        [Display(Description = "QTP", ResourceType = typeof(tokens)), Sort(7, 3)]
        QTP = 91175,

        
        [Display(Description = "Greer", ResourceType = typeof(tokens)), Sort(8, 1)]
        Greer = 104047,
        [Display(Description = "Decima", ResourceType = typeof(tokens)), Sort(8, 2)]
        Decima = 103754,
        [Display(Description = "Ura", ResourceType = typeof(tokens)), Sort(8, 3)]
        Ura = 103996,
        #endregion Raids

        #region Strikes
        [Display(Description = "Boneskinner", ResourceType = typeof(tokens)), Sort(9, 2)]
        Boneskinner = 93781,

        [Display(Description = "AetherbladeHideout_Old", ResourceType = typeof(tokens)), Sort(10, 1)]
        AetherbladeHideout_Old = 95638,
        [Display(Description = "AetherbladeHideout", ResourceType = typeof(tokens)), Sort(10, 2)]
        AetherbladeHideout = 107067,
        [Display(Description = "XunlaiJadeJunkyard_Old", ResourceType = typeof(tokens)), Sort(10, 5)]
        XunlaiJadeJunkyard_Old = 95982,
        [Display(Description = "XunlaiJadeJunkyard", ResourceType = typeof(tokens)), Sort(10, 6)]
        XunlaiJadeJunkyard = 106954,
        [Display(Description = "KainengOverlook_Old", ResourceType = typeof(tokens)), Sort(10, 9)]
        KainengOverlook_Old = 97451,
        [Display(Description = "KainengOverlook", ResourceType = typeof(tokens)), Sort(10, 10)]
        KainengOverlook = 107026,
        [Display(Description = "HarvestTemple_Old", ResourceType = typeof(tokens)), Sort(10, 13)]
        HarvestTemple_Old = 97132,
        [Display(Description = "HarvestTemple", ResourceType = typeof(tokens)), Sort(10, 14)]
        HarvestTemple = 106956,
        [Display(Description = "OldLionsCourt_Old", ResourceType = typeof(tokens)), Sort(10, 17)]
        OldLionsCourt_Old = 99165,
        [Display(Description = "OldLionsCourt", ResourceType = typeof(tokens)), Sort(10, 18)]
        OldLionsCourt = 106951,

        [Display(Description = "AetherbladeHideoutCM_Old", ResourceType = typeof(tokens)), Sort(10, 3)]
        AetherbladeHideoutCM_Old = 97269,
        [Display(Description = "AetherbladeHideoutCM", ResourceType = typeof(tokens)), Sort(10, 4)]
        AetherbladeHideoutCM = 106935,
        [Display(Description = "XunlaiJadeJunkyardCM_Old", ResourceType = typeof(tokens)), Sort(10, 7)]
        XunlaiJadeJunkyardCM_Old = 96638,
        [Display(Description = "XunlaiJadeJunkyardCM", ResourceType = typeof(tokens)), Sort(10, 8)]
        XunlaiJadeJunkyardCM = 106999,
        [Display(Description = "KainengOverlookCM_Old", ResourceType = typeof(tokens)), Sort(10, 11)]
        KainengOverlookCM_Old = 96419,
        [Display(Description = "KainengOverlookCM", ResourceType = typeof(tokens)), Sort(10, 12)]
        KainengOverlookCM = 107028,
        [Display(Description = "HarvestTempleCM_Old", ResourceType = typeof(tokens)), Sort(10, 15)]
        HarvestTempleCM_Old = 95986,
        [Display(Description = "HarvestTempleCM", ResourceType = typeof(tokens)), Sort(10, 16)]
        HarvestTempleCM = 106910,
        [Display(Description = "OldLionsCourtCM_Old", ResourceType = typeof(tokens)), Sort(10, 19)]
        OldLionsCourtCM_Old = 99204,
        [Display(Description = "OldLionsCourtCM", ResourceType = typeof(tokens)), Sort(10, 20)]
        OldLionsCourtCM = 107132,

        [Display(Description = "CosmicObservatory_Old", ResourceType = typeof(tokens)), Sort(11, 1)]
        CosmicObservatory_Old = 100068,
        [Display(Description = "CosmicObservatory", ResourceType = typeof(tokens)), Sort(11, 2)]
        CosmicObservatory = 107087,
        [Display(Description = "TempleOfFebe_Old", ResourceType = typeof(tokens)), Sort(11, 5)]
        TempleOfFebe_Old = 100858,
        [Display(Description = "TempleOfFebe", ResourceType = typeof(tokens)), Sort(11, 6)]
        TempleOfFebe = 107114,

        [Display(Description = "CosmicObservatoryCM_Old", ResourceType = typeof(tokens)), Sort(11, 3)]
        CosmicObservatoryCM_Old = 101172,
        [Display(Description = "CosmicObservatoryCM", ResourceType = typeof(tokens)), Sort(11, 4)]
        CosmicObservatoryCM = 106940,
        [Display(Description = "TempleOfFebeCM_Old", ResourceType = typeof(tokens)), Sort(11, 7)]
        TempleOfFebeCM_Old = 101542,
        [Display(Description = "TempleOfFebeCM", ResourceType = typeof(tokens)), Sort(11, 8)]
        TempleOfFebeCM = 107065,

        [Display(Description = "GuardiansGlade", ResourceType = typeof(tokens)), Sort(12, 1)]
        GuardiansGlade = 106994,

        [Display(Description = "IcebroodCoffer", ResourceType = typeof(tokens)), Sort(13, 1)]
        IcebroodCoffer = 106909,
        [Display(Description = "WeeklyQuickplayRaidCache", ResourceType = typeof(tokens)), Sort(13, 2)]
        WeeklyQuickplayRaidCache = 107019,
        #endregion Strikes
    }
}
