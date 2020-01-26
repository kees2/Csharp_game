using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Factories;

namespace Engine.Models
{
    public class Location
    {
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public List<Quest> QuestsAvailableHere { get; set; } = new List<Quest>();

        public List<MonsterEncounter> MonsterHere { get; set; } = new List<MonsterEncounter>();

        public void AddMonster(int monsterID, int changeOfEncounter)
        {
            if(MonsterHere.Exists(m => m.MonsterID == monsterID))
            {
                MonsterHere.First(m => m.MonsterID == monsterID).ChanceOfEncountering = changeOfEncounter;
            }
            else
            {
                MonsterHere.Add(new MonsterEncounter(monsterID, changeOfEncounter));
            }
        }

        public Monster GetMonster()
        {
            if (!MonsterHere.Any())
            {
                return null;
            }
            else
            {
                int totalChances = MonsterHere.Sum(m => m.ChanceOfEncountering);

                int randomNumber = RandomNumberGenerator.NumberBetween(1, totalChances);


                int runningTotal = 0;

                foreach(MonsterEncounter monsterEncounter in MonsterHere)
                {
                    runningTotal += monsterEncounter.ChanceOfEncountering;

                    if(randomNumber <= runningTotal)
                    {
                        return MonsterFactory.GetMonster(monsterEncounter.MonsterID);
                    }
                }
                return MonsterFactory.GetMonster(MonsterHere.Last().MonsterID);
            }



        }
    }
}
