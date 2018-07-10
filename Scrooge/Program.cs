using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Alea;
//using Alea.Parallel;

//using System.Diagnostics;
//using NUnit.Framework;
//using Alea.CSharp;

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

            f.Run(15);
        }

        static void Test()
        {
            

            while (true)
            {
                Network n2 = new Network(Network.SavedStringToDNA("|||||||||||||||||||||||||||||||||||||||||||||||||||||||"));

                Console.WriteLine(n2.GetScore());

                Console.ReadKey(true);
            }
        }
    }
}