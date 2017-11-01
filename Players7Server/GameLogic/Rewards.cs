using System;
using System.Collections.Generic;
using System.Linq;

namespace Players7Server.GameLogic
{
    public sealed class Rewards
    {
        internal static float[][] Rewarding;
        static Rewards()
        {
            Rewarding = new float[][]
            {
                new [] { 2, 1, -1, 0, 0, 0, 0, 0f },
                new [] { 3, 2, -0.5f, -1.5f, 0, 0, 0, 0 },
                new [] { 4, 3, -0.3f, -1, -1.7f, 0, 0, 0 },
                new [] { 5, 4, 0, -0.75f, -1.4f, -1.85f, 0, 0 },
                new [] { 6, 5, 0.35f, -0.5f, -1.1f, -1.65f, -2.1f, 0 },
                new [] { 7, 6, 0.7f, -0.1f, -0.8f, -1.4f, -1.95f, -2.45f }
            };
        }

        /// <summary>
        /// Ordered array of winners' IDs. Initialized as n-vector
        /// with all values equal to 0. As players win, their IDs
        /// complete the vector.
        /// </summary>
        private Dictionary<int, int> PlayerIDsAndPlaces;
        public double Win;
        public Rewards(double win, int[] players)
        {
            //players.CopyTo(this.PlayerIDs, 0);
            this.Win = win;
            this.PlayerIDsAndPlaces = new Dictionary<int, int>(players.Length);
            for (int i = 0; i < players.Length; i++)
            {
                PlayerIDsAndPlaces.Add(players[i], 0);
            }
        }

        int won = 0;
        public int AssignPlayer(int uid) {
            //for (int i = 0; i < PlayerIDs.Length; i++)
            //{
            //    if (PlayerIDs[i] == 0)
            //    {
            //        PlayerIDs[i] = uid;
            //        break;
            //    }
            //}
            this.PlayerIDsAndPlaces[uid] = ++won;
            return won;
        }

        public bool HasFinished(int uid) {
            int val;
            if (this.PlayerIDsAndPlaces.TryGetValue(uid, out val))
            {
                return true;
            }
            else return val != 0;
        }

        public void DistributeRewards(int[] winners, out Dictionary<int, double> distribution)
        {
            int[] wnnPlaces = PlayerIDsAndPlaces.Values.ToArray();
            int[] wnnIDs = PlayerIDsAndPlaces.Keys.ToArray();
            int len = winners.Length;
            double[] rewards = new double[len];
            for (int i = 0; i < len; i++)
            {
                rewards[i] = this.Win * Rewards.Rewarding[len - 2][wnnPlaces[i]];
            }
            distribution = new Dictionary<int, double>();
            for (int i = 0; i < len; i++)
            {
                distribution.Add(wnnIDs[i], rewards[i]);
            }
        }
    }
}