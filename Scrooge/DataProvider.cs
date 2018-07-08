using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class DataProvider
    {
        private static List<List<List<float>>> data;
        private static List<float> prices;
        private static string filename = "D:/Downloads/SPFB.RTS-6.18_170421_180521 (1).txt";
        private static char separator = ';';
        public static readonly int period = 5;

        static DataProvider()
        {
            Prepare();
        }

        public static List<float> GetPrices()
        {
            return prices;
        }

        public static List<List<List<float>>> GetData()
        {
            return data;
        }

        private static void Prepare()
        {
            string line;
            
            StreamReader file = new StreamReader(filename);

            List<List<float>> rawData = new List<List<float>>();
            List<float> dailyRawData = null;

            string[] tmp;
            string current_date = "";

            int
                KEY_DATE = 2,
                KEY_CLOSE = 7,
                KEY_VOL = 8;

            bool is_first = true;

            while ((line = file.ReadLine()) != null)
            {//skip first row
                if (is_first)
                {
                    is_first = false;
                    continue;
                }

                tmp = line.Replace(',', separator).Replace('.', ',').Split(separator);

                if (!current_date.Equals(tmp[KEY_DATE]))
                {
                    if(dailyRawData != null)
                        rawData.Add(dailyRawData);

                    dailyRawData = new List<float>();

                    current_date = tmp[KEY_DATE];
                }

                dailyRawData.Add(float.Parse(tmp[KEY_CLOSE]));
            }

            file.Close();

            List<List<List<float>>> preparedData = new List<List<List<float>>>();//day => time => input vector
            List<List<float>> preparedDailyData; //time => input vector
            List<float> preparedDataRow;
            float price_from, price_to;

            for (int day = 0; day < rawData.Count; day++)
            {
                dailyRawData = rawData[day];

                preparedDailyData = new List<List<float>>();
                
                for (int i = 0; i < dailyRawData.Count; i++)
                {
                    if (i < period * 2 - 1)
                        continue;

                    preparedDataRow = new List<float>();

                    for (int from = 0; from < period; from++)//dailyRawData[i - from] - от какой цены считать изменение
                    {
                        price_from = dailyRawData[i - from];

                        prices.Add(price_from);

                        for (int to = 1; to <= period; to++)//dailyRawData[i - from - to] - к какой цене считать изменение
                        {
                            price_to = dailyRawData[i - from - to];

                            preparedDataRow.Add((price_from - price_to) / price_from);
                        }
                    }

                    if(preparedDataRow.Count > 0)
                        preparedDailyData.Add(preparedDataRow);
                }
                
                if (preparedDailyData.Count > 0)
                    preparedData.Add(preparedDailyData);
            }

            data = preparedData;
        }
    }
}