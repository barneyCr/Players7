using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Players7Server
{
	public delegate void TEventHandler<TArgs>(TArgs args);
	public delegate void TEventHandler<TSender, TArgs>(TSender sender, TArgs args);

	internal static class Helper
	{
		public static readonly Random Randomizer = new Random();
		//internal static readonly JavaScriptSerializer JSON = new JavaScriptSerializer();
		public static Func<string, object[], string> joinPacketParams = (head, obj) => string.Concat(head + "|", string.Join("|", obj));
		public static Byte NextByte(int minValue, int maxValue)
		{
			return (byte)Helper.Randomizer.Next(minValue, maxValue);
		}
		public static Int64 NextInt64()
		{
			return Randomizer.Next(int.MinValue, int.MaxValue) * Randomizer.Next(short.MinValue, short.MaxValue) * Randomizer.Next(byte.MinValue, byte.MaxValue);
		}
		public static Int32 NextInt32(int minValue = int.MinValue)
		{
			return Randomizer.Next(minValue, int.MaxValue);
		}
		public static UInt32 NextUInt32()
		{
			return (uint)(Randomizer.Next(0, int.MaxValue) * Randomizer.Next(1, 2));
		}
		public static Int16 NextShort()
		{
			return (short)Randomizer.Next(short.MinValue, short.MaxValue);
		}
		public static string GenerateRandomString(int length, bool lowerCase)
		{
			//length = length > 7 ? 7 : length;

			StringBuilder s = new StringBuilder(length);
            char[] possible = lowerCase ? possibleCharactersLowerCase : possibleCharactersAllCase;
			while (--length >= 0)
				s.Append(possible[Randomizer.Next(0, possible.Length)]);

			return s.ToString();
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
				chars[i] = (char)((int)chars[i] ^ (seed = (seed + 1 <= 0xFF) ? ++seed : 0));
			s1 = new string(chars);
			return s1;
		}
		public static int Enum(object _enum)
		{
			return (int)_enum;
		}
		public static bool DifferenceGreaterThan(DateTime one, DateTime two, TimeSpan diff)
		{
			return one - two > diff;
		}
		public static bool DifferenceGreaterThan(DateTime one, long two, TimeSpan diff)
		{
			return DifferenceGreaterThan(one, new DateTime(two), diff);
		}
		public static bool AreWeLate(DateTime pastDeadLine, TimeSpan passed)
		{
			return DateTime.Now - pastDeadLine > passed;
		}

		public static int ToInt(this string s)
		{
			return int.Parse(s);
		}

		/// <summary>
		/// Returns the header of the packet
		/// </summary>
		/// <param name="packet"></param>
		public static string GetHeader(string packet)
		{
			try
			{
				return packet.Split('|')[1];
			}
			catch (IndexOutOfRangeException)
			{
				return null;
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
		private static char[] possibleCharactersLowerCase = "abcdefghijkmnopqrstuvwxyz".ToCharArray();
        private static char[] possibleCharactersAllCase = "abcdefghijkmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
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
		public const string NULL_LITERAL = "NULL";
		public const string NULL_EMAIL = "nothing@mail.dom";
	}

	public static class Extensions
	{
        
		public static void Send(this Socket sock, string msg)
		{
            sock.Send(Networking.Server.Encoding.GetBytes(String.Concat(msg, "\n")));
		}
		public static void Send(this Socket sock, string msg, params object[] p)
		{
            sock.Send(Networking.Server.Encoding.GetBytes(String.Format(String.Concat(msg, "\n"), p)));
		}
		
		public static async void FireAndForget(this Task task)
		{
			await task;
		}
		public static void ReInitialize(this CancellationTokenSource src)
		{
			if (src == null || src.IsCancellationRequested)
				src = new CancellationTokenSource();
		}
		public static int ToInt(this bool val)
		{
			return val ? 1 : 0;
		}
		public static bool IsEven(this int val)
		{
			return (val & 1) == 0;
		}
		public static bool IsEven(this long val)
		{
			return (val & 1) != 0;
		}

		public static string ToBase64(this string original)
		{
			return Convert.ToBase64String(original.GetBytes());
		}
		public static string FromBase64(this string e)
		{
			return Convert.FromBase64String(e).GetString();
		}
		public static string GetString(this byte[] bytes)
		{
            return Networking.Server.Encoding.GetString(bytes);
		}
		public static byte[] GetBytes(this string @string)
		{
			return Networking.Server.Encoding.GetBytes(@string);
		}
		public static String[] GetWords(this String s)
		{
			String[] ps = s.Split(' ');
			for (int i = 0; i < ps.Length; i++)
			{
				ps[i] = ps[i].Replace(".", "");
				ps[i] = ps[i].Replace("_", "");
			}
			return ps;
		}
		public static T RemoveReturn<T>(this List<T> list)
		{
			var r = list.ElementAt(Helper.Randomizer.Next(0, list.Count));
			list.Remove(r);
			return r;
		}
		public static IEnumerable<T> ExceptThis<T>(this IEnumerable<T> list, T element)
		{
			return list.Where(t => !t.Equals(element));
		}
		public static IEnumerable<T> ExceptSome<T>(this IEnumerable<T> list, params T[] elements)
		{
			return list.Except(elements);
		}
		public static IEnumerable<TValue> ValuesWhere<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<TValue> predicate)
		{
			foreach (var pair in dictionary)
				if (predicate(pair.Value))
					yield return pair.Value;
		}
	}
}