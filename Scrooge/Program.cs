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
            
            f.Run(10);
            
            Console.ReadKey(true);

            /*float[]
                input = new float[] { 0.5f, 0.7f },
                target = new float[] { 0.01f, 0.5f };

            Network n = new Network(new int[] { 1, 2, 3, 4, 5, 6, 8, 200, 235, 2 });
            
            Console.WriteLine("input: "+ MatrixTools.Vector2Str(input));
            Console.WriteLine("target: " + MatrixTools.Vector2Str(target));
            Console.WriteLine("first result: " + MatrixTools.Vector2Str(n.Query(input)));

            for (int i = 0; i < 10000; i++)
            {
                n.Train(target, input);
            }

            Console.WriteLine("second result: " + MatrixTools.Vector2Str(n.Query(input)));*/

            Test();

            Console.ReadKey(true);
        }

        //delegate float s2f(string i);
        static void Test()
        {
            Network n = new Network();

            Console.WriteLine("score: "+ n.GetScore());
        }

        static int GetMaxKey(float[] arr)
        {
            int key = 0;
            float last = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > last)
                {
                    key = i;
                    last = arr[i];
                }
            }

            return key;
        }
    }
}