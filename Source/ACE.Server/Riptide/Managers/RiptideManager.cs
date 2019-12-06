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
using ACE.Server.Riptide.Managers.Networking;
using ACE.Server.WorldObjects;


namespace ACE.Server.Riptide.Managers
{
    public static class RiptideManager
    {
        public static IDatabaseManager Database = new RiptideDatabaseManager();
        public static IInventoryManager Inventory = new RiptideInventoryManager();
        public static ISessionManager Sessions = new RiptideSessionManager();
        public static IPlayerManager Players = new RiptidePlayerManager();
        public static IGameActionManager GameActions = new GameActionManager();
        public static IS2CManager S2C = new S2CManager();
    }
}
