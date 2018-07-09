using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class NetworkTest
    {
        private float last_money_value = 0;
        private float money = 0;
        private int papers = 0;
        private int number_of_trades = 0;
        private float last_trade_price;

        private enum Decision { LONG, CASH, SHORT};
        
        public NetworkTest()
        {
            
        }

        public float Do(Network n)
        {
            List<List<List<float>>> data = DataProvider.GetData();
            
            List<List<float>> dailyData;
            List<float> dataRow;
            float price;
            float[] result, expected_result;
            Decision decision;
            List<string> rows = new List<string>();

            for (int d = 0; d < data.Count; d++)
            {
                dailyData = data[d];

                //Console.WriteLine("DAY "+ d +"================================================================================");

                for (int t = 0; t < dailyData.Count; t++)
                {
                    dataRow = dailyData[t];

                    price = dataRow[dataRow.Count - 1];

                    dataRow = dataRow.GetRange(0, dataRow.Count - 1);

                    if (GetCurrentResult(price) > 0)
                    {
                        
                        //0 - to short
                        //1 - to cash
                        //2 - to long
                         
                        if (papers > 0)//long
                            expected_result = new float[] { 0.01f, 0.01f, 0.99f };
                        else
                            expected_result = new float[] { 0.99f, 0.01f, 0.01f };

                        n.Train(expected_result);
                    }
                    else if(GetCurrentResult(price) < 0)
                    {
                        if (papers > 0)
                            expected_result = new float[] { 0.99f, 0.01f, 0.01f };
                        else
                            expected_result = new float[] { 0.01f, 0.01f, 0.99f };

                        n.Train(expected_result);
                    }

                    result = n.Query(dataRow.ToArray());

                    //Console.WriteLine(MatrixTools.Vector2Str(result));

                    decision = GetDecision(result);
                    
                    if (decision == Decision.SHORT)
                        ToShort(price);
                    else if (decision == Decision.LONG)
                        ToLong(price);
                    else
                        ToCash(price);

                    if(t == dataRow.Count - 1)
                        ToCash(price);

                    rows.Add(price +";"+ last_money_value);
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\result.csv"))
            {
                foreach (string row in rows)
                {
                    file.WriteLine(row);
                }
            }
            

            return money;
        }


        /**
         * 0 - to short
         * 1 - to cash
         * 2 - to long
         */
        private Decision GetDecision(float[] answer)
        {
            if (answer[0] > answer[1] && answer[0] > answer[2])
                return Decision.SHORT;
            else if (answer[2] > answer[1] && answer[2] > answer[0])
                return Decision.LONG;
            else
                return Decision.CASH;
        }

        private void ToLong(float price)
        {
            if (papers < 0)
                ToCash(price);
            else if (papers > 0)
                return;

            //Console.WriteLine("To long");

            Buy(price);
        }

        private void ToShort(float price)
        {
            if (papers > 0)
                ToCash(price);
            else if (papers < 0)
                return;

            //Console.WriteLine("To short");

            Sell(price);
        }

        private void ToCash(float price)
        {
            if (papers == 0)
                return;

            //Console.WriteLine("To cash");

            if (papers > 0)
                Sell(price);
            else if (papers < 0)
                Buy(price);
            
            number_of_trades++;
            last_money_value = money;
        }

        private void Buy(float price, int qty = 1)
        {
            //Console.WriteLine("buy "+ qty +"x"+ price);

            last_trade_price = price;
            papers += qty;
            money -= qty * price;
        }

        private void Sell(float price, int qty = 1)
        {
            //Console.WriteLine("sell " + qty + "x" + price);

            last_trade_price = price;
            papers -= qty;
            money += qty * price;
        }

        private float GetCurrentResult(float price)
        {
            if (papers == 0)
                return 0;
            else
                return (price - last_trade_price) * papers;
        }
    }
}