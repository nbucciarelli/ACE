using System;
using ACE.Database.Models.Shard;
using ACE.Server.Network;
using ACE.Server.WorldObjects;

namespace ACE.Server.Riptide.Managers.Networking
{
    public interface IS2CManager
    {
        void RefreshPlayerInventory(Player player);
        void RefreshPlayerInventory(Character character);
    }

    internal class S2CManager: IS2CManager
    {
        public S2CManager()
        {
        }

        public void RefreshPlayerInventory(Character character)
        {
            Session session = RiptideManager.Sessions.GetSession(character);
            if (session != null)
                RefreshPlayerInventory(session.Player);
        }

        public void RefreshPlayerInventory(Player player)
        {
            if (player != null)
            {
                // simulates logging into the game.
                Next next = (() =>
                {
                    //player.SendInventoryAndWieldedItems();
                });
                player.RefreshInventoryServerSide(next);
            }
        }
    }
}
