using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class NetworkTester
    {
        static readonly int period = 10;
        static readonly DataProvider dataProvider;
        private int counter = 0;
        static NetworkTester()
        {
            dataProvider = new DataProvider("data/GAZP_170626_180625.txt", period);
        }

        private float fund = 0;
        private int qty = 0;

        private void Buy(float price, int _qty)
        {
            Console.WriteLine("Buy " + _qty + " x " + price);
            qty  += _qty;
            fund -= price * _qty;
            counter++;
        }

        private void Sell(float price, int _qty)
        {
            Console.WriteLine("Sell " + _qty + " x " + price);
            qty -= _qty;
            fund += price * _qty;
            counter++;
        }

        public float Test(Network n)
        {
            float price = 0;//текущая цена актива

            float[][] example;
            float[] res;

            int
                action;//текущее действие

            dataProvider.Reset();
            int i = 0;
            while ((example = dataProvider.GetNextExample()) != null)
            {
                res = n.Query(example[1]);
                action = GetMaxKey(res);
                i++;
                //Console.WriteLine(res[0] +" "+ res[1] +" "+ res[2]);

                price  = example[0][0];
                
                if (action == 0)//to short
                {
                    if (qty < 0)
                        continue;

                    Sell(price, 1 + qty);
                }
                else if (action == 1)//to hold
                {
                    if (qty < 0)
                        Buy(price, -qty);
                    else if (qty > 0)
                        Sell(price, qty);
                    else
                        continue;
                }
                else//to long
                {
                    if (qty > 0)
                        continue;

                    Buy(price, 1 - qty);
                }
            }

            if (qty < 0)
                Buy(price, -qty);
            else if (qty > 0)
                Sell(price, qty);

            Console.WriteLine("fund: " + fund);
            Console.WriteLine("qty: " + qty);
            Console.WriteLine("i: " + i);
            Console.WriteLine("counter: " + counter);

            return fund;
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
