using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Players7Server.Enums;
using Players7Server.GameLogic;
using Players7Server.Networking;
using System.Linq;
using System.Threading;

using NLog;

#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.

namespace Players7Server
{
    partial class Program
    {
        public static Logger MainLogger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Program.Write(LogMessageType.Config, "Hello master!");
            try
            {
                var watch = Stopwatch.StartNew();
                Console.ForegroundColor = ConsoleColor.Black;
                Settings = Program.LoadSettings();
                LoadInvitesCodes(false);
                WriteInLogFile = Settings["writeInFile"];
                InitializeConsole();


                Server = new Server(Settings["serverPort"], Settings["maxClients"], Parse<AuthMethod>(Settings["authMethod"]), Settings["passKey"]);
                Server.Go();
                watch.Stop();
                Program.Write("Loaded server in " + watch.Elapsed.TotalSeconds.ToString("F2") + " seconds", "Trace");
                Program.Write(LogMessageType.Config, "{0} commanding server {1}", Settings["serverOwner"], Settings["serverName"]);

                VariousJobTimer.Elapsed += VariousJobTimer_Elapsed;
                VariousJobTimer.Start();


                ConsoleActions();
            }
            catch (Exception e)
            {
                MainLogger.Fatal(e.Message);
                Console.WriteLine(e.Message);
            }
        }

