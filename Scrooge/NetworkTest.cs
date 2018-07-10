using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class NetworkTest
    {
        private int number_of_learning_trades = 1;
        private float last_money_value = 0;
        private float money = 0;
        private int papers = 0;
        private int number_of_trades = 0;
        private float last_trade_price;

        private enum Decision { LONG, CASH, SHORT};
        
        public float Do(Network n)
        {
            List<List<List<float>>> data = DataProvider.GetData();
            List<List<float>> dailyData;
            float currentPrice = 0;
            float[] input;
            Decision decision;
            List<string> rows_to_save = new List<string>();

            for (int day = 0; day < data.Count; day++)
            {
                dailyData = data[day];

                //Console.WriteLine("\rDAY {0} ===================================", day);

                foreach (List<float> row in dailyData)
                {
                    currentPrice = row[row.Count - 1];
                    input        = row.GetRange(0, row.Count - 1).ToArray();
                    decision     = GetDecision(n.Query(input));

                    if (papers != 0)
                    {
                        /*if (GetCurrentResult(currentPrice) < 0)
                        {
                            if (papers < 0)
                                n.Train(new float[] { 0.99f, 0.01f, 0.01f });
                            else
                                n.Train(new float[] { 0.01f, 0.01f, 0.99f });
                        }
                        else*/ if (number_of_trades < 5000 && GetCurrentResult(currentPrice) > 0)
                        {
                            if (papers < 0)
                                n.Train(new float[] { 0.01f, 0.01f, 0.99f });
                            else
                                n.Train(new float[] { 0.99f, 0.01f, 0.01f });
                        }
                    }

                    if (decision == Decision.LONG)
                        ToLong(currentPrice);
                    else if (decision == Decision.SHORT)
                        ToShort(currentPrice);
                    else
                        ToCash(currentPrice);
                    
                    rows_to_save.Add(currentPrice +";"+ last_money_value);
                }
                
                //everyday at the end of day we need to exit to cash
                ToCash(currentPrice);
                
                //Console.ReadKey();
            }
            Console.WriteLine("account volume: {0}", money);
            Console.WriteLine("nuber of trades: {0}", number_of_trades);

            Console.WriteLine("total profit: {0}", all_profits);
            Console.WriteLine("number_of_profitable_trades: {0}", number_of_profitable_trades);

            if(number_of_profitable_trades > 0)
                Console.WriteLine("average profit: {0}", all_profits / number_of_profitable_trades);

            Console.WriteLine("total loss: {0}", all_losses);
            Console.WriteLine("number_of_unprofitable_trades: {0}", number_of_unprofitable_trades);

            if (number_of_unprofitable_trades > 0)
                Console.WriteLine("average profit: {0}", all_losses / number_of_unprofitable_trades);

            Console.WriteLine("number_of_zero_trades: {0}", number_of_zero_trades);

            if (number_of_trades > 0)
                Console.WriteLine("average trade: {0}", money/number_of_trades);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\result.csv"))
            {
                foreach (string row in rows_to_save)
                {
                    file.WriteLine(row);
                }
            }

            return money;
        }

        private float all_profits = 0;
        private float all_losses = 0;
        private int number_of_profitable_trades = 0;
        private int number_of_unprofitable_trades = 0;
        private int number_of_zero_trades = 0;


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

            //Console.Write("To long:  ");

            Buy(price);
        }

        private void ToShort(float price)
        {
            if (papers > 0)
                ToCash(price);
            else if (papers < 0)
                return;

            //Console.Write("To short: ");

            Sell(price);
        }

        private void ToCash(float price)
        {
            if (papers == 0)
                return;

            if (number_of_trades >= number_of_learning_trades)
            {
                if (GetCurrentResult(price) > 0)
                {
                    all_profits += GetCurrentResult(price);
                    number_of_profitable_trades++;
                }
                else if (GetCurrentResult(price) < 0)
                {
                    all_losses += GetCurrentResult(price);
                    number_of_unprofitable_trades++;
                }
                else
                {
                    number_of_zero_trades++;
                }
            }
            //Console.Write("To cash:  ");

            if (papers > 0)
                Sell(price);
            else if (papers < 0)
                Buy(price);
            
            number_of_trades++;
            last_money_value = money;
        }

        private void Buy(float price, int qty = 1)
        {
            //Console.WriteLine("buy  "+ qty +"x"+ price);

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