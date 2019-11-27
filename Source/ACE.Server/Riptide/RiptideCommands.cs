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
using ACE.Server.Riptide;
using ACE.Server.WorldObjects;
using ACE.Server.WorldObjects.Entity;


using Position = ACE.Entity.Position;
using Spell = ACE.Server.Entity.Spell;

namespace ACE.Server.Command.Handlers
{
    public static class RiptideCommands
    {
        [CommandHandler("rt-showprops", AccessLevel.Admin, CommandHandlerFlag.None, 0, "Shows Riptide features.")]
        public static void HandleRiptideShowProperties(Session session, params string[] paramters)
        {
            CommandHandlerHelper.WriteOutputInfo(session, CustomPropertiesManager.ListProperties());
        }

        [CommandHandler("rt-modifybool", AccessLevel.Admin, CommandHandlerFlag.None, 2, "Modifies a Riptide property that is a bool", "modifybool (string) (bool)")]
        public static void HandleModifyRiptideBoolProperty(Session session, params string[] paramters)
        {
            try
            {
                var boolVal = bool.Parse(paramters[1]);
                if (CustomPropertiesManager.ModifyBool(paramters[0], boolVal))
                    CommandHandlerHelper.WriteOutputInfo(session, "Bool property successfully updated!");
                else
                    CommandHandlerHelper.WriteOutputInfo(session, "Unknown bool property was not updated. Type showprops for a list of properties.");
            }
            catch (Exception)
            {
                CommandHandlerHelper.WriteOutputInfo(session, "Please input a valid bool", ChatMessageType.Help);
            }
        }

        [CommandHandler("rt-fetchbool", AccessLevel.Admin, CommandHandlerFlag.None, 1, "Fetches a Riptide property that is a bool", "fetchbool (string)")]
        public static void HandleFetchRiptideBoolProperty(Session session, params string[] paramters)
        {
            var boolVal = CustomPropertiesManager.GetBool(paramters[0]);
            CommandHandlerHelper.WriteOutputInfo(session, $"{paramters[0]} - {boolVal.Description ?? "No Description"}: {boolVal.Item}");
        }

        [CommandHandler("rt-modifydouble", AccessLevel.Admin, CommandHandlerFlag.None, 2, "Modifies a Riptide property that is a float", "rt-modifydouble (string) (float)")]
        public static void HandleModifyRiptideFloatProperty(Session session, params string[] paramters)
        {
            try
            {
                var doubleVal = double.Parse(paramters[1]);
                if (CustomPropertiesManager.ModifyDouble(paramters[0], doubleVal))
                    CommandHandlerHelper.WriteOutputInfo(session, "Float property successfully updated!");
                else
                    CommandHandlerHelper.WriteOutputInfo(session, "Unknown float property was not updated. Type rt-showprops for a list of properties.");
            }
            catch (Exception)
            {
                CommandHandlerHelper.WriteOutputInfo(session, "Please input a valid float", ChatMessageType.Help);
            }
        }

        [CommandHandler("rt-fetchdouble", AccessLevel.Admin, CommandHandlerFlag.None, 1, "Fetches a Riptide property that is a float", "fetchdouble (string)")]
        public static void HandleFetchRiptideFloatProperty(Session session, params string[] paramters)
        {
            var floatVal = CustomPropertiesManager.GetDouble(paramters[0]);
            CommandHandlerHelper.WriteOutputInfo(session, $"{paramters[0]} - {floatVal.Description ?? "No Description"}: {floatVal.Item}");
        }

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

        //[CommandHandler("rt-mule", AccessLevel.Admin, CommandHandlerFlag.None, 3, "Mule an item.")]
        //public static void HandleRiptideMuleItem(Session session, params string[] parameters)
        //{
        //    //session.UpdateCharacters()
        //    try
        //    {
        //        Biota item = ResolveBiota(parameters[0]);  // item to transfer.
        //        Character source = ResolveCharacter(parameters[1]);  // current owner.
        //        Character target = ResolveCharacter(parameters[2]);  // new owner.
        //        TransferItemWithNetworking(item, source, target);
        //    } catch (Exception e)
        //    {
        //        CommandHandlerHelper.WriteOutputInfo(session, e.Message, ChatMessageType.Help);
        //    }
        //}

        private static Biota ResolveBiota(string param)
        {
            uint id = uint.Parse(param);
            Biota biota = DatabaseManager.Shard.GetBiota(id);
            if (biota == null)
            {
                throw new Exception($"Biota not found: {param}");
            } else
            {
                return biota;
            }
        }

        private static Character ResolveCharacter(string param)
        {
            uint id = uint.Parse(param);
            Character character = DatabaseManager.Shard.GetCharacterByGuid(id);
            if (character == null)
            {
                throw new Exception($"Character not found: {param}");
            } else
            {
                return character;
            }
        }
    }
}

