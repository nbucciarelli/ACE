using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Riptide.Managers;

namespace ACE.Server.WorldObjects
{
    public partial class Container : WorldObject
    {
        public bool TryAddToInventory(WorldObject worldObject, Next callback)
        {
            return TryAddToInventory(worldObject, callback, out _, placementPosition: 0, limitToMainPackOnly: false, burdenCheck: false);
        }

        private bool TryAddToInventory(WorldObject worldObject, Next callback, out Container container, int placementPosition = 0, bool limitToMainPackOnly = false, bool burdenCheck = true)
        {
            // refer to Container.cs

            // bug: should be root owner
            //if (this is Player player && burdenCheck)
            //{
            //    if (!player.HasEnoughBurdenToAddToInventory(worldObject))
            //    {
            //        container = null;
            //        return false;
            //    }
            //}

            IList<WorldObject> containerItems;

            if (worldObject.UseBackpackSlot)
            {
                containerItems = Inventory.Values.Where(i => i.UseBackpackSlot).ToList();

                if ((ContainerCapacity ?? 0) <= containerItems.Count)
                {
                    container = null;
                    return false;
                }
            }
            else
            {
                containerItems = Inventory.Values.Where(i => !i.UseBackpackSlot).ToList();

                if ((ItemCapacity ?? 0) <= containerItems.Count)
                {
                    // Can we add this to any side pack?
                    if (!limitToMainPackOnly)
                    {
                        var containers = Inventory.Values.OfType<Container>().ToList();
                        containers.Sort((a, b) => (a.Placement ?? 0).CompareTo(b.Placement ?? 0));

                        foreach (var sidePack in containers)
                        {
                            if (sidePack.TryAddToInventory(worldObject, out container, placementPosition, true))
                            {
                                EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
                                Value += (worldObject.Value ?? 0);

                                return true;
                            }
                        }
                    }

                    container = null;
                    return false;
                }
            }

            if (Inventory.ContainsKey(worldObject.Guid))
            {
                container = null;
                return false;
            }

            worldObject.Location = null;
            worldObject.Placement = ACE.Entity.Enum.Placement.Resting;

            worldObject.OwnerId = Guid.Full;
            worldObject.ContainerId = Guid.Full;
            worldObject.PlacementPosition = placementPosition; // Server only variable that we use to remember/restore the order in which items exist in a container

            // Move all the existing items PlacementPosition over.
            if (!worldObject.UseBackpackSlot)
                containerItems.Where(i => !i.UseBackpackSlot && i.PlacementPosition >= placementPosition).ToList().ForEach(i => i.PlacementPosition++);
            else
                containerItems.Where(i => i.UseBackpackSlot && i.PlacementPosition >= placementPosition).ToList().ForEach(i => i.PlacementPosition++);

            Inventory.Add(worldObject.Guid, worldObject);

            EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
            Value += (worldObject.Value ?? 0);

            container = this;

            OnAddItem(callback);

            return true;
        }

        private void OnAddItem(Next callback)
        {
            //Console.WriteLine("Storage.OnAddItem()");

            if (Inventory.Count > 0)
            {
                // Here we explicitly save the storage to the database to prevent item loss.
                // If the player adds an item to the storage, and the server crashes before the storage has been saved, the item will be lost.
                SaveBiotaToDatabase(callback);
            }
        }
    }

    partial class WorldObject
    {

        public void SaveBiotaToDatabase(Next afterSave, bool enqueueSave = true)
        {
            // Make sure all of our positions in the biota are up to date with our current cached values.
            foreach (var kvp in positionCache)
                Biota.SetPosition(kvp.Key, kvp.Value, BiotaDatabaseLock, out _);

            LastRequestedDatabaseSave = DateTime.UtcNow;
            ChangesDetected = false;

            if (enqueueSave)
            {
                CheckpointTimestamp = Time.GetUnixTime();
                //DatabaseManager.Shard.SaveBiota(Biota, BiotaDatabaseLock, null);
                DatabaseManager.Shard.SaveBiota(Biota, BiotaDatabaseLock, result =>
                {
                    if (!result)
                    {
                        if (this is Player player)
                        {
                            //todo: remove this later?
                            player.Session.Network.EnqueueSend(new GameMessageSystemChat("WARNING: A database save for this character has failed. As a result of this failure, it is possible for future saves to also fail. In order to avoid a potentially significant character rollback, please find a safe place, log out of the game and then reconnect & re-login. This error has also been logged to be further reviewed by ACEmulator team.", ChatMessageType.WorldBroadcast));
                        }
                    }
                    afterSave();
                });
            }
        }
    }
}
