using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;

using System.Diagnostics;
//using NUnit.Framework;
using Alea.CSharp;

namespace Scrooge
{
    class MatrixTools
    {
        private MatrixTools()
        {
            
        }

        public static float[,] FillRandomMatrix(int rows, int cols)
        {
            float[,] m = new float[rows, cols];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    m[r, c] = GetRandomMatrixValue();

                    return m;
        }

        public static float[,] FillRandomMatrix(List<int> layer, int[] incoming_keys, List<int>[] links_from)
        {
            float[,] m = new float[layer.Count, incoming_keys.Length];
            int from, to;
            for (int row = 0; row < layer.Count; row++)
            {
                from = layer[row];

                for (int column = 0; column < incoming_keys.Length; column++)
                {
                    to = incoming_keys[column];

                    if (links_from[from].IndexOf(to) == -1)
                        m[row, column] = 0;
                    else
                        m[row, column] = GetRandomMatrixValue();
                }
            }
            
            return m;
        }

        public static float GetRandomMatrixValue()
        {
            //return ((float)Network.rand.Next(1, 4999999) / 10000000 - 0.5f) * (Network.rand.Next(0, 2) == 0 ? 1 : -1);
            return ((float)Network.rand.Next(1, 49) / 100 - 0.5f) * (Network.rand.Next(0, 2) == 0 ? 1 : -1);
        }

        public static float[] AddVI(float[] v, float n)
        {
            float[] res = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
                res[i] = v[i] + n;
            return res;
        }

        public static float[] MultiplyVV(float[] v1, float[] v2)
        {
            float[] r = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
                r[i] = v1[i] * v2[i];
            return r;
        }

        public static float[] MultiplyVV(float[] v1, float[] v2, float[] v3)
        {
            /*Console.WriteLine("v1: "+ v1.Length);
            Console.WriteLine("v2: "+ v2.Length);
            Console.WriteLine("v3: "+ v3.Length);
            Console.WriteLine("==================================");*/
            float[] r = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
                r[i] = v1[i] * v2[i] * v3[i];
            return r;
        }

        public static float[] MultiplyVI(float[] v, float n)
        {
            float[] res = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
                res[i] = v[i] * n;
            return res;
        }
        
        public static float[] MultiplyMV(float[,] matrix, float[] vector)
        {
            /*Console.WriteLine("M "+ matrix.GetLength(0) + " x "+ matrix.GetLength(1));
            Console.WriteLine("V "+ vector.Length);
            Console.WriteLine("=========");*/

            int rows = matrix.GetLength(0), cols = matrix.GetLength(1);
            float[] result = new float[rows];

            for (int r = 0; r < rows; r++)
            {
                result[r] = 0;

                for (int c = 0; c < cols; c++)
                    result[r] += matrix[r, c] * vector[c];
            }
            
            return result;
        }

        public static float[] SubtractVV(float[] v1, float[] v2)
        {
            float[] r = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
                r[i] = v1[i] - v2[i];
            return r;
        }

        public static float[] SubtractFV(float f, float[] v)
        {
            float[] r = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
                r[i] = f - v[i];
            return r;
        }

        public static float[,] TransposeM(float[,] matrix)
        {
            int
                rows = matrix.GetLength(0),
                cols = matrix.GetLength(1);

            float[,] transposed = new float[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    transposed[c, r] = matrix[r, c];
                }
            }

            return transposed;
        }

        public static string Matrix2Str(float[,] matrix)
        {
            string res = "";
            int
                rows = matrix.GetLength(0),
                cols = matrix.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    res += matrix[r, c] + " ";
                }
                res += "\n";
            }

            return res;
        }

        public static string Vector2Str(float[] v)
        {
            string res = "[ ";

            for (int r = 0; r < v.Length; r++)
            {
                res += v[r] + " ";
            }

            res += "]";

            return res;
        }

        public static float[,] AddMM(float[,] m1, float[,] m2)
        {
            int
                rows = m1.GetLength(0),
                cols = m1.GetLength(1);
            float[,] res = new float[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    res[r, c] = m1[r, c] + m2[r, c];
                }
            }

            return res;
        }

        public static float[,] MultiplyVVAsMatrix(float[] v1, float[] v2)
        {
            float[,] res = new float[v1.Length, v2.Length];

            for (int r = 0; r < v1.Length; r++)
            {
                for (int c = 0; c < v2.Length; c++)
                {
                    res[r, c] = v1[r] * v2[c];
                }
            }

            return res;
        }

        public static string Vector2Str(int[] v)
        {
            string s = "VECTOR "+ v.Length +"\n";

            foreach (int i in v)
                s += "| "+ i +" |\n";

            return s;
        }

        public static string Vector2String(float[] v)
        {
            string s = "VECTOR " + v.Length + "\n";

            foreach (float i in v)
                s += "| " + i + " |\n";

            return s;
        }

        public static string Matrix2String(float[,] m)
        {
            string s = "MATRIX " + m.GetLength(0) + "X" + m.GetLength(1) + "\n";

            for (int r = 0; r < m.GetLength(0); r++)
            {
                s += "| ";
                for (int c = 0; c < m.GetLength(1); c++)
                {
                    if (m[r, c] >= 0)
                        s += " ";
                    s += Math.Round(m[r, c], 2) + "\t";
                }
                s += "|\n";
            }

            return s;
        }
    }
}
