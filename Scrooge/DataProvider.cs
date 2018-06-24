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
        private static readonly char delimiter = '|';

        private float[][] data;
        private float[][] answers;
        private int counter = 0;
        private readonly int period;

        public DataProvider(string fname, int period)
        {
            this.period = period;
            this.counter = period;
            Prepare(fname);
        }

        public float[][] GetNextExample()
        {
            if (counter >= data.Length)
            {
                return null;
            }

            float[][] example = new float[2][];

            example[0] = answers[counter - 1];
            example[1] = new float[period * data[0].Length];

            int j = 0;

            for (int i = counter - period; i < counter; i++)
            {
                Array.Copy(
                    data[i],
                    0,
                    example[1],
                    j* data[0].Length,
                    data[0].Length
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
                        /*Array.Copy(line.Split(','), 4, str_row, 0, str_row.Length);
                        for (int i = 0; i < str_row.Length; i++)
                            Console.Write("|" + str_row[i] + "|");
                        Console.WriteLine();
                        row = Array.ConvertAll<string, float>(str_row, new Converter<string, float>(Single.Parse));*/
                        //Console.WriteLine(row[row.Length-1]);

                        /*for(int i = 0; i < str_row.Length; i++)
                            Console.Write("|"+ str_row[i]+"|");
                        Console.WriteLine();*/
                    }
                }
                
                list.RemoveAt(0);
                
                float[][] tmp_data = new float[list.Count][];
                string[] tmp = new string[5];

                for (int i = 0; i < list.Count; i++)
                {
                    Array.Copy(list[i].Split(delimiter), 4, tmp, 0, tmp.Length);

                    tmp_data[i] = Array.ConvertAll<string, float>(tmp, new Converter<string, float>(float.Parse));
                }

                data = new float[tmp_data.Length - 2][];
                answers = new float[tmp_data.Length - 2][];

                float current_close, next_close;
                float current_high, next_high;
                float current_low, next_low;
                float tmp2;
                for (int i = 1; i < tmp_data.Length - 1; i++)
                {
                       data[i - 1] = new float[tmp_data[i].Length];
                    answers[i - 1] = new float[3];

                    current_high = tmp_data[i][1];
                    current_low = tmp_data[i][2];
                    current_close = tmp_data[i][3];
                    next_high = tmp_data[i + 1][1];
                    next_low = tmp_data[i + 1][2];
                    next_close = tmp_data[i + 1][3];

                    if (next_high > current_high && next_low > current_low && next_close > current_close)
                    {
                        answers[i - 1][0] = 0.01f;//short
                        answers[i - 1][1] = 0.01f;//flat
                        answers[i - 1][2] = 0.99f;//long
                    }
                    else if (next_high < current_high && next_low < current_low && next_close < current_close)
                    {
                        answers[i - 1][0] = 0.99f;//short
                        answers[i - 1][1] = 0.01f;//flat
                        answers[i - 1][2] = 0.01f;//long
                    }
                    else
                    {
                        answers[i - 1][0] = 0.01f;//short
                        answers[i - 1][1] = 0.99f;//flat
                        answers[i - 1][2] = 0.01f;//long
                    }


                        //Console.WriteLine(current_close + " : " + next_close + " | " + answers[i - 1][0] +", "+ answers[i - 1][1] + ", " + answers[i - 1][2]);
                        //Console.WriteLine(answers[i - 1][0] + " , " + answers[i - 1][1] + " , " + answers[i - 1][2]);
                        for (int j = 0; j < tmp_data[i].Length; j++)
                        {
                            tmp2 = (tmp_data[i][j] - tmp_data[i - 1][j]) / tmp_data[i - 1][j];
                            data[i - 1][j] = (float)(tmp2 == 0 ? 0.00001 : tmp2);
                            //data[i - 1][j] = tmp_data[i][j];
                        }

                    //if (i == 18) break;
                }

                /*Console.WriteLine("answers.Length: "+ answers.Length);
                Console.ReadKey(true);
                for (int i = 1; i < answers.Length - 1; i++)
                {
                    Console.WriteLine(answers[i]);
                }*/
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
