using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum RaidBoss
    {
        [Display(Name = "Vale Guardian")]
        vale_guardian,
        [Display(Name = "Spirit Woods")]
        spirit_woods,
        [Display(Name = "Gorseval")]
        gorseval,
        [Display(Name = "Sabetha"), FinalBoss]
        sabetha,

        [Display(Name = "Slothasor")]
        slothasor,
        [Display(Name = "Bandit Trio")]
        bandit_trio,
        [Display(Name = "Matthias"), FinalBoss]
        matthias,

        [Display(Name = "Escort")]
        escort,
        [Display(Name = "Keep Construct")]
        keep_construct,
        [Display(Name = "Twisted Castle")]
        twisted_castle,
        [Display(Name = "Xera"), FinalBoss]
        xera,

        [Display(Name = "Cairn")]
        cairn,
        [Display(Name = "Mursaat Overseer")]
        mursaat_overseer,
        [Display(Name = "Samarog")]
        samarog,
        [Display(Name = "Deimos"), FinalBoss]
        deimos,

        [Display(Name = "Desmina")]
        soulless_horror,
        [Display(Name = "River of Souls")]
        river_of_souls,
        [Display(Name = "Statues of Grenth")]
        statues_of_grenth,
        [Display(Name = "Dhuum"), FinalBoss]
        voice_in_the_void,

        [Display(Name = "Conjured Amalgamate")]
        conjured_amalgamate,
        [Display(Name = "Twin Largos")]
        twin_largos,
        [Display(Name = "Qadim"), FinalBoss]
        qadim,

        [Display(Name = "Gate")]
        gate,
        [Display(Name = "Adina")]
        adina,
        [Display(Name = "Sabir")]
        sabir,
        [Display(Name = "Qadim the Peerless"), FinalBoss]
        qadim_the_peerless
    }
}
