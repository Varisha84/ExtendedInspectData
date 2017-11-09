using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ZoneInspectData
{
    class SingleZoneGrowingData
    {
        public int[] growRatesAbsolute;
        public int totalPlantedCount;
        public List<Thing> harvestablePlants;
        public List<Thing> fullyGrownPlants;
        public Zone_Growing zone;

        public int growthRateMaxCount
        {
            get
            {
                int max = 0;

                for (int j=0; j<growRatesAbsolute.Length; j++)
                {
                    if (growRatesAbsolute[j] > max)
                    {
                        max = growRatesAbsolute[j];
                    }
                }
                return  max;
            }
        }

        public SingleZoneGrowingData()
        {
            harvestablePlants = new List<Thing>();
            fullyGrownPlants = new List<Thing>();
            growRatesAbsolute = new int[101];
            for (int i = 0; i < 101; i++)
            {
                growRatesAbsolute[i] = 0;
            }
        }

        public void Clear()
        {
            zone = null;
            totalPlantedCount = 0;
            harvestablePlants.Clear();
            fullyGrownPlants.Clear();
            for (int i = 0; i < 101; i++)
            {
                growRatesAbsolute[i] = 0;
            }
        }
    }    
}
