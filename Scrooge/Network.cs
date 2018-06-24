using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Alea;

namespace Scrooge
{
    class Network : IComparable
    {
        private static int max_input_layer_size = 20;
        private static int max_layer_size = 150;
        private static int min_layer_size = 3;
        private static readonly int output_layer_size = 3;
        private static readonly int relation_degree = 1;
        private static int last_id = 0;

        private readonly int id;
        private int[][] parents = new int[0][];

        /**
         * length of DNA is a number of layers in network, each numeric is a number of nodes in layer
         * first node is a learning_rate. To get a correct rate you need divide rate by 100
         * second DNA.node is a number of input network's nodes
         * last node everytime is 3 because output layer size is 3
         */
        public readonly int[] DNA;
        private float[][,] matrices;
        private static Random rand = new Random();
        private float score = -1;

        private Network()
        {
            id = ++last_id;
        }

        public Network(int[] dna) : this()
        {
            DNA = dna;
        }

        public Network(int[] dna, Network parent1, Network parent2) : this(dna)
        {
            SetNewParents(parent1, parent2);
        }

        public Network(int DNALength) : this()
        {
            DNA = GetRandomDNA(DNALength);
        }

        protected float[][,] GetMatrices()
        {
            if(matrices == null)
            {
                matrices = new float[DNA.Length - 2][,];
                outputs  = new float[DNA.Length - 1][];
                inputs   = new float[DNA.Length - 1][];

                for (int i = 1; i < DNA.Length - 1; i++)
                {
                    matrices[i - 1] = GetRandomMatrix(DNA[i + 1], DNA[i]);
                }
            }
            
            return matrices;
        }

        protected float[,] GetRandomMatrix(int rows, int cols)
        {
            float[,] m = new float[rows, cols];
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    m[r, c] = (float)rand.NextDouble() - 0.5f;

                    if (m[r, c] == 0)
                        m[r, c] = 0.01f;
                }
            }

            return m;
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

            int
                divider1 = rand.Next(1, DNA.Length),
                divider2 = rand.Next(1, other.DNA.Length);

            //for the first child we take the first part of self and the second part of other
            int[] new_dna1 = new int[divider1 + other.DNA.Length - divider2];

            Array.Copy(DNA, new_dna1, divider1);
            Array.Copy(other.DNA, divider2, new_dna1, divider1, other.DNA.Length - divider2);

            children.Add(new Network(new_dna1, this, other));

            //for the second child we take the first part of other and the second part of self 
            int[] new_dna2 = new int[divider2 + DNA.Length - divider1];

            Array.Copy(other.DNA, new_dna2, divider2);
            Array.Copy(DNA, divider1, new_dna2, divider2, DNA.Length - divider1);

            children.Add(new Network(new_dna2, other, this));

            //sorting
            children.Sort();
            
            return children[0];
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
        
        private int[] GetRandomDNA(int length)
        {
            
            int[] dna = new int[length];

            dna[0] = rand.Next(1, 11);//learning rate
            dna[1] = rand.Next(min_layer_size, max_input_layer_size + 1);//size of input layer

            for (int i = 2; i < dna.Length - 1; i++)
            {
                dna[i] = rand.Next(min_layer_size, max_layer_size + 1);
            }

            dna[dna.Length - 1] = output_layer_size;

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

        public float GetScore()
        {
            if (score == -1)
            {
                //GetMatrices();

                score = DNA.Length;

                //matrices = null;
            }

            return score;
        }

        public string Dna2Str(int[] dna)
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

        public void Train(float[] target, float[] input)
        {
            float[][,] matrices = GetMatrices();
            float[]    error    = MatrixTools.SubstractVV(target, Query(input));

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