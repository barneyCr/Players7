using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InviteCodeGenerator
{
    class Program
    {
        static bool HIRED_PROC;

        static int GROUP_COUNT = 5;
        static int CHARS_PER_GRP_MAX = 7;
        const int LETTERS_PER_GRP_MIN = 3;
        const int DIGITS_PER_GRP_MIN = 1;

        const int MANDATORY = (LETTERS_PER_GRP_MIN + DIGITS_PER_GRP_MIN);
        static int TOTAL_CHAR_COUNT() { return (CHARS_PER_GRP_MAX + 1) * GROUP_COUNT - 1; }

        const string SEPARATOR = "\n\n\n------------------------------\n";

        static Random Randomizer = new Random();

        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Cate combinatii doresti sa fie generate?");
                int comb;
                if (args.Length == 0)
                {
                    comb = int.Parse(Console.ReadLine());
                    HIRED_PROC = false;
                }
                else
                {
                    /* 0 = nr coduri
                     * 1 = nr grupari
                     * 2 = char/grp
                     * 3 = open file after finish
                     */

                    try
                    {
                        comb = int.Parse(args[0]);
                        GROUP_COUNT = int.Parse(args[1]);
                        CHARS_PER_GRP_MAX = int.Parse(args[2]);
                    }
                    catch (FormatException)
                    {
                        return -1;
                    }
                    HIRED_PROC = true;
                }
                var stopwatch = Stopwatch.StartNew();

                Console.WriteLine(SEPARATOR);

                char[] code;
                char[][] codesBuffer = new char[comb][];
                int digUsed, lettersUsed, chunkIndex;


                for (int i = 0; i < comb; i++) // iteration for a new code
                {
                    code = new char[TOTAL_CHAR_COUNT()];
                    chunkIndex = 0;
                    for (int j = 0; j < GROUP_COUNT; j++, chunkIndex++) // iteration for a new group
                    {
                        digUsed = lettersUsed = 0;
                        for (int k = 0; k < CHARS_PER_GRP_MAX; k++, chunkIndex++) // iteration for a new char
                        {
                            if (k >= MANDATORY && ((digUsed < DIGITS_PER_GRP_MIN) || (lettersUsed < LETTERS_PER_GRP_MIN)))
                            {
                                if (digUsed < DIGITS_PER_GRP_MIN)
                                {
                                    code[chunkIndex] = DigitGenerator();
                                    digUsed++;
                                }
                                else
                                {
                                    code[chunkIndex] = LetterGenerator();
                                    lettersUsed++; 
                                }
                            }
                            else
                            {
                                switch (NextC())
                                {
                                    case CharChoice.Digit:
                                        code[chunkIndex] = DigitGenerator();
                                        digUsed++;
                                        break;
                                    case CharChoice.Letter:
                                        code[chunkIndex] = LetterGenerator();
                                        lettersUsed++;
                                        break;
                                }
                            }
                        }

                        if (chunkIndex + 1 > TOTAL_CHAR_COUNT()) // test if we are at the end of this code
                        {
                            if (HIRED_PROC && comb >= 30000)
                            {
                                if (i%5000==0)
                                Console.WriteLine("Generated {0} codes... <{1}%>", i, ((double)i)/((double)comb)*100);
                            }
                            else
                            {
                                Console.WriteLine(code);
                            }
                            codesBuffer[i] = code;
                            break;
                        }
                        // if not, add the line
                        code[chunkIndex] = '-';
                    }


                }

                Console.WriteLine(SEPARATOR);
                stopwatch.Stop();
                Console.WriteLine("Generated in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                Console.WriteLine(SEPARATOR);

                Console.WriteLine("Do you want these codes written to a text file?\n(1/yes, if yes)\n");
                string answer;
                answer = HIRED_PROC ? "1" : Console.ReadLine();
                if (answer == "1" || answer.ToLower() == "yes")
                {
                    Console.WriteLine("\nWriting...");
                    using (StreamWriter writer = new StreamWriter(args[4]))
                    {
                        for (int i = 0; i < comb; i++)
                        {
                            writer.WriteLine(codesBuffer[i]);
                        }
                    }
                    Console.WriteLine("Done!");
                    if (args.Length != 0 && args[3].ToLower() == "yes")
                    {
                        Process.Start(args[4]);
                    }
                }
                if (!HIRED_PROC)
                    Console.ReadLine();
                return 0;
            }
            catch
            {
                // TODO add better handling
                return -2;
            }
        }

        static char LetterGenerator()
        {
            return (char)Randomizer.Next(65, 90);
        }
        static char DigitGenerator()
        {
            return (char)Randomizer.Next(48, 57);
        }

        static CharChoice NextC()
        {
            return (CharChoice)Randomizer.Next(2);
        }

        enum CharChoice
        {
            Digit = 0,
            Letter = 1
        }
    }
}