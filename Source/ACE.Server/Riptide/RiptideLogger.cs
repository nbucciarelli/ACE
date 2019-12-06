using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACE.Database.Models.Shard;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Network;
using ACE.Server.Network.GameMessages;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Riptide.Managers;
using ACE.Server.WorldObjects;
using ACE.Server.WorldObjects.Managers;
using log4net;

namespace ACE.Server.Riptide
{
    public static class RiptideLogger
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Killshot(DamageHistoryInfo killer, Player victim, DamageHistoryInfo finisher) {
            // called on Player Death.
            try
            {
                var killer_id = killer.Guid.Full;
                var finisher_id = finisher.Guid.Full;
                var victim_id = victim.Guid.Full;
                log.Info($"[KILLSHOT.V1] Killer.Id: {killer_id}, Victim.Id: {victim_id}, Finisher.Id: {finisher_id}, Killer.Name: {killer.Name}, Victim.Name: {victim.Name}, Finisher.Name: {finisher.Name}");
            } catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void PlayerLogout(Player player)
        {
            // Track when a toon leaves the game.
            try
            {
                log.Info($"[LOGOUT.V1] Player.Name: {player.Character.Name}, Player.Id: {player.Character.Id}");
            } catch (Exception e) {
                log.Error(e);
            }
        }

        public static void PlayerLogin(Character character)
        {
            // Track when a toon joins the game.
            try
            {
                log.Info($"[LOGIN.V1] Player.Name: {character.Name}, Player.Id: {character.Id}");
            } catch (Exception e)
            {
                log.Error(e);
            }
        }

        //// TODO: this only seems to fire on "Move" type emotes.
        //public static void OnExecuteEmote(EmoteManager em, BiotaPropertiesEmote emoteSet, BiotaPropertiesEmoteAction emote, WorldObject targetObject = null)
        //{
        //    // this logic is meant to shadow EmoteManager.ExecuteEmote()
        //    return;
        //    var player = targetObject as Player;
        //    var creature = em.WorldObject as Creature;
        //    var targetCreature = targetObject as Creature;
        //    //log.Info($"{em.WorldObject.WeenieClassId} - {emote.Type}");
        //    //log.Info($"{emote.Type}");

        //    switch ((EmoteType)emote.Type)
        //    {
        //        case EmoteType.DirectBroadcast:
        //            if (player != null)
        //            {
        //                LogChat(emote, em.WorldObject, targetObject);
        //                //LogGameMessageSystemChatBroadcast("DirectBroadcast", text);
        //            }

        //            break;

        //        case EmoteType.FellowBroadcast:
        //        case EmoteType.TellFellow:
        //            if (player != null)
        //            {
        //                var fellowship = player.Fellowship;
        //                if (fellowship == null)
        //                {
        //                    LogChat(emote, em.WorldObject, player);
        //                }
        //                else
        //                {
        //                    var fellowshipMembers = fellowship.GetFellowshipMembers();

        //                    foreach (var fellow in fellowshipMembers.Values)
        //                    {
        //                        LogChat(emote, em.WorldObject, fellow);
        //                    }
        //                }
        //            }
        //            break;

        //        case EmoteType.Say:
        //            LogChat(emote, em.WorldObject, null);
        //            break;

        //        case EmoteType.Tell:

        //            if (player != null)
        //            {
        //                LogChat(emote, em.WorldObject, player);
        //            }
        //            break;

        //        case EmoteType.TextDirect:

        //            if (player != null)
        //            {
        //                LogChat(emote, em.WorldObject, player);
        //            }
        //            break;
        //    }
        //}

        //private static void LogChat(BiotaPropertiesEmoteAction emote, WorldObject source, WorldObject targetObject=null)
        //{
        //    var message = emote.Message;
        //    var _emoteType = (EmoteType)emote.Type;
        //    var emoteType = "";
        //    string sourceId = (source == null) ? "" : $"{source.Guid.Full}";
        //    string sourceName = (source == null) ? "" : $"{source.Name}";
        //    string targetId = (targetObject == null) ? "" : $"{targetObject.Guid.Full}";
        //    string targetName = (targetObject == null) ? "" : $"{targetObject.Name}";

        //    switch ((EmoteType)emote.Type)
        //    {
        //        case EmoteType.DirectBroadcast:
        //            emoteType = "DirectBroadcast";
        //            break;
        //        case EmoteType.FellowBroadcast:
        //            emoteType = "FellowBroadcast";
        //            break;
        //        case EmoteType.TellFellow:
        //            emoteType = "TellFellow";
        //            break;
        //        case EmoteType.Say:
        //            emoteType = "Say";
        //            break;
        //        case EmoteType.Tell:
        //            emoteType = "Tell";
        //            break;
        //        case EmoteType.TextDirect:
        //            emoteType = "TextDirect";
        //            break;
        //    }
        //    var sb = new StringBuilder();
        //    sb.Append($"[CHAT.V1] emoteType: {emoteType}");
        //    switch ((EmoteType)emote.Type)
        //    {
        //        case EmoteType.Say:
        //            var playersInRange = RiptideManager.Players.GetPlayersInRange(source, WorldObject.LocalBroadcastRange).ToList();
        //            sb.Append($", playersInRange: {playersInRange.Count()}");
        //            break;
        //    }
        //    sb.Append($", sourceId: {sourceId}, sourceName: {sourceName}, targetId: {targetId}, targetName: {targetName}, message: {message}");
        //    log.Info(sb.ToString());
        //}
    }
}
