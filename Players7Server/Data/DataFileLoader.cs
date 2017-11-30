using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace Players7Server.Data
{
    public class DataFileLoader
    {
        private string _filePath;

        public Dictionary<int, ClientData> LoadedUsers { get; private set; }

        public DataFileLoader(string path)
        {
            this.LoadedUsers = new Dictionary<int, ClientData>();
            this._filePath = path; 
        }

        public void Load() {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                using (StreamReader reader = new StreamReader(this._filePath))
                {
                    // parser delegates:
                    Func<string, int> pi = int.Parse;
                    Func<string, double> pd = double.Parse;
                    Func<string, bool> pb = bool.Parse;

                    string line;
                    while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                    {
                        if (line.TrimStart().FirstOrDefault() == '\'')
                        {
                            continue; // skip lines starting with '
                        }
                        Networking.Packet data = new Networking.Packet(line, null, false);
                        int uid = data.ReadInt();
                        string username = data.ReadString();
                        DateTime premiumUntil = data.ReadDateTime();
                        ClientType ctype = (ClientType)data.ReadInt();
                        double credits =data.ReadDouble();

                        ClientData cdata = new ClientData() { UserID = uid, Username = username, CType = ctype, Money = credits, PremiumUntil = premiumUntil };
                        this.LoadedUsers.Add(uid, cdata); // if two userids are identical this will throw InvalidOpExc
                    }
                }
            }
            catch (FileNotFoundException fnfex) {
                
            }
            catch (InvalidOperationException iopex) {
                // 
            }
            catch (FormatException frmex) {
                
            }
            catch (IndexOutOfRangeException iorex) {
                
            }
            finally {
                timer.Stop();
                Program.Write(Enums.LogMessageType.Config, "Load() completed in " + timer.ElapsedMilliseconds + " ms");
            }
        }
    }
}
