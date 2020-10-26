using System;

namespace Chat_Client
{
    public static class EliteSpec
    {
        public enum EliteSpecs
        {
            //Mesmer
            Chronomancer = 40,
            Mirage = 59,

            //Necromancer
            Reaper = 34,
            Scourge = 60,

            //Elementalist
            Tempest = 48,
            Weaver = 56,

            //Revenant
            Herald = 52,
            Renegade = 63,

            //Warrior
            Berserker = 18,
            Spellbreaker = 61,

            //Guardian 
            Dragonhunter = 27,
            Firebrand = 62,

            //Ranger 
            Druid = 5,
            Soulbeast = 55,

            //Engineer 
            Scrapper = 43,
            Holosmith = 57,

            //Thief 
            Daredevil = 7,
            Deadeye = 58
        }

        public static string GetElite(int spec)
        {
            if (Enum.IsDefined(typeof(EliteSpecs), spec))
                return ((EliteSpecs)spec).ToString();
            return null;
        }
    }
}
