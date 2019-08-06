//using System;
using System;
using System.Collections.Generic;

using ACE.Entity.Enum;
using ACE.Server.Network;
//using ACE.Server.Network.GameMessages.Messages;

namespace ACE.Server.Command.Handlers
{
    public class VerificationCode
    {
        public int Code;
        private DateTime IssuedAt;
        private DateTime Expires;
        public VerificationCode()
        {
            Code = ThreadSafeRandom.Next(1111, 9999);
            IssuedAt = DateTime.UtcNow;
            Expires = DateTime.UtcNow + new TimeSpan(0, 10, 0); // 10 minutes.
        }
        public bool IsExpired() { return DateTime.UtcNow > Expires; }
    }

    public static class DiscordCommands
    {
        public static Dictionary<uint, VerificationCode> RegistrationCodes = new Dictionary<uint, VerificationCode>();

        [CommandHandler("register", AccessLevel.Player, CommandHandlerFlag.None, 0, "Gets a registration code for the Undertaker.")]
        public static void HandleRegister(Session session, params string[] parameters)
        {
            if (session != null)
            {
                // this command only works if run in-game.
                //int rando = ThreadSafeRandom.Next(1111, 9999);
                uint accountId = session.Player.Account.AccountId;
                RegistrationCodes.Add(accountId, new VerificationCode());
                CommandHandlerHelper.WriteOutputInfo(session, $"Paste the following into Discord:");
                CommandHandlerHelper.WriteOutputInfo(session, $"!verify {accountId} {RegistrationCodes[accountId].Code}");
            }
        }

        [CommandHandler("verify", AccessLevel.Player, CommandHandlerFlag.None, 0, "Used by Undertaker to verify a registration code.", "(accountId) (code)")]
        public static void HandleVerify(Session session, params string[] parameters)
        {
            if (session == null)
            {
                // this command only works if run out-of-game. (WebService CLI.)
                if (parameters != null && parameters.Length == 2)
                {
                    uint accountId = uint.Parse(parameters[0]);
                    int actualCode = int.Parse(parameters[1]);
                    if (RegistrationCodes.ContainsKey(accountId))
                    {
                        VerificationCode expected = RegistrationCodes[accountId];
                        //int expectedCode = expected.Code
                        if (expected.IsExpired())
                        {
                            CommandManager.History.OnException($"Your verification code has expired: {actualCode}.\nGo back into Asheron's Call and type /register again.", null);
                            RegistrationCodes.Remove(accountId);
                            return;
                        }
                        if (actualCode == expected.Code)
                        {
                            CommandHandlerHelper.WriteOutputInfo(session, $"1");
                            RegistrationCodes.Remove(accountId);
                            return;
                        } else
                        {
                            CommandManager.History.OnException($"Incorrect verification code: {actualCode}.\nGo back into Asheron's Call and type /register again.", null);
                            RegistrationCodes.Remove(accountId);
                            return;
                        }
                    } else
                    {
                        CommandManager.History.OnException($"No verification is pending for accountId: {accountId}.\nGo back into Asheron's Call and type /register again.", null);
                        return;
                    }
                }
                CommandManager.History.OnException($"Failed to parse command.\nGo back into Asheron's Call and type /register again.", null);
                CommandHandlerHelper.WriteOutputInfo(session, $"0");
                return;
            }
        }

        [CommandHandler("dtell", AccessLevel.Player, CommandHandlerFlag.None, 0, "Lets Undertaker send a /tell message.", "(playername), (message)")]
        public static void HandleTell(Session session, params string[] parameters)
        {
            string playerName;
            string message;

            /// todo : move this to a commandhandlerhelper
            //CommandHandlerHelper.parsePlayerNameParams(parameters) { }
            List<string> temp = new List<string>();
            int i = -1;
            for (int j=0; j<parameters.Length; j++) { if (parameters[i].EndsWith(",")) { i = j; break; } }
            if (i == -1) {
                CommandHandlerHelper.WriteOutputInfo(session, $"Parse Error: Expected a comma after player name.\nUsage: tell <player name>, <your message here>");
                return;
            }
            for (int p=0; p<i; p++) { temp.Add(parameters[p]); }
            temp.Add(parameters[i].Substring(0, parameters[i].Length - 1));

            playerName = string.Join(" ", temp);

            temp.Clear();
            for (int p=i; p<parameters.Length; p++) { temp.Add(parameters[p]); }
            message = string.Join(" ", temp);

            ///
            
        }
        //// acehelp (command)
        //[CommandHandler("acehelp", AccessLevel.Player, CommandHandlerFlag.None, 0, "Displays help.", "(command)")]
        //public static void HandleACEHelp(Session session, params string[] parameters)
        //{
        //    if (parameters?.Length <= 0)
        //    {
        //        if (session != null)
        //        {
        //            var msg = "Note: You may substitute a forward slash (/) for the at symbol (@).\n"
        //                    + "Use @help to get more information about commands supported by the client.\n"
        //                    + "Available help:\n"
        //                    + "@acehelp commands - Lists all commands.\n"
        //                    + "You can also use @acecommands to get a complete list of the supported ACEmulator commands available to you.\n"
        //                    + "To get more information about a specific command, use @acehelp command\n";
        //            session.Network.EnqueueSend(new GameMessageSystemChat(msg, ChatMessageType.Broadcast));
        //        }

