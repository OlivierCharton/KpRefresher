using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum Token
    {
        #region Raids
        [Display(Name = "Vale Guardian Fragment")]
        Vale = 77705,
        [Display(Name = "Gorseval Tentacle Piece")]
        Gorseval = 77751,
        [Display(Name = "Sabetha Flamethrower Fragment Piece")]
        Sabetha = 77728,

        [Display(Name = "Slothasor Mushroom")]
        Slothasor = 77706,
        [Display(Name = "White Mantle Abomination Crystal")]
        Matthias = 77679,

        [Display(Name = "Turret Fragment")]
        Escort = 78873,
        [Display(Name = "Keep Construct Rubble")]
        KeepConstruct = 78902,
        [Display(Name = "Ribbon Scrap")]
        Xera = 78942,

        [Display(Name = "Cairn Fragment")]
        Cairn = 80623,
        [Display(Name = "Recreation Room Floor Fragment")]
        MursaatOverseer = 80269,
        [Display(Name = "Impaled Prisoner Token")]
        Samarog = 80087,
        [Display(Name = "Fragment of Saul's Burden")]
        Deimos = 80542,

        [Display(Name = "Desmina's Token")]
        Desmina = 85993,
        [Display(Name = "River of Souls Token")]
        River = 85785,
        [Display(Name = "Statue Token")]
        Statue = 85800,
        [Display(Name = "Dhuum's Token")]
        Dhuum = 85633,

        [Display(Name = "Conjured Amalgamate Token")]
        ConjuredAmalgamate = 88543,
        [Display(Name = "Twin Largos Token")]
        TwinLargos = 88860,
        [Display(Name = "Qadim's Token")]
        Qadim = 88645,

        [Display(Name = "Cardinal Adina's Token")]
        Adina = 91246,
        [Display(Name = "Cardinal Sabir's Token")]
        Sabir = 91270,
        [Display(Name = "Ether Djinn's Token")]
        QTP = 91175,
        #endregion Raids

        #region Strikes
        [Display(Name = "Boneskinner Ritual Vial")]
        Boneskinner= 93781,

        [Display(Name = "Mai Trin's Coffer")]
        AetherbladeHideout = 95638,
        [Display(Name = "Ankka's Coffer")]
        XunlaiJadeJunkyard = 95982,
        [Display(Name = "Minister Li's Coffer")]
        KainengOverlook = 97451,
        [Display(Name = "Void's Coffer")]
        HarvestTemple = 97132,
        [Display(Name = "Assault Knights' Coffer")]
        OldLionsCourt = 99165,

        [Display(Name = "Mai Trin's Magnificent Coffer")]
        AetherbladeHideoutCM = 97269,
        [Display(Name = "Ankka's Magnificent Coffer")]
        XunlaiJadeJunkyardCM = 96638,
        [Display(Name = "Minister Li's Magnificent Coffer")]
        KainengOverlookCM = 96419,
        [Display(Name = "Void's Magnificent Coffer")]
        HarvestTempleCM = 95986,
        [Display(Name = "Assault Knights' Magnificent Coffer")]
        OldLionsCourtCM = 99204
        #endregion Strikes
    }
}
