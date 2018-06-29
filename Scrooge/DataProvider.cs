using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Scrooge
{
    class DataProvider
    {
        private static readonly char delimiter = '|';

        private float[][] changes;
        private float[][] prices;

        private int counter = 0;

        private readonly int period;

        public DataProvider(string fname, int period)
        {
            //Console.WriteLine("I am DataProvider");
            this.period = period;
            
            Prepare(fname);

            Reset();
        }

        public void Reset()
        {
            counter = period - 1;
        }

        public float[][] GetNextExample(int counter)
        {
            counter += period - 1;

            if (counter >= changes.Length)
                return null;

            float[][] example = new float[2][];
            
            example[0] = prices[counter];
            example[1] = new float[period * changes[0].Length];

            int j = 0;

            for (int i = counter - period + 1; i <= counter; i++)
            {
                Array.Copy(
                    changes[i],
                    0,
                    example[1],
                    j* changes[0].Length,
                    changes[0].Length
                );
                j++;
            }

            counter++;
            
            return example;
        }

        protected void Prepare(string fname)
        {
            try
            {
                List<string> list = new List<string>();
                
                using (StreamReader sr = new StreamReader(fname))
                {
                    string line;
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        list.Add(line.Replace(',', delimiter).Replace('.', ','));
                    }
                }
                
                list.RemoveAt(0);
                
                float[][] tmp_data = new float[list.Count][];
                string[] tmp = new string[2];

                for (int i = 0; i < list.Count; i++)
                {
                    Array.Copy(list[i].Split(delimiter), 7, tmp, 0, tmp.Length);

                    tmp_data[i] = Array.ConvertAll<string, float>(tmp, new Converter<string, float>(float.Parse));
                }

                changes = new float[tmp_data.Length - 1][];
                prices  = new float[tmp_data.Length - 1][];
                
                for (int i = 1; i < tmp_data.Length; i++)
                {
                    changes[i - 1] = new float[tmp_data[i].Length];
                    prices[i - 1]  = new float[] { tmp_data[i][tmp_data[i].Length - 2] };

                    for (int j = 0; j < tmp_data[i].Length; j++)
                    {
                        changes[i - 1][j] = (tmp_data[i][j] - tmp_data[i - 1][j]) / tmp_data[i - 1][j];
                        //changes[i - 1][j] = tmp_data[i][j];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
