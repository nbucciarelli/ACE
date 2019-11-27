using System;
using ACE.Database;
using ACE.Database.Models.Auth;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Network.Managers;
using ACE.Server.WorldObjects;


namespace ACE.Server.Riptide.Managers
{
    public interface IDatabaseManager
    {
        Account GetAccount(uint id);
        Character GetCharacter(uint id);
        Biota GetBiota(uint id);
        WorldObject GetWorldObject(uint id);
    }

    internal class RiptideDatabaseManager: IDatabaseManager
    {
        public Account GetAccount(uint id) {
            Account account = DatabaseManager.Authentication.GetAccountById(id);
            return account;
        }

        public Character GetCharacter(uint guid)
        {
            Character character = DatabaseManager.Shard.GetCharacterByGuid(guid);
            return character;
        }

        public Biota GetBiota(uint id)
        {
            Biota biota = DatabaseManager.Shard.GetBiota(id);
            return biota;
        }

        public WorldObject GetWorldObject(uint id)
        {
            Biota biota = GetBiota(id);
            WorldObject worldObject = WorldObjectFactory.CreateWorldObject(biota);
            return worldObject;
        }
    }
}
