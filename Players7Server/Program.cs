using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Players7Server.Enums;
using Players7Server.GameLogic;
using Players7Server.Networking;

namespace Players7Server
{
    partial class Program
    {

        public static void Main(string[] args)
        {
            /*
            CardPack pack = new CardPack();
            pack.Initialize();
            pack.RemoveCard(new Card() { Type = CardType.Romb, Value = CardValue.Eight });
            foreach (var card in pack)
            {
                Console.WriteLine(card.ToString());
            }
*/

            Program.Write(LogMessageType.Error, "Hello master!");
#if RELEASE
            Program.Write(LogMessageType.Auth, "Password?");
            if (Console.ReadLine() != "no") {
                return;
            }
#endif

            try
            {
                var watch = Stopwatch.StartNew();
                Settings = Program.LoadSettings();

                WriteInLogFile = Settings["writeInFile"];
                InitializeConsole();

                Server = new Server(Settings["serverPort"], Settings["maxClients"], Parse<AuthMethod>(Settings["authMethod"]), Settings["passKey"]);
				Server.Go();
                watch.Stop();
                Program.Write("Loaded server in " + watch.Elapsed.TotalSeconds.ToString("F2") + " seconds", "Trace");

				string line = "";
                while ((line = Console.ReadLine()) != "csn")
                {
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }

        public static Server Server;

        public static List<string> InviteCodes = new List<string>(500);
        public static readonly string[] ReservedNames = new[] { "admin", "system", "server", "TODEA" };

        public static Dictionary<string, Game> Games = new Dictionary<string, Game>(7);

		static void InitializeConsole()
		{
			Console.Title = "Chat Server";
            //Console.WriteLine(Console.BackgroundColor);

            ConsoleColor bckCol = /*Console.BackgroundColor =*/ Parse<ConsoleColor>(Settings["consoleBackColor"]);

            Console.ForegroundColor = bckCol == ConsoleColor.Black ? ConsoleColor.Cyan : ConsoleColor.DarkCyan;

#if mac
			Console.SetWindowSize(105, 35);
			Console.SetBufferSize(105, 1500);
#endif

			//Console.Clear();
			Program.Write(LogMessageType.Config, "Settings loaded");
		}



#region Write

        static bool WriteInLogFile;
        const ConsoleColor DefaultColor = ConsoleColor.DarkGray;
        static StreamWriter writer = new StreamWriter("logs.txt", true);
        public static void Write(string msg, string header = "", ConsoleColor color = DefaultColor)
        {
            string msg_ = (!string.IsNullOrWhiteSpace(header) ? (string.Concat(DateTime.Now.ToLongTimeString(), " >>> [", header, "]  ", msg)) : (string.Concat(">>>", "   ", msg)));
            var _col = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(msg_);
            Console.ForegroundColor = _col;

            if (WriteInLogFile)
            {
                writer.WriteLine(msg_);
                writer.Flush();
            }
        }
        public static void Write(LogMessageType type, string msg, params object[] obj)
        {
            string head = GetEnum(type);
            msg = string.Format(msg, obj);
            switch (type)
            {
                case LogMessageType.Config:
                    Write(msg, head, ConsoleColor.Red);
                    break;

                case LogMessageType.Network:
                    Write(msg, head, ConsoleColor.Green);
                    break;

                case LogMessageType.Chat:
                    Write(msg, head, ConsoleColor.DarkCyan);
                    break;

                case LogMessageType.Auth:
                    Write(msg, head);
                    break;

                case LogMessageType.UserEvent:
                    Write(msg, head, ConsoleColor.DarkYellow);
                    break;

                case LogMessageType.Packet:
                    Write(msg, head);
                    break;

                case LogMessageType.ReportFromUser:
                    Write(msg, "REPORT", ConsoleColor.DarkRed);
                    break;

                case LogMessageType.Error:
                    Write(msg, head, ConsoleColor.Red);
                    break;

                case LogMessageType.Warning:
                    Write(msg, head, ConsoleColor.DarkYellow);
                    break;

                case LogMessageType.OK:
                    Write("OK", "System");
                    break;

                default:
                    Write(msg);
                    break;
            }
        }

#endregion

#region helpers

        static string GetEnum<T>(T _enum) where T : struct
        {
            return Enum.GetName(typeof(T), _enum);
        }

        static T Parse<T>(string from) where T : struct // == where T : Enum
        {
            T auth;
            if (Enum.TryParse<T>(from, true, out auth))
                return auth;
            else return default(T);
        }

#endregion
    }
}
