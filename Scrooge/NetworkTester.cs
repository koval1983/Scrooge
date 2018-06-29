using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class NetworkTester
    {
        static readonly DataProvider dataProvider;

        private int counter = 0;

        float volume = 0;
        int number_of_trades = 0;

        static NetworkTester()
        {
            dataProvider = new DataProvider("data/SPFB.RTS_170627_180626 (1).txt", Network.GetInputLayerSize() / 2);
        }

        private float fund = 0;
        private int qty = 0;

        public NetworkTester()
        {
            //Console.WriteLine("I am NetworkTester");
        }

        private void Buy(float price, int _qty)
        {
            //Console.WriteLine("Buy " + _qty + " x " + price);
            qty  += _qty;
            fund -= price * _qty;
            counter++;
        }

        private void Sell(float price, int _qty)
        {
            //Console.WriteLine("Sell " + _qty + " x " + price);
            qty -= _qty;
            fund += price * _qty;
            counter++;
        }

        public float Test(Network n, int from = 0, int to = 17785, bool log = false)
        {
            float price = 0;//текущая цена актива

            float[][] example;
            float[] res;

            int
                action;//текущее действие

            //StreamWriter file = new StreamWriter(@"D:\generations\test.csv");
            
            dataProvider.Reset();
            
            while ((example = dataProvider.GetNextExample(from++)) != null && (from < to || to <= 0))
            {
                res = n.Query(example[1]);
                action = GetMaxKey(res);
                
                //Console.WriteLine(res[0] +" "+ res[1] +" "+ res[2]);

                price  = example[0][0];
                
                if (action == 0)//to short
                    ToShort(price);
                else if (action == 1)//to hold
                    ToHold(price);
                else//to long
                    ToLong(price);

                if (log)
                {
                    //file.WriteLine(price + ";" + volume);
                }
            }

            ToHold(price);

            if (log)
            {
                //file.WriteLine(price + ";" + volume);
            }
            
            //file.Dispose();

            if (number_of_trades == 0)
                return 0;

            float avg = fund / number_of_trades;
            int k = fund > 0 ? 1 : -1;
            return k * avg * avg * number_of_trades;
        }

        public void ToLong(float price)
        {
            if (qty > 0)
                return;
            
            if(qty < 0)
                Buy(price, -qty);

            volume = fund;

            Buy(price, 1);
        }

        public void ToHold(float price)
        {
            if (qty == 0)
                return;
            else if (qty < 0)
                Buy(price, -qty);
            else if (qty > 0)
                Sell(price, qty);

            number_of_trades++;

            volume = fund;
        }

        public void ToShort(float price)
        {
            if (qty < 0)
                return;

            if (qty > 0)
                Sell(price, qty);

            volume = fund;

            Sell(price, 1);
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
