using System;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;

namespace ACE.Server.Riptide
{
    public static class RiptideInventoryManager
    {
        //public static void TransferItemWithNetworking(Session session, Biota item, Character source, Character target)
        //{
        //    var obj = WorldObjectFactory.CreateWorldObject(item);
        //    if (source.Id != obj.GetProperty(PropertyInstanceId.Owner))
        //    {
        //        throw new Exception($"Source Player {source.Name} does not own item {obj.Name}");
        //    }
        //    var src = GetCurrentContainer(obj);
        //    var tar = GetFirstContainerWithFreeSpace(target);
        //    //TryTransferInventoryWithNetworking(session, obj, src, tar);
        //}

        //private static Container GetCurrentContainer(WorldObject obj)
        //{
        //    try {
        //        var src = new Container(DatabaseManager.Shard.GetBiota(obj.GetProperty(PropertyInstanceId.Container).Value));
        //        return src;
        //    } catch (Exception e)
        //    {
        //        return null;
        //    }
        //}

        //private static Container GetFirstContainerWithFreeSpace(Character character)
        //{
        //    // todo: replace this with an iteration over the character's packs.
        //    var container = new Container(DatabaseManager.Shard.GetBiota(character.Id));
        //    if (container.GetFreeInventorySlots(false) == 0)
        //    {
        //        throw new Exception($"Target Player {character.Name} has no free inventory slots!");
        //    }
        //    return container;
        //}

        //private static void GiveItemWithNetworking(Session session, WorldObject item, Container target)
        //{
        //    session.Network.EnqueueSend(new GameMessageCreateObject(item));

        //    if (item is Container itemAsContainer)
        //    {
        //        session.Network.EnqueueSend(new GameEventViewContents(session, itemAsContainer));

        //        foreach (var obj in itemAsContainer.Inventory.Values)
        //            session.Network.EnqueueSend(new GameMessageCreateObject(obj, Adminvision, false));
        //    }

        //    session.Network.EnqueueSend(
        //        new GameEventItemServerSaysContainId(session, item, container),
        //        new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.EncumbranceVal, EncumbranceVal ?? 0));

        //    if (item.WeenieType == WeenieType.Coin || item.WeenieType == WeenieType.Container)
        //        UpdateCoinValue();

        //    item.SaveBiotaToDatabase();

        //    return true;
        //}
    }
}
