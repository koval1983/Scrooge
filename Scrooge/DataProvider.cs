using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Input vector is a vector has a length is period * period + 1
 * where last value is current price
 * and other values is an input vector
 */

namespace Scrooge
{
    class DataProvider
    {
        private static List<List<List<float>>> data;//_data[day][row][item] 
        private static string filename = "D:/Downloads/SPFB.RTS-6.18_170421_180521 (1).txt";
        private static char separator = ';';
        public static readonly int period = 5;

        static DataProvider()
        {
            Prepare();
        }

        private static void Prepare()
        {
            data = new List<List<List<float>>>();//готовые данные
            List<List<float>> preparedDailyData;//готовые данные за один день

            List<List<float>> PricesByDays = GetPricesByDays();//все цены по порядку, разбитые по дням

            for (int day = 0; day < PricesByDays.Count; day++)
            {
                ///Console.WriteLine("day {0}", day);
                preparedDailyData = GetPreparedDailyData(PricesByDays[day]);

                if (preparedDailyData.Count > 0)
                    data.Add(preparedDailyData);

                //Console.ReadKey();
            }
        }

        private static List<List<float>> GetPreparedDailyData(List<float> rawDailyData)
        {
            List<List<float>> preparedDailyData = new List<List<float>>();

            List<float> inputVector;
            List<float> pricesSequence;

            for (int index = 0; index <= rawDailyData.Count - period*2; index++)
            {
                pricesSequence = rawDailyData.GetRange(index, period * 2);

                inputVector = PricesSequenceToInputVector(pricesSequence);

                //Console.WriteLine(MatrixTools.Vector2Str(inputVector));

                preparedDailyData.Add(inputVector);
            }

            return preparedDailyData;
        }

        public static List<float> PricesSequenceToInputVector(List<float> pricesSequence)
        {
            List<float> inputVector = new List<float>();
            
            for (int key_from = pricesSequence.Count - 1; key_from >= period; key_from--)
            {
                for (int key_to = key_from - 1; key_to >= key_from - period; key_to--)
                {
                    inputVector.Add(pricesSequence[key_from] - pricesSequence[key_to]);
                }
            }

            inputVector = MatrixTools.NormalizeV(inputVector);

            //do only after normalization
            if (inputVector.Count > 0)
                inputVector.Add(pricesSequence[pricesSequence.Count - 1]);//add current price to last position of input vector
            //Console.WriteLine("pricesSequence[pricesSequence.Count - 1] = {0}", pricesSequence[pricesSequence.Count - 1]);
            return inputVector;
        }

        public static List<List<List<float>>> GetData()
        {
            return data;
        }

        public static void T()
        {

        }

        private static List<List<float>> GetPricesByDays()
        {
            StreamReader file = new StreamReader(filename);
            List<List<float>> pricesByDays = new List<List<float>>();
            string line, current_date = "";
            bool is_first = true;
            string[] row;
            List<float> dailyPrices = null;
            int
                KEY_DATE = 2,
                KEY_CLOSE = 7;

            while ((line = file.ReadLine()) != null)
            {//skip first row
                if (is_first)
                {
                    is_first = false;
                    continue;
                }

                row = line.Replace(',', separator).Replace('.', ',').Split(separator);

                if (!current_date.Equals(row[KEY_DATE]))
                {
                    if (dailyPrices != null)
                        pricesByDays.Add(dailyPrices);

                    dailyPrices = new List<float>();

                    current_date = row[KEY_DATE];
                }

                dailyPrices.Add(float.Parse(row[KEY_CLOSE]));
            }

            file.Close();

            return pricesByDays;
        }
    }
}