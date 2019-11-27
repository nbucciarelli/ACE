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
                var killer_id = killer.Guid;
                var finisher_id = finisher.Guid;
                var victim_id = victim.Guid;
                log.Debug($"[KILLSHOT.V1] Killer.Guid: {killer_id}, Victim.Guid: {victim_id}, Finisher.Guid: {finisher_id}, Killer.Name: {killer.Name}, Victim.Name: {victim.Name}, Finisher.Name: {finisher.Name}");
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
                log.Debug($"[LOGOUT.V1] Account.Name: {player.Account.AccountName}, Player.Name: {player.Character.Name}");
            } catch (Exception e) {
                log.Error(e);
            }
        }

        public static void PlayerLogin(Player player, Character character)
        {
            // Track when a toon joins the game.
            try
            {
                log.Debug($"[LOGIN.V1] Account.Name: {player.Account.AccountName} Player.Name: {character.Name}");
            } catch (Exception e)
            {
                log.Error(e);
            }
        }
    }
}
