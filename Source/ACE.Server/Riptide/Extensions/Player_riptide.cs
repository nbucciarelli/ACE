using System;
using ACE.Common;
using ACE.Database;
//using ACE.Database.Models.Auth;
//using ACE.Database.Models.Shard;
//using ACE.Database.Models.World;
//using ACE.DatLoader;
//using ACE.DatLoader.FileTypes;
//using ACE.Entity;
//using ACE.Entity.Enum;
//using ACE.Entity.Enum.Properties;
//using ACE.Server.Entity;
using ACE.Database.Entity;
using ACE.Server.Entity.Actions;
//using ACE.Server.Managers;
//using ACE.Server.Network;
//using ACE.Server.Network.GameEvent.Events;
//using ACE.Server.Network.GameMessages.Messages;
//using ACE.Server.Network.Structure;
//using ACE.Server.Physics.Animation;
//using ACE.Server.Physics.Common;
//using ACE.Server.WorldObjects.Managers;

namespace ACE.Server.WorldObjects
{
    public delegate void Next();
    partial class Player
    {
        public void RefreshInventoryServerSide(Next next)
        {
            log.Info($"RefreshInventoryServerSide({Name})");
            var character = Character;
            var start = DateTime.UtcNow;
            DatabaseManager.Shard.GetPossessedBiotasInParallel(character.Id, (biotas) =>
            {
                log.Info($"GetPossessedBiotasInParallel for {character.Name} took {(DateTime.UtcNow - start).TotalMilliseconds:N0} ms");
                //DoPseudoLogin(biotas);
                actionQueue.EnqueueAction(new ActionEventDelegate(() => DoPseudoLogin(biotas, next)));
                //actionQueue.EnqueueAction(new ActionEventDelegate(() => DoPlayerEnterWorld(session, character, offlinePlayer.Biota, biotas)));
            });
        }

        private void DoPseudoLogin(PossessedBiotas b, Next next)
        {
            log.Info($"DoPseudoLogin({Name})");
            SetEphemeralValues();

            //PossessedBiotas b = GetPossessedBiotas();
            SortBiotasIntoInventory(b.Inventory);
            AddBiotasToEquippedObjects(b.WieldedItems);

            UpdateCoinValue(false);

            next();
        }
    }
}