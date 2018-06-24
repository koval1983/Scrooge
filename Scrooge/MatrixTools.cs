using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class MatrixTools
    {
        private MatrixTools()
        {

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

        public static float[] MultiplyVI(float[] v, float n)
        {
            float[] res = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
                res[i] = v[i] * n;
            return res;
        }

        public static float[] MultiplyMV(float[,] matrix, float[] vector)
        {
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

        public static float[] SubstractVV(float[] v1, float[] v2)
        {
            float[] r = new float[v1.Length];
            for (int i = 0; i < v1.Length; i++)
                r[i] = v1[i] - v2[i];
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
    }
}
