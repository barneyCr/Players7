using System;
using System.Collections.Generic;
using System.Linq;

namespace Players7Server.GameLogic
{
    public sealed class Rewards
    {
        static float[][] Rewarding;
        static Rewards()
        {
            Rewarding = new float[][]
            {
                new float[] { 2, 1, -1, 0, 0, 0, 0 },
                new float[] { 3, 2, -0.5f, -1.5f, 0, 0, 0 },
                new float[] { 4, 3, -0.6f, -1, -1.4f, 0, 0 },
                new float[] { 5, 4, -0.7f, -0.9f, -1.1f, -1.3f, 0 },
                new float[] { 6, 5, -0.75f, -0.85f, -1.0f, -1.1f, -1.3f },
            };
        }

        /// <summary>
        /// Ordered array of winners' IDs. Initialized as n-vector
        /// with all values equal to 0. As players win, their IDs
        /// complete the vector.
        /// </summary>
        public Dictionary<int, int> PlayerIDsAndPlaces;
        public float Win;
        public Rewards(float win, int[] players)
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

        public void DistributeRewards(int[] winners, out Dictionary<int, float> distribution)
        {
            int[] wnnPlaces = PlayerIDsAndPlaces.Values.ToArray();
            int[] wnnIDs = PlayerIDsAndPlaces.Keys.ToArray();
            int len = winners.Length;
            float[] rewards = new float[len];
            for (int i = 0; i < len; i++)
            {
                rewards[i] = this.Win * Rewards.Rewarding[len - 2][wnnPlaces[i]];
            }
            distribution = new Dictionary<int, float>();
            for (int i = 0; i < len; i++)
            {
                distribution.Add(wnnIDs[i], rewards[i]);
            }
        }
    }
}