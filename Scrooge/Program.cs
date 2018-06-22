using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class Program
    {
        static void Main(string[] args)
        {
            Farm f = new Farm();
            
            f.Run(512);
            
            Console.ReadKey(true);

            /*char[] first = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', };
            char[] second = new char[] { '0', '1', '2', '3', '4', '5', };

            Test.Cross(first, second);
            Test.Cross(first, second);
            Test.Cross(first, second);
            Test.Cross(first, second);
            Test.Cross(first, second);

            Console.ReadKey(true);*/
        }
    }

    class Test
    {
        private static Random rand = new Random();

        public Test()
        {
            Console.WriteLine("default constructor");
        }

        public Test(int p) : this()
        {
            Console.WriteLine("constructor with param");
        }
    }
}