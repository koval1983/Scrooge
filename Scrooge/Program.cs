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
                n = new Network(5);

                Console.WriteLine(n);

                Console.WriteLine("do you like it?");
            }
            while (Console.ReadKey().Key != System.ConsoleKey.Y);

            float[] input = new float[] { 1.0f, 2.0f, 3.0f };

            float[] output = n.Query(input);

            Console.WriteLine("input: "+ MatrixTools.Vector2String(input));

            Console.WriteLine(n);

            Console.WriteLine("output: " + MatrixTools.Vector2String(output));

            //Console.WriteLine(n);
        }
    }
}