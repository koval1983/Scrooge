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
        protected string filename;
        protected StreamReader sr;

        public DataProvider(string fname)
        {
            filename = fname;
            sr = new StreamReader(filename);
        }

        public float[][] GetData()
        {
            float[][] data = new float[2][];
            string line;

            try
            {
                if ((line = sr.ReadLine()) != null)
                {
                    int[] row = Array.ConvertAll<string, int>(line.Split(','), new Converter<string, int>(int.Parse));

                    data[0] = GetTargetRow(row[0]);
                    data[1] = GetInputsRow(row);
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return data;
        }

        protected float[] GetInputsRow(int[] src)
        {
            float[] s = new float[src.Length - 1];

            for (int i = 1; i < src.Length; i++)
            {//inputs = (numpy.asfarray(all_values[1:]) / 255.0 * 0.99) + 0.01;
                s[i - 1] = ((float)src[i] / 255 * 0.99f) + 0.01f;
            }

            return s;
        }

        protected float[] GetTargetRow(int v)
        {
            float[] t = new float[10];

            for (int i = 0; i < t.Length; i++)
            {
                t[i] = 0.01f;
            }

            t[v] = 0.99f;

            return t;
        }
    }
}
