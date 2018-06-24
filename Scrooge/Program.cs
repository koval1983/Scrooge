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
            /*Farm f = new Farm();
            
            f.Run(512);
            
            Console.ReadKey(true);*/

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
            int period = 10;
            Network n = new Network(new int[] { 15, period * 5, 100, 3 });
            DataProvider dp = new DataProvider("data/GAZP_170625_180624.txt", period);

            float[][] example;
            int t = 0, score = 0, counter = 0;
            while ((example = dp.GetNextExample()) != null)
            {
                //t++;
                if (++t <= 210000)
                    n.Train(example[0], example[1]);
                else
                {
                    if (GetMaxKey(example[0]) == GetMaxKey(n.Query(example[1])))
                        score++;

                    counter++;
                }
            }
            //Console.WriteLine(t);
            Console.WriteLine("result is "+ score +" / "+ counter +" ( "+ ((float)score / counter) +" )");
            /*float[][] data;
            int c = 0;
            while ((data = dp.GetData()) != null)
            {
                n.Train(data[0], data[1]);
                c++;

                if (c % 1000 == 0) Console.Write(".");
            }
            Console.WriteLine("");


            dp = new DataProviderImage("data/mnist_test.csv");

            int counter = 0, score = 0;
            int got_answer, correct_answer;

            while ((data = dp.GetData()) != null)
            {
                got_answer = GetMaxKey(n.Query(data[1]));
                correct_answer = GetMaxKey(data[0]);

                if (got_answer == correct_answer)
                    score++;
                counter++;

                Console.WriteLine(got_answer +" : "+ correct_answer);
            }

            Console.WriteLine("result is "+ score +" / "+ counter);*/
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