using KpRefresher.Domain.Attributes;
using KpRefresher.Ressources;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum Token
    {
        #region Raids
        [Display(Description = "Vale", ResourceType = typeof(tokens)), Order(1)]
        Vale = 77705,
        [Display(Description = "Gorseval", ResourceType = typeof(tokens)), Order(2)]
        Gorseval = 77751,
        [Display(Description = "Sabetha", ResourceType = typeof(tokens)), Order(3)]
        Sabetha = 77728,

        [Display(Description = "Slothasor", ResourceType = typeof(tokens)), Order(4)]
        Slothasor = 77706,
        [Display(Description = "Matthias", ResourceType = typeof(tokens)), Order(5)]
        Matthias = 77679,

        [Display(Description = "Escort", ResourceType = typeof(tokens)), Order(6)]
        Escort = 78873,
        [Display(Description = "KeepConstruct", ResourceType = typeof(tokens)), Order(7)]
        KeepConstruct = 78902,
        [Display(Description = "Xera", ResourceType = typeof(tokens)), Order(8)]
        Xera = 78942,

        [Display(Description = "Cairn", ResourceType = typeof(tokens)), Order(9)]
        Cairn = 80623,
        [Display(Description = "MursaatOverseer", ResourceType = typeof(tokens)), Order(10)]
        MursaatOverseer = 80269,
        [Display(Description = "Samarog", ResourceType = typeof(tokens)), Order(11)]
        Samarog = 80087,
        [Display(Description = "Deimos", ResourceType = typeof(tokens)), Order(12)]
        Deimos = 80542,

        [Display(Description = "Desmina", ResourceType = typeof(tokens)), Order(13)]
        Desmina = 85993,
        [Display(Description = "River", ResourceType = typeof(tokens)), Order(14)]
        River = 85785,
        [Display(Description = "Statue", ResourceType = typeof(tokens)), Order(15)]
        Statue = 85800,
        [Display(Description = "Dhuum", ResourceType = typeof(tokens)), Order(16)]
        Dhuum = 85633,

        [Display(Description = "ConjuredAmalgamate", ResourceType = typeof(tokens)), Order(17)]
        ConjuredAmalgamate = 88543,
        [Display(Description = "TwinLargos", ResourceType = typeof(tokens)), Order(18)]
        TwinLargos = 88860,
        [Display(Description = "Qadim", ResourceType = typeof(tokens)), Order(19)]
        Qadim = 88645,

        [Display(Description = "Adina", ResourceType = typeof(tokens)), Order(20)]
        Adina = 91246,
        [Display(Description = "Sabir", ResourceType = typeof(tokens)), Order(21)]
        Sabir = 91270,
        [Display(Description = "QTP", ResourceType = typeof(tokens)), Order(22)]
        QTP = 91175,

        
        [Display(Description = "Greer", ResourceType = typeof(tokens)), Order(23)]
        Greer = 104047,
        [Display(Description = "Decima", ResourceType = typeof(tokens)), Order(24)]
        Decima = 103754,
        [Display(Description = "Ura", ResourceType = typeof(tokens)), Order(25)]
        Ura = 103996,
        #endregion Raids

        #region Strikes
        [Display(Description = "Boneskinner", ResourceType = typeof(tokens)), Order(26)]
        Boneskinner = 93781,

        [Display(Description = "AetherbladeHideout", ResourceType = typeof(tokens)), Order(27)]
        AetherbladeHideout = 95638,
        [Display(Description = "XunlaiJadeJunkyard", ResourceType = typeof(tokens)), Order(28)]
        XunlaiJadeJunkyard = 95982,
        [Display(Description = "KainengOverlook", ResourceType = typeof(tokens)), Order(29)]
        KainengOverlook = 97451,
        [Display(Description = "HarvestTemple", ResourceType = typeof(tokens)), Order(30)]
        HarvestTemple = 97132,
        [Display(Description = "OldLionsCourt", ResourceType = typeof(tokens)), Order(31)]
        OldLionsCourt = 99165,

        [Display(Description = "AetherbladeHideoutCM", ResourceType = typeof(tokens)), Order(32)]
        AetherbladeHideoutCM = 97269,
        [Display(Description = "XunlaiJadeJunkyardCM", ResourceType = typeof(tokens)), Order(33)]
        XunlaiJadeJunkyardCM = 96638,
        [Display(Description = "KainengOverlookCM", ResourceType = typeof(tokens)), Order(34)]
        KainengOverlookCM = 96419,
        [Display(Description = "HarvestTempleCM", ResourceType = typeof(tokens)), Order(35)]
        HarvestTempleCM = 95986,
        [Display(Description = "OldLionsCourtCM", ResourceType = typeof(tokens)), Order(36)]
        OldLionsCourtCM = 99204,

        [Display(Description = "CosmicObservatory", ResourceType = typeof(tokens)), Order(37)]
        CosmicObservatory = 100068,
        [Display(Description = "TempleOfFebe", ResourceType = typeof(tokens)), Order(38)]
        TempleOfFebe = 100858,

        [Display(Description = "CosmicObservatoryCM", ResourceType = typeof(tokens)), Order(39)]
        CosmicObservatoryCM = 101172,
        [Display(Description = "TempleOfFebeCM", ResourceType = typeof(tokens)), Order(40)]
        TempleOfFebeCM = 101542,
        #endregion Strikes
    }
}
