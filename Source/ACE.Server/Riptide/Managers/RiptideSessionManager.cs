using System;
using ACE.Database.Models.Auth;
using ACE.Database.Models.Shard;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.Network.Managers;
using ACE.Server.WorldObjects;

namespace ACE.Server.Riptide.Managers
{
    public interface ISessionManager
    {
        Session GetSession(Character character);
        Session GetSession(Account account);
    }

    public class RiptideSessionManager: ISessionManager
    {
        public RiptideSessionManager()
        {
        }

        //public Session GetSession(Character character) {
        //    if (character == null)
        //        return null;
        //    Account account = RiptideManager.Database.GetAccount(character.AccountId);
        //    if (account == null)
        //        return null;
        //    Session session = GetSession(account);
        //    if (session == null)
        //        return null;
        //    if (session.Player.Character.Id != character.Id)
        //        // ignore the session if it's for another character on the same account.
        //        return null;
        //    return session;
        //}

        public Session GetSession(Character character)
        {
            if (character == null)
                return null;
            Player player = PlayerManager.GetOnlinePlayer(character.Id);
            return (player == null) ? null : player.Session;
        }

        public Session GetSession(Account account)
        {
            if (account == null)
                return null;
            return NetworkManager.Find(account.AccountId);
        }
    }
}
