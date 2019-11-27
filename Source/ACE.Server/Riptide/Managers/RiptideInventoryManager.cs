using System;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Network.Managers;
using ACE.Server.WorldObjects;
using log4net;

namespace ACE.Server.Riptide.Managers
{
    public interface IInventoryManager
    {
        Character GetOwner(WorldObject item);
        Container GetContainer(WorldObject item);
        Container GetContainer(Character character);
        void CreateItem(Character recipient, WorldObject item);
        void TradeItem(Character sender, Character recipient, WorldObject item, int amount);
    }

    internal class RiptideInventoryManager: IInventoryManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Character GetOwner(WorldObject item)
        {
            if (item == null)
                return null;
            if (item.OwnerId == null)
                return null;
            if (!item.OwnerId.HasValue)
                return null;
            Character result = RiptideManager.Database.GetCharacter(item.OwnerId.Value);
            if (result == null)
                return null;
            return result;
        }

        public Container GetContainer(WorldObject item)
        {
            if (item == null)
                return null;
            if (item.ContainerId == null)
                return null;
            if (!item.ContainerId.HasValue)
                return null;
            Biota biota = RiptideManager.Database.GetBiota(item.ContainerId.Value);
            if (biota == null)
                return null;
            Container container = new Container(biota);
            if (container == null)
                return null;
            return container;
        }

        public Container GetContainer(Character character)
        {
            Biota biota = RiptideManager.Database.GetBiota(character.Id);
            Container playerInventory = new Container(biota);
            return playerInventory;
        }

        public void CreateItem(Character recipient, WorldObject item)
        {
            if (recipient == null)
                throw new Exception("Character cannot be null.");
            if (item == null)
                throw new Exception("Item cannot be null.");
            Character owner = GetOwner(item);
            if (owner != null)
                throw new Exception($"Use 'TradeItem' instead of 'CreateItem'. (Precondition violated: Item {item.Name} already has an owner: {owner.Name}.)");

            AddItemToInventory(recipient, item);
            //int effectiveAmount = RemoveItemFromInventory(owner, item);
            //AddItemToInventory(character, toGive, amount);
        }

        public void TradeItem(Character sender, Character recipient, WorldObject item, int amount = -1)
        {
            if (sender == null)
                throw new Exception("Sender cannot be null.");
            if (recipient == null)
                throw new Exception("Recipient cannot be null.");
            if (item == null)
                throw new Exception("Item cannot be null.");
            WorldObject created; // this is only used when splitting stacks.
            SplitStack(item, amount, out created, save: true);
            WorldObject package = (created == null) ? item : created;
            bool success = _TradeItem(sender, recipient, package);
            if (!success)
            {
                log.Error($"Failed to trade items!");
            }
        }

        private void SplitStack(WorldObject original, int amount, out WorldObject created, bool save = false)
        {
            created = null;
            if (!IsStackable(original))
                return;  // no-op
            if (amount >= original.StackSize)
                return;  // no-op
            original.SetStackSize(original.StackSize - amount);
            created = WorldObjectFactory.CreateNewWorldObject(original.Biota.WeenieClassId);
            created.SetStackSize(amount);
            if (save)
            {
                original.SaveBiotaToDatabase();
                created.SaveBiotaToDatabase();
            }
        }

        private bool _TradeItem(Character sender, Character recipient, WorldObject item)
        {
            bool result = true;
            if (result)
                result &= RemoveItemFromInventory(sender, item);
            if (result)
                result &= AddItemToInventory(recipient, item);
            return result;
        }

        public bool IsStackable(WorldObject item) { return item.MaxStackSize.HasValue; }

        private bool AddItemToInventory(Character character, WorldObject item)
        {
            Session session = RiptideManager.Sessions.GetSession(character);
            if (session != null)
            {
                /** Do it WITH networking */
                if (session.Player.GetFreeInventorySlots(true) == 0)
                    throw new Exception($"Character {character.Id} has no free inventory space!");
                return session.Player.TryAddToInventory(item, placementPosition: 0);
            } else
            {
                /** Do it WITHOUT networking */
                Container inventory = GetContainer(character);
                return inventory.TryAddToInventory(item, placementPosition: 0, burdenCheck: false);
            }
        }

        private bool RemoveItemFromInventory(Character character, WorldObject item)
        {
            if (character == null)
            {
                return false;
            } else
            {
                Session session = RiptideManager.Sessions.GetSession(character);
                if (session != null)
                {
                    // the current owner is logged into the game.
                    return session.Player.TryConsumeFromInventoryWithNetworking(item);
                } else
                {
                    // the current owner is not logged in right now.
                    Container container = GetContainer(item);
                    if (container == null)
                    {
                        log.Error($"Item {item.Name} has no container!");
                        return false;
                    } else
                    {
                        return container.TryRemoveFromInventory(item.Guid);
                    }
                }
            }
            
        }

        //private static Container GetContainerOfObject(WorldObject obj)
        //{
        //    try
        //    {
        //        var src = new Container(DatabaseManager.Shard.GetBiota(obj.GetProperty(PropertyInstanceId.Container).Value));
        //        return src;
        //    }
        //    catch (Exception e)
        //    {
        //        return null;
        //    }
        //}
    }
}
