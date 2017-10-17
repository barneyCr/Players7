using System;
namespace Players7Server.GameLogic
{
    public sealed class Rewards
    {
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
        public static float[][] Rewarding;

        public int[] PlayerIDs;
        public float Win;

        public Rewards(float win, int[] players)
        {
            players.CopyTo(this.PlayerIDs, 0);
            this.Win = win;
        }

        public void DistributeRewards(int[] winners, out float[] rewards)
        {
            int len = winners.Length;
            rewards = new float[len];
            for (int i = 0; i < len; i++)
            {
                rewards[i] = this.Win * Rewards.Rewarding[len - 2][winners[i]];
            }
        }
    }
}