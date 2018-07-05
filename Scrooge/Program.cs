using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;

using System.Diagnostics;
//using NUnit.Framework;
using Alea.CSharp;

namespace Scrooge
{
    class Program
    {
        static void Main(string[] args)
        {
            //Grow();

            Test();

            Console.Write("\n\n\npress any key to exit...");
            Console.ReadKey(true);
        }

        static void Grow()
        {
            Farm f = new Farm();

            f.Run(100);
        }

        static void Test()
        {
            Network n;

            do
            {
                n = new Network(500);
                Console.WriteLine(n);
                Console.WriteLine("have you understood?");
            }
            while (Console.ReadKey().Key != System.ConsoleKey.Y);

            float[] input = new float[] { 0.01f, 0.99f, 0.01f };
            float[] expected = new float[] { 0.123f, 0.456f, 0.789f };

            float[] output = n.Query(input);

            Console.WriteLine("input: "+ MatrixTools.Vector2String(input));
            Console.WriteLine("output: " + MatrixTools.Vector2String(output));

            for (int i = 0; i < 10000; i++)
            {
                output = n.Query(input);
                n.Train(expected);
            }

            Console.WriteLine("try again");
            Console.WriteLine("input: " + MatrixTools.Vector2String(input));
            Console.WriteLine("output: " + MatrixTools.Vector2String(output));

            //Console.WriteLine(n);
        }
    }
}