        static void ConsoleActions() {
            string line = "";
            while ((line = Console.ReadLine()) != "csn")
            {
                try
                {
                    if (line == "")
                    { continue; }
                    if (line.StartsWith("sp|"))
                    {
                        string[] prms = line.Split('|');
                        Client c; int id;
                        if (int.TryParse(prms[1], out id))
                            if (Server.Connections.TryGetValue(id, out c))
                            {
                                c.Send(Packet.CreatePacket(prms.Skip(2).ToArray()));
                                Program.Write(LogMessageType.OK, "Sent!");
                            }
                    }
                    else if (line == "get-time")
                    {
                        Program.Write(LogMessageType.OK, "");
                    }
                    if (line == "clients")
                    {
                        int uol = 0;
                        foreach (var pair in Server.Connections)
                        {
                            Console.WriteLine("{0}\t\t-\t\t{1}", pair.Key.ToString().PadRight(20, ' '), pair.Value.Username.PadLeft(10, ' '));
                            uol++;
                        }
                        Console.WriteLine("\n\t>>  {0} users online", uol);
                    }
                    else if (line.StartsWith("settings"))
                    {
                        if (line.Length == 8)
                        {
                            Console.WriteLine("\n>>");
                            foreach (var pair in Settings)
                                Console.WriteLine("\t{0}-{1}", pair.Key.PadRight(23, ' '), pair.Value.ToString().PadLeft(20, ' '));
                        }
                        else
                        {
                            string spec = line.Substring("settings".Length + 1);
                            dynamic result;
                            if (Settings.TryGetValue(spec, out result))
                            {
                                Console.WriteLine(">>  {0}\t-\t{1}", spec, result);
                            }
                            else
                            {
                                Console.WriteLine("Not found... use \'e\' command for adding");
                            }
                        }
                    }
                    else if (line == "cls" || line == "clear")
                        Console.Clear();
                    else if (line.Length > 2)
                    {
                        if (line[1] == ' ')
                        {
                            if (line[0] == 'b')
                            {
                                string m = line.Substring(2);
                                // buffer max
                                if (m.Length < 1000)
                                {
                                    if (line.EndsWith("-last") && !String.IsNullOrWhiteSpace(LastBroadcastedMsg))
                                        Server.Broadcast(LastBroadcastedMsg);
                                    else
                                    {
                                        Server.Broadcast(m);
                                        LastBroadcastedMsg = m;
                                    }
                                }
                                else
                                {
                                    Write(LogMessageType.Error, "Message is too long! Can't be longer than 1000 characters. Message copied to clipboard.");
                                    System.Windows.Forms.Clipboard.SetText(m);
                                }
                            }
                            else if (line[0] == 'a')
                            {
                                int[] uids = new String(line.Substring(2).TakeWhile(c => c != ' ' && c != '^').ToArray()).Split(',').Select(s => int.Parse(s)).ToArray();
                                Server.AdminMessage(line.Substring(line.IndexOf('^') + 1), uids);
                            }
                            else if (line[0] == 'k')
                            {
                                string[] params_ = line.Split(' ');

                                Client client = Server.Connections[params_[1].ToInt()];
                                string endpoint = client.Socket.RemoteEndPoint.ToString().Split(':')[0];

                                Kick(client, endpoint);
                                RemoveBlacklist(endpoint, (params_.Length == 2) ? Settings["defaultBanTime"] : params_[2].ToInt());
                            }
                            else if (line[0] == 'e')
                            {
                                string[] data = line.Substring(2).Split('=');
                                lock (Settings)
                                {
                                    if (Settings.ContainsKey(data[0]))
                                    {
                                        try
                                        {
                                            dynamic oldVal = Settings[data[0]];
                                            //if (oldVal.GetType() == data[1].GetType())
                                            Console.WriteLine(">>\tChanged: {0}\t=\t{1}", data[0], Settings[data[0]] = data[1]);
                                            //else
                                            //    throw new ArgumentException("Wrong data type for " + data[0]);
                                            if (firstEditOfSettings)
                                            {
                                                Write("Changing settings is unsafe: no type check!", "Warning", ConsoleColor.Red);
                                                firstEditOfSettings = false;
                                            }
                                        }
                                        catch (IndexOutOfRangeException)
                                        {
                                            Console.WriteLine("Wrong syntax for E (edit) command");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No such key... add in session memory? 1 = yes");
                                        if (Console.ReadLine() == "1")
                                        {
                                            lock (Settings)
                                            {
                                                AddSmart(data[0], data[1]);
                                                Console.WriteLine("Added {0} = {1}", data[0], data[1]);
                                            }
                                        }
                                        else
                                            Console.WriteLine("Dismissed");
                                    }
                                }
                            }
                        }
                        if (line.StartsWith("udata"))
                        {
                            string input = line.Substring(6);

                            int uid; Client val;
                            if (int.TryParse(input, out uid)) // in: "UID", out: uid (int), OUTPUT: Username
                            {
                                if (Server.Connections.TryGetValue(uid, out val))
                                    Write("Username: " + val.Username, "Query");
                                else
                                    Write("No client with UID: " + uid);
                            }
                            else
                            {
                                val = Server.Connections.ValuesWhere(c => c.Username == input).FirstOrDefault();
                                if (val != null)
                                    Write("UID: " + val.UserID, "Query");
                                else
                                    Write("No client with Username: " + input);
                            }
                        }
                        else if (line.StartsWith("restart"))
                        {
                            Program.Write("Restarting does not work on macOS platform -- todo");
                            int delay = 0;
                            if (line.Length > 7)
                            {
                                delay = int.Parse(line.Substring(8));
                            }
                            Write(String.Format("Restarting in {0}...", delay), "Application", ConsoleColor.Blue);
                            Thread.Sleep(delay);
                            Process.Start("Players7Server.exe", "-wait 1000");
                            return;
                        }
                        else if (line == "add ic")
                        {
                            Console.Write("Expecting invite code: ");
                            string code = Console.ReadLine();
                            lock (InviteCodes)
                                InviteCodes.Add(code);
                            Write(LogMessageType.Config, "Added invite code " + code);
                        }
                        else if (line.StartsWith("pop ic"))
                        {
                            string code;
                            lock (InviteCodes)
                            {
                                if (InviteCodes.Count != 0)
                                {
                                    code = InviteCodes.First();
                                }
                                else
                                {
                                    code = Helper.GenerateRandomString(6, true);
                                    InviteCodes.Add(code);
                                }
                            }
                            Console.Write(code);

                            if ((line.Length > 7 && line.Substring(7) == "-nc"))
                            {
                                Console.WriteLine();
                            }
                            else
                            {
                                Thread.Sleep(100);
                                System.Windows.Forms.Clipboard.SetText(code);
                                Console.WriteLine(" -> copied");
                            }
                        }
                        else if (line.StartsWith("del ic"))
                        {
                            if (line == "del ic -all")
                            {
                                // delete file
                                lock (InviteCodes)
                                {
                                    InviteCodes.Clear();
                                    File.Delete("invites.txt"); // no cross threading thanks to locking invitecodes everywhere
                                    Program.Write(LogMessageType.OK, "");
                                }
                            }
                            else
                            {
                                line = line.Substring(7);
                                lock (InviteCodes)
                                {
                                    InviteCodes.Remove(line);
                                    Program.Write(LogMessageType.OK, "");
                                }
                            }
                        }
                        else if (line == "openf")
                        {
                            Process.Start("explorer", Environment.CurrentDirectory);
                        }
                        else if (line == "help")
                        {
                            Console.WriteLine(Players7Server.Properties.Resources.ConsoleCommandGuide);
                        }
                        else if (line == "time")
                        {
                            DateTime now = DateTime.Now;
                            TimeSpan running = (DateTime.Now - Program.TimeServerStarted);
                            string runningStr = (running.TotalMinutes < 75) ? running.TotalMinutes.ToString("F1") + " min" : running.TotalHours.ToString("F2") + " h";

                            Program.Write(LogMessageType.ServerMessage,
                                "Time is {0}, process running for {1} ",
                                now.ToLongTimeString(), runningStr);
                        }
                        else if (line.StartsWith("vote"))
                        {

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        private static void RemoveBlacklist(string endpoint, dynamic dynamic)
        {
            throw new NotImplementedException();
        }

        private static void Kick(Client client, string endpoint)
        {
            throw new NotImplementedException();
        }

        public static Server Server;
        const int VARIOUS_JOB_TIMER_TICK = 7500;
        static System.Timers.Timer VariousJobTimer = new System.Timers.Timer(VARIOUS_JOB_TIMER_TICK);

        public static List<string> InviteCodes = new List<string>(500);
        public static readonly string[] ReservedNames = new[] { "admin", "system", "server", "TODEA" };
        static string LastBroadcastedMsg = "";

        public static Dictionary<string, Game> Games = new Dictionary<string, Game>(7);

		static void InitializeConsole()
		{
            Console.Title = Settings["serverName"];

            ConsoleColor bckCol = /*Console.BackgroundColor =*/ Parse<ConsoleColor>(Settings["consoleBackColor"]);

            Console.ForegroundColor = bckCol == ConsoleColor.Black ? ConsoleColor.Blue : ConsoleColor.DarkCyan;

#if mac
			Console.SetWindowSize(105, 35);
			Console.SetBufferSize(105, 1500);
#endif

			//Console.Clear();
			Program.Write(LogMessageType.Config, "Settings loaded");
		}



#region Write

        static bool WriteInLogFile;
        const ConsoleColor DefaultColor = ConsoleColor.Black;
        static StreamWriter writer = new StreamWriter("logs.txt", true);
        static bool firstEditOfSettings=true;
        static DateTime TimeServerStarted = DateTime.Now;


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
            MainLogger.Log(LogLevel.Info, "[{0}] - {1}", head, msg);
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
        private static void LoadInvitesCodes(bool force)
        {
            Program.InviteCodes = new List<string>(Settings["maxClients"]);
            if (force == true || Parse<AuthMethod>(Settings["authMethod"]) == AuthMethod.InviteCode)
            {
                try
                {
                    using (StreamReader reader = new StreamReader("invites.txt"))
                    {
                        string line;
                        while (string.IsNullOrWhiteSpace(line = reader.ReadLine()) == false)
                        {
                            Program.InviteCodes.Add(line);
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    Write(LogMessageType.Config, "Warning: no invite code file (create invites.txt)");
                    Write(LogMessageType.Config, "Create new file? <yes/no>\tDefault: <def>");
                    string ans = Console.ReadLine().ToLower();
                    if (ans == "yes" || ans == "def")
                    {
                        string[] args = new string[5];
                        if (ans == "def")
                        {
                            args = new[] { "100", "5", "6", "no", "invites.txt" };
                        }
                        else
                        {
                            Console.Write("Number of codes: ");
                            args[0] = Console.ReadLine();
                            Console.Write("Grouping number: ");
                            args[1] = Console.ReadLine();
                            Console.Write("Chars/group: ");
                            args[2] = Console.ReadLine();
                            Console.Write("Open file after? <yes/no>:");
                            args[3] = Console.ReadLine();
                            args[4] = "invites.txt";
                        }
                        if (!File.Exists("icgen.exe"))
                        {
                            int len = global::Players7Server.Properties.Resources.icgen.Length;
                            using (var stream = File.Create("icgen.exe", len))
                            {
                                stream.Write(global::Players7Server.Properties.Resources.icgen, 0, len);
                            }
                        }
                        using (var generatorProcess = Process.Start("icgen.exe", String.Join(" ", args)))
                        {
                            generatorProcess.WaitForExit();
                            if (generatorProcess.ExitCode == -2)
                            {
                                Write(LogMessageType.Config, "Could not generate invite codes, reason unknown.");
                            }
                            else
                            {
                                switch (generatorProcess.ExitCode)
                                {
                                    case -1:
                                        Write(LogMessageType.Config, "Wrong format for parameters, retrying process"); break;
                                    case 0:
                                        Write(LogMessageType.Config, "{0} codes generated! Restarting the server...", args[0]); break;
                                    default:
                                        Write(
                                            "Unknown exit code for icgen.exe <" + generatorProcess.ExitCode + ">",
                                            "ERROR", ConsoleColor.DarkRed); break;
                                }

                                LoadInvitesCodes(force);
                            }
                        }
                    }
                }
            }
        }

        static int timerTickerCounter = 0;
        static void VariousJobTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerTickerCounter++;
            lock (Settings)
            {
                WriteInLogFile = Settings["writeInFile"];
                ConsoleColor temp = Parse<ConsoleColor>(Settings["consoleBackColor"]);
                if (Console.BackgroundColor != temp)
                {
                    Console.BackgroundColor = temp;
                    Console.Clear();
                }
                Server.AuthMethod = Parse<AuthMethod>(Settings["authMethod"]);
                Server.Password = Settings["passKey"];
                try
                {
                    Server.LogPackets = (Settings["logPackets"]);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    Settings["logPackets"] = true;
                }
                catch (NullReferenceException)
                {
                    throw;
                    // ce dracu
                    // server ii null
                }
                // todo add more maybe
            }
            if (Server.AuthMethod == AuthMethod.InviteCode)
            {
                lock (InviteCodes)
                {
                    if (InviteCodes.Count != 0)
                        File.WriteAllLines("invites.txt", InviteCodes);
                    else
                        File.Delete("invites.txt");
                }
            }
            if (timerTickerCounter % 4 == 0)
            {
                if (Settings["rewriteSettings"] == true)
                {
                    lock (Settings)
                    {
                        using (var writer = new StreamWriter("server.ini"))
                        {
                            foreach (var pair in Settings)
                            {
                                writer.WriteLine("{0}={1}", pair.Key, pair.Value.ToString());
                            }
                        }
                    }
                }
            }
        }

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
#pragma warning restore RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
#pragma warning restore RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
