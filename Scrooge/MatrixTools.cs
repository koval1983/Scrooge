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

    class MatrixMultShared
    {
        private const int BlockSize = 32;

        private static float GetMatrixElement(float[,] matrix, int blockRow, int blockCol, int row, int col)
        {
            var globalRow = blockRow * BlockSize + row;
            var globalCol = blockCol * BlockSize + col;
            if (globalRow < matrix.GetLength(0) && globalCol < matrix.GetLength(1))
                return matrix[globalRow, globalCol];
            else
                return 0.0f;
        }

        private static float GetMatrixElement(int ld, float[] matrix, int blockRow, int blockCol, int row, int col)
        {
            var globalRow = blockRow * BlockSize + row;
            var globalCol = blockCol * BlockSize + col;
            var globalIdx = globalRow * ld + globalCol;
            if (globalIdx < matrix.Length)
                return matrix[globalIdx];
            else
                return 0.0f;
        }

        private static void SetMatrixElement(float[,] matrix, int blockRow, int blockCol, int row, int col, float value)
        {
            var globalRow = blockRow * BlockSize + row;
            var globalCol = blockCol * BlockSize + col;
            if (globalRow < matrix.GetLength(0) && globalCol < matrix.GetLength(1))
                matrix[globalRow, globalCol] = value;
        }

        private static void SetMatrixElement(int ld, float[] matrix, int blockRow, int blockCol, int row, int col,
            float value)
        {
            var globalRow = blockRow * BlockSize + row;
            var globalCol = blockCol * BlockSize + col;
            var globalIdx = globalRow * ld + globalCol;
            if (globalIdx < matrix.Length)
                matrix[globalIdx] = value;
        }

        private static int DivUp(int num, int den)
        {
            return (num + den - 1) / den;
        }

        private static void Kernel(float[,] a, float[,] b, float[,] c)
        {
            var colsA = a.GetLength(1);
            var blockRow = blockIdx.x;
            var blockCol = blockIdx.y;

            var valueC = 0.0f;

            var row = threadIdx.x;
            var col = threadIdx.y;

            for (var m = 0; m < DivUp(colsA, BlockSize); ++m)
            {
                var subA = __shared__.Array2D<float>(BlockSize, BlockSize);
                var subB = __shared__.Array2D<float>(BlockSize, BlockSize);

                subA[row, col] = GetMatrixElement(a, blockRow, m, row, col);
                subB[row, col] = GetMatrixElement(b, m, blockCol, row, col);
                DeviceFunction.SyncThreads();

                for (var e = 0; e < BlockSize; ++e)
                {
                    valueC += subA[row, e] * subB[e, col];
                }
                DeviceFunction.SyncThreads();
            }

            SetMatrixElement(c, blockRow, blockCol, row, col, valueC);
        }

        private static void KernelPacked(float[] a, float[] b, float[] c, int colsA, int colsB, int colsC)
        {
            var blockRow = blockIdx.x;
            var blockCol = blockIdx.y;

            float valueC = 0.0f;

            var row = threadIdx.x;
            var col = threadIdx.y;

            for (var m = 0; m < DivUp(colsA, BlockSize); ++m)
            {
                var subA = __shared__.Array2D<float>(BlockSize, BlockSize);
                var subB = __shared__.Array2D<float>(BlockSize, BlockSize);

                subA[row, col] = GetMatrixElement(colsA, a, blockRow, m, row, col);
                subB[row, col] = GetMatrixElement(colsB, b, m, blockCol, row, col);
                DeviceFunction.SyncThreads();

                for (var e = 0; e < BlockSize; ++e)
                {
                    valueC += subA[row, e] * subB[e, col];
                }
                DeviceFunction.SyncThreads();
            }

            SetMatrixElement(colsC, c, blockRow, blockCol, row, col, valueC);
        }

        /*[GpuManaged]
        public static void RunGpuPacked(float[,] a, float[,] b, float[,] c)
        {
            var lp = LaunchParam(a, b, c);
            var aFlat = Pack(a);
            var bFlat = Pack(b);
            var cFlat = new float[c.Length];
            Gpu.Default.Launch(KernelPacked, lp, aFlat, bFlat, cFlat, a.GetLength(1), b.GetLength(1), c.GetLength(1));
            Unpack(cFlat, c);
        }*/

        [GpuManaged]
        public static void RunGpu(float[,] a, float[,] b, float[,] c)
        {
            var lp = LaunchParam(a, b, c);
            Gpu.Default.Launch(Kernel, lp, a, b, c);
        }

        public static void RunCpu(float[,] a, float[,] b, float[,] c)
        {
            for (var row = 0; row < c.GetLength(0); ++row)
            {
                for (var col = 0; col < c.GetLength(1); ++col)
                {
                    float sum = 0.0f;
                    for (var k = 0; k < a.GetLength(1); ++k)
                    {
                        sum += a[row, k] * b[k, col];
                    }
                    c[row, col] = sum;
                }
            }
        }

        private static LaunchParam LaunchParam(float[,] a, float[,] b, float[,] c)
        {
            Check(a, b, c);
            var blockSize = new dim3(BlockSize, BlockSize);
            var gridSize = new dim3(DivUp(a.GetLength(0), BlockSize), DivUp(b.GetLength(1), BlockSize));
            return new LaunchParam(gridSize, blockSize);
        }

        private static float[] Pack(float[,] a)
        {
            var flat = new float[a.Length];
            var rows = a.GetLength(0);
            var cols = a.GetLength(1);
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                    flat[i * cols + j] = a[i, j];
            return flat;
        }

        [GpuManaged]
        private static void Unpack(float[] aFlat, float[,] a)
        {
            var rows = a.GetLength(0);
            var cols = a.GetLength(1);
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                    a[i, j] = aFlat[i * cols + j];
        }

        private static void Check(float[,] a, float[,] b, float[,] c)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (c == null) throw new ArgumentNullException(nameof(c));
            Debug.Assert(a.GetLength(1) == b.GetLength(0));
            Debug.Assert(a.GetLength(0) == c.GetLength(0));
            Debug.Assert(b.GetLength(1) == c.GetLength(1));
        }
    }

    /*class MatrixMultSharedTest
    {
        static readonly Random rng = new Random(42);

        public static float[,] RandomMatrix(int rows, int cols)
        {
            var a = new float[rows, cols];
            for (var i = 0; i < rows; ++i)
                for (var j = 0; j < cols; ++j)
                    a[i, j] = rng.Nextfloat();
            return a;
        }

        private static void Run(int n, float tolerance)
        {
            var a = RandomMatrix(n, n);
            var b = RandomMatrix(n, n);
            var c = new float[n, n];

            MatrixMultShared.RunCpu(a, b, c);

            var cGpu = new float[n, n];
            MatrixMultShared.RunGpu(a, b, cGpu);
            Assert.That(cGpu, Is.EqualTo(c).Within(tolerance));

            var cGpuPacked = new float[n, n];
            MatrixMultShared.RunGpuPacked(a, b, cGpuPacked);
            Assert.That(cGpuPacked, Is.EqualTo(c).Within(tolerance));
        }

        [GpuManaged, Test]
        public static void Small()
        {
            Run(128, 1e-4);
        }

        public static void Large()
        {
            Run(1024, 1e-4);
        }
    }*/
}
