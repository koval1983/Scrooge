using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;

namespace Scrooge
{
    class Network : IComparable
    {
        private static readonly int[] layers = new int[] { 200, 100, 100, 3 };
        private static int last_id = 0;
        private static Random rand = new Random();
        private static readonly int relation_degree = 1;

        private readonly int id;
        public readonly float[] DNA;
        private float score;

        private int[][] parents = new int[0][];
        private float[][,] matrices;


        /*private static int max_input_layer_size = 20;
        private static int max_layer_size = 150;
        private static int min_layer_size = 3;
        private static readonly int output_layer_size = 3;*/

        public static int GetInputLayerSize()
        {
            return layers[0];
        }

        public Network(float[] dna)
        {
            id = ++last_id;
            DNA = dna;
        }

        public Network()
        {
            id = ++last_id;
            DNA = GetRandomDNA();
        }
        
        public Network(float[] dna, Network parent1, Network parent2)
        {
            id = ++last_id;
            DNA = dna;
            SetNewParents(parent1, parent2);
        }

        protected float[][,] GetMatrices()
        {
            if(matrices == null)
            {
                matrices = new float[layers.Length - 1][,];
                outputs  = new float[layers.Length][];
                inputs   = new float[layers.Length][];

                int position = 0;
                var gpu = Gpu.Default;

                int[] l = layers;
                
                for (int i = 0; i < layers.Length - 1; i++)
                {
                    matrices[i] = new float[layers[i + 1], layers[i]];

                    for (int row = 0; row < matrices[i].GetLength(0); row++)
                        for (int column = 0; column < matrices[i].GetLength(1); column++)
                            matrices[i][row, column] = DNA[position++];
                }
            }
            
            return matrices;
        }

        public bool IsRelated(Network other)
        {
            int[]
                r1 = GetRelations(),
                r2 = other.GetRelations();

            for (int i = 0; i < r1.Length; i++)
            {
                for (int j = 0; j < r2.Length; j++)
                {
                    if (r1[i] == r2[j])
                        return true;
                }
            }

            return false;
        }

        public Network Cross(Network other)
        {
            List<Network> children = new List<Network>();

            int divider = rand.Next(1, DNA.Length);

            //for the first child we take the first part of self and the second part of other
            float[] new_dna1 = new float[DNA.Length];

            Array.Copy(DNA, new_dna1, divider);
            Array.Copy(other.DNA, divider, new_dna1, divider, other.DNA.Length - divider);

            Network n = new Network(new_dna1, this, other);

            n.GetScore();

            return n;
            /*children.Add(new Network(new_dna1, this, other));

            //for the second child we take the first part of other and the second part of self 
            float[] new_dna2 = new float[DNA.Length];

            Array.Copy(other.DNA, new_dna2, divider);
            Array.Copy(DNA, divider, new_dna2, divider, DNA.Length - divider);

            children.Add(new Network(new_dna2, other, this));

            //sorting
            children.Sort();
            
            return children[0];*/
        }

        public int GetId()
        {
            return id;
        }

        public int[][] GetParents()
        {
            return parents;
        }

        protected int[] _relations;

        public int[] GetRelations()
        {
            if (_relations == null)
            {
                _relations = new int[(int)Math.Pow(2, parents.Length + 1) - 1];
                int ii = 0;

                _relations[ii++] = id;

                for (int i = 0; i < parents.Length; i++)
                {
                    parents[i].CopyTo(_relations, ii);
                    ii += parents[i].Length;
                }

                Array.Resize<int>(ref _relations, ii);
            }

            return _relations;
        }
        
        private float[] GetRandomDNA()
        {
            int length = 0;

            for (int i = 0; i < layers.Length - 1; i++)
                length += layers[i] * layers[i + 1];
            
            float[] dna = new float[length];
            
            for (int i = 0; i < dna.Length - 1; i++)
                dna[i] = (float)rand.Next(-99, 100) / 100;
            
            return dna;
        }

        private void SetNewParents(Network parent1, Network parent2)
        {
            int[][]
                parents1 = parent1.GetParents(),
                parents2 = parent2.GetParents();

            parents = new int[Math.Min(relation_degree, Math.Max(parents1.Length, parents2.Length) + 1)][];

            int ii;

            for (int i = 0; i < parents.Length; i++)
            {
                if (i == 0)
                {
                    parents[i] = new int[] { parent1.GetId(), parent2.GetId() };
                    continue;
                }

                parents[i] = new int[(int)Math.Pow(2, i + 1)];

                ii = 0;

                if (parents1.Length >= i && parents1[i - 1].Length > 0)
                {
                    parents1[i - 1].CopyTo(parents[i], ii);
                    ii += parents1[i - 1].Length;
                }

                if (parents2.Length >= i && parents2[i - 1].Length > 0)
                {
                    parents2[i - 1].CopyTo(parents[i], ii);
                    ii += parents2[i - 1].Length;
                }

                Array.Resize<int>(ref parents[i], ii);
            }
        }

        private bool need_to_test = true;
        public float GetScore()
        {
            if (need_to_test)
            {
                need_to_test = false;

                NetworkTester test = new NetworkTester();

                score = test.Test(this);
                
                matrices = null;
            }

            return score;
        }
        
        public string Dna2Str(float[] dna)
        {
            string res = " [ ";

            for (int i = 0; i < dna.Length; i++)
            {
                res += dna[i] + " ";
            }

            res += "]";

            return res;
        }

        public int CompareTo(Object other)
        {
            Network o = (Network)other;

            if (GetScore() > o.GetScore())
                return -1;
            else if (GetScore() < o.GetScore())
                return 1;
            else
                return 0;
        }

        protected float[][] inputs, outputs;

        public float[] Query(float[] input)
        {
            float[][,] matrices = GetMatrices();
            inputs[0] = input;
            outputs[0] = input;

            for (int i = 0; i < matrices.Length; i++)
            {
                inputs[i + 1] = MatrixTools.MultiplyMV(matrices[i], outputs[i]);
                outputs[i + 1] = ActivationFunction(inputs[i + 1]);
            }

            return outputs[outputs.Length - 1];
        }

        public float GetLearningRate()
        {
            return (float)DNA[0] / 100;
        }

        public float[] Train(float[] target, float[] input)
        {
            float[] answer = Query(input);
            float[][,] matrices = GetMatrices();
            float[]    error    = MatrixTools.SubstractVV(target, answer);

            float[] minus_e, derivative, vector_one;
            float[,] weights_delta;
            
            for (int i = matrices.Length - 1; i >= 0; i--)
            {//sigmoid is outputs[i + 1]
                derivative = MatrixTools.MultiplyVV(outputs[i + 1], MatrixTools.AddVI(MatrixTools.MultiplyVI(outputs[i + 1], -1), 1));

                minus_e = MatrixTools.MultiplyVI(error, 1 * GetLearningRate());

                vector_one = MatrixTools.MultiplyVV(derivative, minus_e);

                weights_delta = MatrixTools.MultiplyVVAsMatrix(vector_one, outputs[i]);

                error = MatrixTools.MultiplyMV(MatrixTools.TransposeM(matrices[i]), error);

                matrices[i] = MatrixTools.AddMM(matrices[i], weights_delta);
            }

            return answer;
        }

        protected float[] ActivationFunction(float[] vector)
        {
            for(int i = 0; i < vector.Length; i++)
                vector[i] = Sigmoid(vector[i]);
            return vector;
        }

        protected float Sigmoid(float f)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-f));
        }
    }
}