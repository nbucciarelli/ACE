using System;
using ACE.Server.Network;
using ACE.Server.Network.GameAction;
using log4net;
using ACE.Common.Extensions;
using System.IO;
using ACE.Server.WorldObjects;
using System.Linq;
using ACE.Server.Managers;
//using ACE.Entity.Enum;
//using ACE.Server.Command;
//using ACE.Server.Network.GameMessages.Messages;

namespace ACE.Server.Riptide.Managers
{
    public interface IGameActionManager
    {
        void Handle(ClientMessage message, Session session);
    }

    internal class GameActionManager : IGameActionManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal GameActionManager()
        {
        }

        public void Handle(ClientMessage clientMessage, Session session)
        {
            //log.Info($"GameMessageType: {name}, Session: ..., Message: ...");
            
            uint sequence = clientMessage.Payload.ReadUInt32();
            uint opcode = clientMessage.Payload.ReadUInt32();
            var opCode = (GameActionType)opcode;

            switch (opCode)
            {
                case GameActionType.Talk:
                    OnTalk(opCode, clientMessage, session);
                    break;
                case GameActionType.TalkDirect:
                    OnTalkDirect(opCode, clientMessage, session);
                    break;
                case GameActionType.Tell:
                    OnTell(opCode, clientMessage, session);
                    break;
            }
            return;
        }

        private string Name(GameActionType opCode)
        {
            return Enum.GetName(typeof(GameActionType), opCode);
        }

        private void OnTalk(GameActionType opCode, ClientMessage clientMessage, Session session)
        {
            var playerId = session.Player.Guid.Full;
            var playerName = session.Player.Name;
            var message = clientMessage.Payload.ReadString16L();
            var inRange = RiptideManager.Players.GetPlayersInRange(session.Player, WorldObject.LocalBroadcastRange).ToList();
            log.Info($"[GAME_ACTION.V1] type: {Name(opCode)}, len: {clientMessage.Length}, playerId: {playerId}, playerName: {playerName}, inRange: {inRange.Count()}, message: {message}");
        }

        private void OnTalkDirect(GameActionType opCode, ClientMessage clientMessage, Session session)
        {
            var playerId = session.Player.Guid.Full;
            var playerName = session.Player.Name;
            var message = clientMessage.Payload.ReadString16L();
            var targetGuid = clientMessage.Payload.ReadUInt32();

            var creature = session.Player.CurrentLandblock?.GetObject(targetGuid) as Creature;
            if (creature == null)
            {
                return;  // Target is offline.
            }

            if (creature is Player targetPlayer)
            {
                log.Info($"[GAME_ACTION.V1] type: {Name(opCode)}, len: {clientMessage.Length}, playerId: {playerId}, playerName: {playerName}, targetGuid: {targetPlayer.Guid.Full}, message: {message}");
            }
        }

        private void OnTell(GameActionType opCode, ClientMessage clientMessage, Session session)
        {
            var playerId = session.Player.Guid.Full;
            var playerName = session.Player.Name;
            var message = clientMessage.Payload.ReadString16L(); // The client seems to do the trimming for us
            var target = clientMessage.Payload.ReadString16L(); // Needs to be trimmed because it may contain white spaces after the name and before the ,

            if (session.Player.IsGagged)
            {
                session.Player.SendGagError();
                return;
            }

            target = target.Trim();
            var targetPlayer = PlayerManager.GetOnlinePlayer(target);

            if (targetPlayer == null)
            {
                return;
            }

            //log.Info($"[GAME_ACTION.V1] type: {Name(opCode)}, len: {clientMessage.Length}, targetGuid: {targetPlayer.Guid.Full}, message: {message}");
            if (session.Player != targetPlayer)
                log.Info($"[GAME_ACTION.V1] type: {Name(opCode)}, len: {clientMessage.Length}, playerId: {playerId}, playerName: {playerName}, targetGuid: {targetPlayer.Guid.Full}, message: {message}");

        }
    }
}
