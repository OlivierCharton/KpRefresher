using KpRefresher.Domain.Attributes;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum RaidBoss
    {
        [Display(Name = "Vale Guardian")]
        [Wing(1)]
        vale_guardian,
        [Display(Name = "Spirit Woods")]
        [Wing(1)]
        spirit_woods,
        [Display(Name = "Gorseval")]
        [Wing(1)]
        gorseval,
        [Display(Name = "Sabetha"), FinalBoss]
        [Wing(1)]
        sabetha,

        [Display(Name = "Slothasor")]
        [Wing(2)]
        slothasor,
        [Display(Name = "Bandit Trio")]
        [Wing(2)]
        bandit_trio,
        [Display(Name = "Matthias"), FinalBoss]
        [Wing(2)]
        matthias,

        [Display(Name = "Escort")]
        [Wing(3)]
        escort,
        [Display(Name = "Keep Construct")]
        [Wing(3)]
        keep_construct,
        [Display(Name = "Twisted Castle")]
        [Wing(3)]
        twisted_castle,
        [Display(Name = "Xera"), FinalBoss]
        [Wing(3)]
        xera,

        [Display(Name = "Cairn")]
        [Wing(4)]
        cairn,
        [Display(Name = "Mursaat Overseer")]
        [Wing(4)]
        mursaat_overseer,
        [Display(Name = "Samarog")]
        [Wing(4)]
        samarog,
        [Display(Name = "Deimos"), FinalBoss]
        [Wing(4)]
        deimos,

        [Display(Name = "Desmina")]
        [Wing(5)]
        soulless_horror,
        [Display(Name = "River of Souls")]
        [Wing(5)]
        river_of_souls,
        [Display(Name = "Statues of Grenth")]
        [Wing(5)]
        statues_of_grenth,
        [Display(Name = "Dhuum"), FinalBoss]
        [Wing(5)]
        voice_in_the_void,

        [Display(Name = "Conjured Amalgamate")]
        [Wing(6)]
        conjured_amalgamate,
        [Display(Name = "Twin Largos")]
        [Wing(6)]
        twin_largos,
        [Display(Name = "Qadim"), FinalBoss]
        [Wing(6)]
        qadim,

        [Display(Name = "Gate")]
        [Wing(7)]
        gate,
        [Display(Name = "Adina")]
        [Wing(7)]
        adina,
        [Display(Name = "Sabir")]
        [Wing(7)]
        sabir,
        [Display(Name = "Qadim the Peerless"), FinalBoss]
        [Wing(7)]
        qadim_the_peerless
    }
}
