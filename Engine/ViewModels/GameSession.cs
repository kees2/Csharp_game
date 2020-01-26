using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Engine.Models;
using Engine.Factories;
using Engine;
using Engine.EventArgs;
 

namespace Engine.ViewModels
{
    public class GameSession : BaseNotificationClass
    {
        public event EventHandler<GameMessageEventArgs> OnMessageRaised;

        private Location _currentLocation;
        private Monster _currentMonster;

        public Player CurrentPlayer{ get; set; }
        public World CurrentWorld { get; set; }

        public Location CurrentLocation
        { 
            get{return _currentLocation;}
            set{
                _currentLocation = value;

                OnPropertyChanged(nameof(CurrentLocation));
                OnPropertyChanged(nameof(HasLocationToNorth));
                OnPropertyChanged(nameof(HasLocationToEast));
                OnPropertyChanged(nameof(HasLocationToWest));
                OnPropertyChanged(nameof(HasLocationToSouth));

                GivePlayerQuestsAtLocation();
                GetMonsterAtLocation();
            }
        }

        public Monster CurrentMonster
        {
            get { return _currentMonster; }
            set { _currentMonster = value;

                OnPropertyChanged(nameof(CurrentMonster));
                OnPropertyChanged(nameof(HasMonster));

                if(CurrentMonster != null)
                {
                    RaiseMessage("");
                    RaiseMessage($"You see a {CurrentMonster.Name} here!");
                }

            }
        }

        public Weapon CurrentWeapon{ set; get; }

        
        public bool HasLocationToNorth
        {
            get{
                     return CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate +1) != null;
            }        
        }
        public bool HasLocationToEast
        {
            get{
                     return CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate) != null;
            }        
        }
        public bool HasLocationToSouth
        {
            get{
                     return CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate -1) != null;
            }        
        }
        public bool HasLocationToWest
        {
            get{
                     return CurrentWorld.LocationAt(CurrentLocation.XCoordinate -1, CurrentLocation.YCoordinate) != null;
            }        
        }

        public bool HasMonster => CurrentMonster != null;

        public GameSession()
        {
            CurrentPlayer = new Player
            {
                Name = "Kees",
                CharacterClass = "Fighter",
                HitPoints = 10,
                Gold = 1000000,
                ExperiencePoints = 0,
                Level = 1
            };

            if (!CurrentPlayer.Weapons.Any())
            {
                CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(1001));
            }


            CurrentWorld = WorldFactory.CreateWorld();

            CurrentLocation = CurrentWorld.LocationAt(2,0);

        }


        public void MoveNorth()
        {
            if (HasLocationToNorth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1);
            }
        }

        public void MoveEast()
        {
            if (HasLocationToEast)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate);

            }
        }
        public void MoveSouth()
        {
            if (HasLocationToSouth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1);
            }
        }
        public void MoveWest()
        {
            if (HasLocationToWest)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate);
            }
        }

        private void GivePlayerQuestsAtLocation()
        {
            foreach (Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                if (!CurrentPlayer.Quests.Any(q => q.PlayerQuest.ID == quest.ID))
                {
                    CurrentPlayer.Quests.Add(new QuestStatus(quest));
                }
            }
        }

        private void GetMonsterAtLocation()
        {
            CurrentMonster = CurrentLocation.GetMonster();
        }

        public void AttackCurrentMonster()
        {
            if(CurrentWeapon == null)
            {
                RaiseMessage("You must select a weapon, to attack the monster");
                return;
            }

            int damageToMonster = RandomNumberGenerator.NumberBetween(CurrentWeapon.MinimumDamage, CurrentWeapon.MaximumDamage);

            if(damageToMonster == 0)
            {
                RaiseMessage($"You missed the {CurrentMonster.Name}.");
            }
            else
            {
                CurrentMonster.HitPoints -=  damageToMonster;
                RaiseMessage($"You hit the {CurrentMonster.Name} for {damageToMonster}!");
            }

            if(CurrentMonster.HitPoints <= 0)
            {
                RaiseMessage("");
                RaiseMessage($"You defeated the {CurrentMonster.Name}!");

                CurrentPlayer.ExperiencePoints += CurrentMonster.RewardExperiencePoints;
                RaiseMessage($"You receive {CurrentMonster.RewardExperiencePoints}");

                CurrentPlayer.Gold += CurrentMonster.RewardGold;
                RaiseMessage($"You receive {CurrentMonster.RewardGold}!");

                foreach(ItemQuantity itemQuantity in CurrentMonster.Inventory)
                {
                    GameItem item = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                    CurrentPlayer.AddItemToInventory(item);
                    RaiseMessage($"You receive {itemQuantity.Quantity} {item.Name}!");

                    GetMonsterAtLocation();
                }
            }
            else
            {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(CurrentMonster.MinimumDamage, CurrentMonster.MaximumDamage);

                if(damageToPlayer == 0)
                {
                    RaiseMessage("The monster attack but misses you");
                }
                else
                {
                    CurrentPlayer.HitPoints -= damageToPlayer;
                    RaiseMessage($"The {CurrentMonster.Name} hit you for {damageToPlayer} hitpoints");
                }

                if(CurrentPlayer.HitPoints == 0)
                {
                    RaiseMessage($"The {CurrentMonster.Name} killed you");
                    CurrentLocation = CurrentWorld.LocationAt(0, -1);
                    CurrentPlayer.HitPoints = CurrentPlayer.Level * 10;
                }



            }

        }

        private void RaiseMessage(string message)
        {
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message));
        }

    }
         
}
