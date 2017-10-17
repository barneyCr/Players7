using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Players7Client
{
    /// <summary>
    /// BLAH BLAH BLAH IMA NOOB!
    /// </summary>
    internal static class Helper
    {
        public static readonly Random Randomizer = new Random();

        public static Int64 NextLong()
        {
            return Randomizer.Next(int.MinValue, int.MaxValue) * Randomizer.Next(short.MinValue, short.MaxValue) * Randomizer.Next(byte.MinValue, byte.MaxValue);
        }

        public static Int32 NextInt32(int minValue = int.MinValue)
        {
            return Randomizer.Next(minValue, int.MaxValue);
        }

        public static UInt32 NextUInt32()
        {
            return (uint) (Randomizer.Next(0, int.MaxValue) * Randomizer.Next(1, 2));
        }

        public static Int16 NextShort()
        {
            return (short) Randomizer.Next(short.MinValue, short.MaxValue);
        }

        public static string ConvertStringArrayToStringJoin(params string[] array)
        {
            return string.Join("|", array);
        }

        public static string XorText(string original)
        {
            return XorText(original, 0x77);
        }

        public static string XorText(string original, int seed)
        {
            string s1;
            seed = (((seed <= 0xFF) ? seed : (seed %= 0xFF)) <= 0) ? 0x77 : seed;
            var chars = original.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char) ((int) chars[i] ^ (seed = (seed + 1 <= 0xFF) ? ++seed : 0));

            s1 = new string(chars);
            return s1;
        }

        /// <summary>
        /// Returns the header of the packet
        /// </summary>
        /// <param name="packet"></param>
        [Obsolete("The fuck is this")]
        public static string GetHeader(string packet)
        {
            if (packet.Contains("|08|")) // Account
                return "08";
            else if (packet.Contains("|06|")) // Chat
                return "06";
            else if (packet.Contains("|02|")) // PING
                return "02";
            else
            {
                string[] pInfo = packet.Split('|');
                try
                {
                    return pInfo[1];
                }
                catch
                {
                    return null;
                }
            }
        }



        public static void EnforceLowering(ref int number, int lowerBound)
        {
            number %= lowerBound;
        }

        public static void EnforceLowering(ref long number, int lowerBound)
        {
            number %= lowerBound;
        }

        private static char[] chars = ",|./@*';\\=".ToCharArray();
        public static bool ContainsIllegalCharacters(this string text)
        {
            return text.ToCharArray().Intersect(chars).Any();
        }

        /// <summary>
        /// Checks if an email is valid
        /// </summary>
        /// <param name="email">the string to be checked</param>
        /// <returns>Boolean</returns>
        public static bool IsValidEmail(string email)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex _Regex = new Regex(strRegex);
            if (_Regex.IsMatch(email))
                return true;
            return false;
        }
    }
    public static class Extensions
    {
        public static String[] GetAllWords(this String s)
        {
            String[] ps = s.Split(' ');
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = ps[i].Replace(".", "");
                ps[i] = ps[i].Replace("_", "");
            }
            return ps;
        }


        /// <summary>
        /// Removes an item from a list and then returns it.
        /// </summary>
        public static T RemoveReturn<T>(this List<T> list)
        {
            var r = list.ElementAt(new Random().Next(0, list.Count));
            list.Remove(r);
            return r;
        }

        public static byte[] GetWhileNotNullBytes(this byte[] array)
        {
            return array.TakeWhile(b => b != 0).ToArray();
        }

        /// <summary>
        /// Returns all the elements except one.
        /// </summary>
        /// <typeparam name="T">the type of the elements</typeparam>
        /// <param name="list">the list we are talking about</param>
        /// <param name="element">the element</param>
        public static IEnumerable<T> ExceptThis<T>(this List<T> list, T element) where T : class
        {
            return list.Where(t => t != element);
        }

    }
}
