using System;
using ACE.Database.Models.Shard;
using ACE.Server.Entity;
using ACE.Server.Network;
using ACE.Server.WorldObjects;
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
    }
}
