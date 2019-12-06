using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity.Actions;
using ACE.Server.Factories;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using log4net;

// note to self: check out the PlayerManager
// you can log out/log in players.

namespace ACE.Server.Riptide.Managers
{
    public interface IPlayerInventory {
        Character Character { get; }
        bool Online { get; }
        Dictionary<ObjectGuid, WorldObject> Inventory { get; }
        Container AsContainer();
        string Print();
    }

    internal class PlayerInventory : Container, IPlayerInventory
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Character Character { get; private set; }
        public bool Online { get; private set; }
        private Player Player { get; set; }

        internal PlayerInventory(Biota biota) : base(biota) {
            Character = RiptideManager.Database.GetCharacter(biota.Id);
            SetPlayer();
        }

        public Container AsContainer() { return this as Container; }
        public string Print()
        {
            StringBuilder sb = new StringBuilder();

            foreach (WorldObject s in Sort(GetBackpackSlots()))
            {
                sb.Append($"Slot[{s.PlacementPosition}]: {s.Name} ({s.Guid.Full})\n");
            }

            sb.Append($"Container[0]: Main Inventory\n");
            foreach (WorldObject i in Sort(GetItems(this)))
            {
                if ((i.MaxStackSize ?? -1) > -1)
                {
                    sb.Append($"- Item[{i.PlacementPosition}]: {i.Name} (x{i.StackSize ?? -1}) ({i.Guid.Full})\n");
                } else
                {
                    sb.Append($"- Item[{i.PlacementPosition}]: {i.Name} ({i.Guid.Full})\n");
                }
            }
            var containers = Sort(GetContainers()).ToList();
            foreach (WorldObject c in containers)
            {
                sb.Append($"Container[{c.PlacementPosition}]: {c.Name} ({c.Guid.Full})\n");
                foreach (WorldObject i in Sort(GetItems(c)))
                {
                    if ((i.MaxStackSize ?? -1) > -1)
                    {
                        sb.Append($"- Item[{i.PlacementPosition}]: {i.Name} (x{i.StackSize ?? -1}) ({c.Guid.Full})\n");
                    }
                    else
                    {
                        sb.Append($"- Item[{i.PlacementPosition}]: {i.Name} ({c.Guid.Full})\n");
                    }
                }
            }
            //List<WorldObject> items = Inventory.Values.ToList();
            //foreach (WorldObject i in items)
            //{
            //    sb.Append($"Item[{i.PlacementPosition ?? null}] -> '{i.Name ?? null}'\n");
            //}
            return $"{sb}";
        }

        private IEnumerable<WorldObject> GetBackpackSlots()
        {
            return from item in Inventory.Values
                   where (item.RequiresPackSlot | (item.WeenieType == ACE.Entity.Enum.WeenieType.Container))
                   orderby item.PlacementPosition ?? 0
                   select item;
        }

        private bool _RequiresPackSlot(WorldObject wo) { return wo.RequiresPackSlot | wo.WeenieType == ACE.Entity.Enum.WeenieType.Container; }

        private IEnumerable<WorldObject> GetItems(WorldObject container)
        {
            //return from item in Inventory.Values
            //       where !item.RequiresPackSlot & (item.ContainerId == container.Guid.Full)
            //       orderby item.PlacementPosition ?? 0 select item;
            return from item in Inventory.Values
                   where (item.ContainerId == container.Guid.Full) & !_RequiresPackSlot(item)
                   orderby item.PlacementPosition ?? 0
                   select item;
        }

        private IEnumerable<WorldObject> GetContainers() {
            return Inventory.Values.Where(i => i.WeenieType == ACE.Entity.Enum.WeenieType.Container);
        }

        private IEnumerable<WorldObject> Sort(IEnumerable<WorldObject> items)
        {
            //IComparer<WorldObject> Sort = Compare;
            return from item in items orderby item.PlacementPosition ?? 0 select item;
        }

        private void SetPlayer()
        {
            Player = PlayerManager.GetOnlinePlayer(Character.Id);
            Online = Player != null;
            if (!Online) { }

            ReloadFromDatabase();
        }

        private void ReloadFromDatabase()
        {
            DatabaseManager.Shard.GetInventoryInParallel(Biota.Id, false, biotas =>
            {
                log.Info($"GetInventoryInParallel({Biota.Id})");
                log.Info($"{Character.Id}");
                log.Info($"#biotas: {biotas.Count}");
                SortBiotasIntoInventory(biotas);
                log.Info($"InventoryLoaded: {InventoryLoaded}");
                //EnqueueAction(new ActionEventDelegate(() => SortBiotasIntoInventory(biotas)));
            });
            int timeout = 2500;
            while (!InventoryLoaded)
            {
                Thread.Sleep(50);
                timeout -= 50;
                if (timeout <= 0)
                    throw new Exception($"Timeout");
            }
        }
    }
    //internal class PlayerInventory: Container, IPlayerInventory
    //{
    //    internal PlayerInventory(Biota biota): base(biota)
    //    {
    //        Player online = PlayerManager.GetOnlinePlayer(biota.Id);
    //        if (online != null)
    //    }
    //}
}
