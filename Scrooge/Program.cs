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
            Farm f = new Farm();

            f.Run(100);

            Console.ReadKey(true);
        }
    }
}