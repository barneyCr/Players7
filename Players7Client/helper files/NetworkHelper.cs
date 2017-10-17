using System;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient
{
    public class NetworkHelper
    {
        static byte[] NameRequiredPacket =          new byte[] { 2, 8, 18, 32 };
        static byte[] FullAuthRequiredPacket =      new byte[] { 2, 32, 8, 18 };
        static byte[] InviteCodeRequiredPacket =    new byte[] { 2, 8, 32, 18 };
        static byte[] ServerIsFullPacket =          new byte[] { 2, 18, 8, 32 };
        static byte[] AccessGrantedPacket =         new byte[] { 2, 32, 18, 8 };
        static byte[] AccessDeniedPacket =          new byte[] { 2, 18, 32, 8 };

        public void Send(string msg, params object[] obj)
        {
            Socket.Send(enc.GetBytes(string.Format(msg, obj)));
        }

        static ASCIIEncoding enc = new ASCIIEncoding();

        public MainForm Form { private get; set; }
        private Thread listenThread;
        public bool Connected { get; private set; }
        public bool Kicked { get; set; }
        public int Port { get; private set; }
        public string IP { get; private set; }
        public Socket Socket { get; set; }
        public string Username, Password;
        public int UID { get; set; }

        static readonly Properties.Settings settings = global::ChatClient.Properties.Settings.Default;
        readonly WriteLogDelegate WriteLog;

        public NetworkHelper(string ip, int port, string username, string password, WriteLogDelegate callbackMethod)
        {
            this.IP = ip;
            this.Port = port;
            this.Username = username;
            this.Password = password;

            this.WriteLog = callbackMethod;

            settings.Username = username;
            settings.PreferredIP = ip;
            settings.Save();
        }

        private void NetworkAction()
        {
            // changed from 512 to 1024 (8th march 2016)
            byte[] buff = new byte[1024];
            int bytes = 0;
            while (Socket.Connected)
            {
                if (!Receive(buff, ref bytes)) { break; }

                Packet packet;
                string[] packetArray = enc.GetString(buff, 0, bytes).Split('\n');

                foreach (var pstr in packetArray)
                { 
                    using (packet = new Packet(pstr))
                    {
                        HandlePacket(packet);
                    }
                }
            }
            if (this.ConnectionLost != null && Kicked == false)
                this.ConnectionLost();
        }

        private async void HandlePacket(Packet p)
        {
            p.Seek(+1);

            if (p.Header == "")
            {
                return;
            }
            if (p.Header == "34") // msg
            {
                int senderUID = p.ReadInt() ^ 0x121;
                Form.WriteInChat(Player.All[senderUID].Username, senderUID, p.ReadString());
            }
            else if (p.Header == "1") // INIT
            {
                int myUid = this.UID = p.ReadInt();
                string name = this.Username = p.ReadString();
                Player.All.Add(myUid, new Player(myUid, name));
            }
            else if (p.Header == "31") // new connected
            {
                int id = p.ReadInt();
                string name = p.ReadString();
                if (!Player.All.ContainsKey(id))
                    Player.All.Add(id, new Player(id, name));
                Form.AnnounceDisConnection(name, id, conn: true);
            }
            else if (p.Header == "29") // prev
            {
                int id = p.ReadInt();
                if (!Player.All.ContainsKey(id))
                    Player.All.Add(id, new Player(id, p.ReadString()));
            }
            else if (p.Header == "3") // broadcast
            {
                if (this.Form != null)
                    this.Form.WriteLog("System >> " + p.ReadString(), Color.Red);
            }
            else if (p.Header == "4") // admin message, different from broadcasting because it can be sent to specific people only
            {
                this.Form.WriteLog("Admin:" + p.ReadString(), Color.Red);
            }
            else if (p.Header == "12") // client disconnected
            {
                int uid = p.ReadInt() ^ 0x50;
                Form.AnnounceDisConnection(Player.All[uid].Username, uid, conn: false);
                Player.All.Remove(uid);
            }
            else if (p.Header == "37") // received whisper
            {
                int senderId = p.ReadInt();
                string message = p.ReadString();
                this.Form.ReceivedWhisper(senderId, message);
            }
            else if (p.Header == "38") // the whisper i wanted to send was sent (confirmation from server)
            {
                this.Form.SentWhisper(p.ReadInt(), p.ReadString());
            }
            else if (p.Header == "-38") // whisper couldn't be sent
            {
                this.Form.WriteLog("The user you are trying to reach is not online.", settings.SystemMessage);
            }
            else if (p.Header == "42") // changed username
            {
                int uidOfChanger = p.ReadInt();
                string newUsername = p.ReadString();
                Player changer = Player.All[uidOfChanger];
                if (changer.UserID == this.UID)
                {
                    this.Username = newUsername;
                    Form.WriteLog("Success: changed username to " + newUsername, settings.SystemMessage);
                }
                else
                {
                    Form.WriteLog(changer.Username + "[" + changer.UserID + "] changed username to " + newUsername);
                }
                changer.Username = newUsername;
            }
            else if (p.Header == "-1") // kicked !
            {
                this.ConnectionLost = null; // do not reconnect
                this.Kicked = true;
                this.Form.WriteLog("You have been kicked from the server. Retry connecting in a few minutes.");
                await Task.Delay(5000);
                Environment.Exit(-1);
            }
        }


        public bool Connect()
        {
            try
            {
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Socket.Connect(IP, Port);
                Connected = true;

                byte[] buffer = new byte[4];
                int bytesRead = 0;

                if (!Receive(buffer, ref bytesRead))
                {
                    return false;
                }
                else
                {
                    byte[] toSend;
                    byte[] username__;
                    if (buffer.SequenceEqual(NameRequiredPacket))
                    {
                        username__ = enc.GetBytes(Helper.XorText(this.Username, 0x45));
                        toSend = new byte[username__.Length + 1];
                        toSend[0] = 0x45;
                        Buffer.BlockCopy(username__, 0, toSend, 1, username__.Length);
                        toSend = toSend.GetWhileNotNullBytes();
                        int sent = Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(FullAuthRequiredPacket))
                    {
                        string packet = String.Concat(
                            (char)0x55,
                            Helper.XorText(this.Username, 0x55),
                            (char)0x7f,
                            Helper.XorText(this.Password, 0x55));
                        toSend = enc.GetBytes(packet);
                        Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(InviteCodeRequiredPacket))
                    {
                        string packet = String.Concat(
                            (char)0x65,
                            this.Username,
                            (char)0x7f,
                            this.Password);
                        toSend = enc.GetBytes(packet);
                        Socket.Send(toSend);
                    }
                    else if (buffer.SequenceEqual(ServerIsFullPacket))
                    {
                        this.WriteLog("The server's capacity has been reached. Come back later !");
                        return false;
                    }

                    buffer = new byte[4];
                    if (!Receive(buffer, ref bytesRead))
                        return false;
                    else
                    {
                        if (buffer.SequenceEqual(AccessGrantedPacket))
                        {
                            this.listenThread = new Thread(NetworkAction);
                            this.listenThread.IsBackground = true;
                            this.listenThread.Start();
                            this.ConnectionLost += NetworkHelper_ConnectionLost;
                            return true;
                        }
                        else if (buffer.SequenceEqual(AccessDeniedPacket))
                        {
                            this.WriteLog("Access denied. Wrong password maybe ?");
                        }
                        return false;
                    }
                }
            }
            catch (SocketException)
            {
                return false;
            }
        }

        // todo this throws an exception when authmethod = invitecode
        // when the client tries to reconnect and finally finds the server,
        // it throws InvalidOperationException
        async void NetworkHelper_ConnectionLost()
        {
            await Task.Run(async () => {
                Player.All.Clear();

                while (!this.Socket.Connected)
                {
                    if (this.ReconnectTick != null)
                        this.ReconnectTick();
                    if (this.Connect())
                    {
                        if (this.Reconnected != null)
                            this.Reconnected();
                        this.ConnectionLost -= this.NetworkHelper_ConnectionLost;
                    }
                    else
                        await Task.Delay(5000);
                }
            });
        }

        private bool Receive(byte[] buffer, ref Int32 bytesRead)
        {
            try
            {
                bytesRead = Socket.Receive(buffer);
                return bytesRead != 0;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public event Action ConnectionLost;
        public event Action ReconnectTick;
        public event Action Reconnected;
    }
}