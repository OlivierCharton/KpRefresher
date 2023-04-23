using KpRefresher.Domain.Attributes;
using KpRefresher.Ressources;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Domain
{
    public enum RaidBoss
    {
        [Display(Name = "Vale Guardian", Description = "vale_guardian", ResourceType = typeof(raidboss))]
        [Wing(1)]
        vale_guardian,
        [Display(Name = "Spirit Woods", Description = "spirit_woods", ResourceType = typeof(raidboss))]
        [Wing(1)]
        spirit_woods,
        [Display(Name = "Gorseval", Description = "gorseval", ResourceType = typeof(raidboss))]
        [Wing(1)]
        gorseval,
        [Display(Name = "Sabetha", Description = "sabetha", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(1)]
        sabetha,

        [Display(Name = "Slothasor", Description = "slothasor", ResourceType = typeof(raidboss))]
        [Wing(2)]
        slothasor,
        [Display(Name = "Bandit Trio", Description = "bandit_trio", ResourceType = typeof(raidboss))]
        [Wing(2)]
        bandit_trio,
        [Display(Name = "Matthias", Description = "matthias", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(2)]
        matthias,

        [Display(Name = "Escort", Description = "escort", ResourceType = typeof(raidboss))]
        [Wing(3)]
        escort,
        [Display(Name = "Keep Construct", Description = "keep_construct", ResourceType = typeof(raidboss))]
        [Wing(3)]
        keep_construct,
        [Display(Name = "Twisted Castle", Description = "twisted_castle", ResourceType = typeof(raidboss))]
        [Wing(3)]
        twisted_castle,
        [Display(Name = "Xera", Description = "xera", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(3)]
        xera,

        [Display(Name = "Cairn", Description = "cairn", ResourceType = typeof(raidboss))]
        [Wing(4)]
        cairn,
        [Display(Name = "Mursaat Overseer", Description = "mursaat_overseer", ResourceType = typeof(raidboss))]
        [Wing(4)]
        mursaat_overseer,
        [Display(Name = "Samarog", Description = "samarog", ResourceType = typeof(raidboss))]
        [Wing(4)]
        samarog,
        [Display(Name = "Deimos", Description = "deimos", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(4)]
        deimos,

        [Display(Name = "Soulless Horror", Description = "soulless_horror", ResourceType = typeof(raidboss))]
        [Wing(5)]
        soulless_horror,
        [Display(Name = "River Of Souls", Description = "river_of_souls", ResourceType = typeof(raidboss))]
        [Wing(5)]
        river_of_souls,
        [Display(Name = "Statues Of Grenth", Description = "statues_of_grenth", ResourceType = typeof(raidboss))]
        [Wing(5)]
        statues_of_grenth,
        [Display(Name = "Voice In The Void", Description = "voice_in_the_void", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(5)]
        voice_in_the_void,

        [Display(Name = "Conjured Amalgamate", Description = "conjured_amalgamate", ResourceType = typeof(raidboss))]
        [Wing(6)]
        conjured_amalgamate,
        [Display(Name = "Twin Largos", Description = "twin_largos", ResourceType = typeof(raidboss))]
        [Wing(6)]
        twin_largos,
        [Display(Name = "Qadim", Description = "qadim", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(6)]
        qadim,

        [Display(Name = "Gate", Description = "gate", ResourceType = typeof(raidboss))]
        [Wing(7)]
        gate,
        [Display(Name = "Adina",  Description = "adina", ResourceType = typeof(raidboss))]
        [Wing(7)]
        adina,
        [Display(Name = "Sabir", Description = "sabir", ResourceType = typeof(raidboss))]
        [Wing(7)]
        sabir,
        [Display(Name = "Qadim The Peerless", Description = "qadim_the_peerless", ResourceType = typeof(raidboss)), FinalBoss]
        [Wing(7)]
        qadim_the_peerless
    }
}
