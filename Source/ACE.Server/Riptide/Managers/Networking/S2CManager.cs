using System;
using ACE.Database.Models.Shard;
using ACE.Server.Network;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;

namespace ACE.Server.Riptide.Managers.Networking
{
    public interface IS2CManager
    {
        void AddItem(Character character, WorldObject item);
        void RemoveItem(Character character, WorldObject item);
        //void RefreshPlayerInventory(Player player);
        //void RefreshPlayerInventory(Character character);
        void RefreshPlayerInventory(Character character, Next callback=null);
    }

    internal class S2CManager : IS2CManager
    {
        public S2CManager()
        {
        }

        public void AddItem(Character character, WorldObject item)
        {
            Session session = RiptideManager.Sessions.GetSession(character);
            if (session != null)
                RemoveItem(session.Player, item);
        }

        public void AddItem(Player player, WorldObject item)
        {
            if (player != null)
            {
                player.Session.Network.EnqueueSend(new GameMessageCreateObject(item));
            }
        }

        public void RemoveItem(Character character, WorldObject item)
        {
            Session session = RiptideManager.Sessions.GetSession(character);
            if (session != null)
                RemoveItem(session.Player, item);
        }

        public void RemoveItem(Player player, WorldObject item)
        {
            if (player != null)
            {
                player.Session.Network.EnqueueSend(new GameMessageInventoryRemoveObject(item));
                //player.SendNetwork(new GameMessageDeleteObject(item), false);
            }
        }

        public void RefreshPlayerInventory(Character character, Next next=null)
        {
            Session session = RiptideManager.Sessions.GetSession(character);
            if (session != null)
                RefreshPlayerInventory(session.Player, next);
        }

        public void RefreshPlayerInventory(Player player, Next next=null)
        {
            if (next == null)
                next = () => { };

            if (player != null)
            {
                player.RefreshInventoryServerSide(next);
            }
        }
    }
}
