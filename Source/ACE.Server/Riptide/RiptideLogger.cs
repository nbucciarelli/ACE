using System;
using ACE.Server.Entity;
using ACE.Server.WorldObjects;
using log4net;

namespace ACE.Server.Riptide
{
    public static class RiptideLogger
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Killshot(DamageHistoryInfo killer, Player victim, DamageHistoryInfo finisher) {
            try
            {
                var killer_id = killer.Guid;
                var finisher_id = finisher.Guid;
                var victim_id = victim.Guid;
                log.Debug($"[KILLSHOT] Killer: {killer_id}, Victim: {victim_id}, Finisher: {finisher_id}");
            } catch (Exception e)
            {
                log.Error(e);
            }
        }
    }
}
