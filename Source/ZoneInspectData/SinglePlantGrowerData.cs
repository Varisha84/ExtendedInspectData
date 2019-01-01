using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ExtendedInspectData
{
    class SinglePlantGrowerData
    {
        public int[] growRatesAbsolute;
        public int totalOccupiedCells;
        public int totalPlantedCount;
        public List<Thing> harvestablePlants;
        public List<Thing> fullyGrownPlants;
        public Building_PlantGrower plantGrower;

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

        public SinglePlantGrowerData()
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
            plantGrower = null;
            totalOccupiedCells = 0;
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
