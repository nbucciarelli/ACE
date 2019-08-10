using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using System.Text;
using log4net;

using ACE.Common;
using ACE.Database;
using ACE.Database.Models.World;
using ACE.Database.Models.Shard;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Factories;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Physics.Common;
using ACE.Server.Physics.Entity;
using ACE.Server.WorldObjects;
using ACE.Server.WorldObjects.Entity;


using Position = ACE.Entity.Position;
using Spell = ACE.Server.Entity.Spell;

namespace ACE.Server.Command.Handlers
{
    public static class RiptideCommands
    {
        [CommandHandler("tt", AccessLevel.Developer, CommandHandlerFlag.None, 0, "Describes your target.")]
        public static void HandleTargetTest(Session session, params string[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"MeleeTarget: {session.Player.MeleeTarget}\n");
            sb.Append($"MissileTarget: {session.Player.MissileTarget}\n");
            sb.Append($"CombatTarget: {session.Player.CombatTarget}\n");
            sb.Append($"ProjectileTarget: {session.Player.ProjectileTarget}\n");
            sb.Append($"CurrentAppraisalTarget: {session.Player.CurrentAppraisalTarget}\n");

            session.Player.DoWorldBroadcast($"{sb}", ChatMessageType.WorldBroadcast);
        }
    }
}