        //        return;
        //    }

        //    if (parameters?[0] == "commands") // Mimick @help commands command
        //    {
        //        HandleACECommands(session, parameters);
        //        return;
        //    }

        //    foreach (var command in CommandManager.GetCommandByName(parameters[0]))
        //    {
        //        if (session != null)
        //        {
        //            if (command.Attribute.Flags == CommandHandlerFlag.ConsoleInvoke)
        //                continue;
        //            if (session.AccessLevel < command.Attribute.Access)
        //                continue;

        //            var msg = $"@{command.Attribute.Command} - {command.Attribute.Description}\n"
        //                    + $"Usage: @{command.Attribute.Command} {command.Attribute.Usage}\n";

        //            session.Network.EnqueueSend(new GameMessageSystemChat(msg, ChatMessageType.Broadcast));

        //            return;
        //        }

        //        if (command.Attribute.Flags == CommandHandlerFlag.RequiresWorld)
        //            continue;
        //        Console.WriteLine($"{command.Attribute.Command} - {command.Attribute.Description}");
        //        Console.WriteLine($"Usage: {command.Attribute.Command} {command.Attribute.Usage}");

        //        return;
        //    }

        //    if (session != null)
        //    {
        //        var msg = "Use @acecommands to get a complete list of commands available for you to use.\n"
        //                + "To get more information about a specific command, use @acehelp command\n";

        //        session.Network.EnqueueSend(new GameMessageSystemChat($"Unknown command: {parameters[0]}", ChatMessageType.Help),
        //                                    new Network.GameEvent.Events.GameEventWeenieError(session, WeenieError.ThatIsNotAValidCommand),
        //                                    new GameMessageSystemChat(msg, ChatMessageType.Broadcast));
        //    }
        //    else
        //        Console.WriteLine($"Unknown command: {parameters[0]}");
        //}

        //// acecommands
        //[CommandHandler("acecommands", AccessLevel.Player, CommandHandlerFlag.None, 0, "Lists all commands.")]
        //public static void HandleACECommands(Session session, params string[] parameters)
        //{
        //    List<String> commandList = new List<string>();

        //    var msgHeader = "Note: You may substitute a forward slash (/) for the at symbol (@).\n"
        //                  + "For more information, type @acehelp < command >.\n";

        //    if (session == null)
        //        Console.WriteLine("For more information, type acehelp < command >.");

        //    foreach (var command in CommandManager.GetCommands())
        //    {
        //        if (session != null)
        //        {
        //            if (command.Attribute.Flags == CommandHandlerFlag.ConsoleInvoke) // Skip Console Commands
        //                continue;
        //            if (session.AccessLevel < command.Attribute.Access) // Skip Commands which are higher than your current access level
        //                continue;

        //            commandList.Add(string.Format("@{0} - {1}", command.Attribute.Command, command.Attribute.Description));
        //        }
        //        else
        //        {
        //            if (command.Attribute.Flags == CommandHandlerFlag.RequiresWorld) // Skip Commands that only work in game
        //                continue;

        //            commandList.Add(string.Format("{0} - {1}", command.Attribute.Command, command.Attribute.Description));
        //        }
        //    }

        //    commandList.Sort();

        //    var msg = string.Join("\n", commandList.ToArray());

        //    if (session != null)
        //        session.Network.EnqueueSend(new GameMessageSystemChat(msgHeader + msg, ChatMessageType.Broadcast));
        //    else
        //        Console.WriteLine(msg);
        //}
    }
}
