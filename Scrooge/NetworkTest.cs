using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class NetworkTest
    {
        private float money = 0;
        private int papers = 0;
        private int number_of_trades = 0;

        private enum Decision { LONG, FLAT, SHORT};
        
        public NetworkTest()
        {
            
        }

        public float Do(Network n)
        {
            /*List<List<List<float>>> data = DataProvider.GetData();
            List<float> prices = DataProvider.GetPrices();

            List<List<float>> dailyData;
            List<float> dataRow;
            float price;
            float[] result;
            Decision decision;
            for (int d = 0; d < data.Count; d++)
            {
                dailyData = data[d];
                price = prices[d];

                for (int t = 0; t < dailyData.Count; t++)
                {
                    dataRow = dailyData[t];

                    result = n.Query(dataRow.ToArray());

                    decision = GetDecision(result);

                    if (decision == Decision.SHORT)
                        ToShort(price);
                    else if (decision == Decision.LONG)
                        ToLong(price);
                    else
                        ToCash(price);
                }

                ToShort(price);
            }*/

            return money;
        }

        private Decision GetDecision(float[] answer)
        {
            if (answer[0] > answer[1] + answer[2])
                return Decision.SHORT;
            else if (answer[2] > answer[1] + answer[0])
                return Decision.LONG;
            else
                return Decision.FLAT;
        }

        private void ToLong(float price)
        {
            if (papers < 0)
                ToCash(price);
            else if (papers > 0)
                return;

            Console.WriteLine("To long");

            Buy(price);
        }

        private void ToShort(float price)
        {
            if (papers > 0)
                ToCash(price);
            else if (papers < 0)
                return;

            Console.WriteLine("To short");

            Sell(price);
        }

        private void ToCash(float price)
        {
            if (papers > 0)
                Sell(price);
            else if (papers < 0)
                Buy(price);
            else
                return;

            Console.WriteLine("To cash");

            number_of_trades++;
        }

        private void Buy(float price, int qty = 1)
        {
            Console.WriteLine("buy "+ qty +"x"+ price);

            papers += qty;
            money -= qty * price;
        }

        private void Sell(float price, int qty = 1)
        {
            Console.WriteLine("sell" + qty + "x" + price);

            papers -= qty;
            money += qty * price;
        }
    }
}