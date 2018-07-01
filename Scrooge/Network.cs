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
        private static int last_id = 0;
        private static Random rand = new Random();
        private static readonly int relation_degree = 1;

        private readonly int id;
        public readonly int[][] DNA;
        private float score;

        private int[][] parents = new int[0][];
        private float[][,] matrices;


        /*private static int max_input_layer_size = 20;
        private static int max_layer_size = 150;
        private static int min_layer_size = 3;
        private static readonly int output_layer_size = 3;*/

        /*public static int GetInputLayerSize()
        {
            return layers[0];
        }*/
        
        public Network(int dna_length)
        {
            id = ++last_id;
            DNA = GetRandomDNA(dna_length);
        }
        
        public Network(int[][] dna, Network parent1, Network parent2)
        {
            id = ++last_id;
            DNA = dna;
            
            SetNewParents(parent1, parent2);
        }

        protected float[][,] GetMatrices()
        {
            if(matrices == null)
            {
                
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

        public Network Cross(Network that)
        {
            List<Network> children = new List<Network>();

            int
                divider1 = rand.Next(1, this.DNA.Length),
                divider2 = rand.Next(1, that.DNA.Length);

            //for the child we take the first part of this and the second part of that
            int[][] new_dna = new int[divider1 + that.DNA.Length - divider2][];

            Array.Copy(this.DNA, new_dna, divider1);
            Array.Copy(that.DNA, divider2, new_dna, divider1, that.DNA.Length - divider2);

            Network n = new Network(new_dna, this, that);
            
            return n;
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
        
        private int[][] GetRandomDNA(int dna_length)
        {
            int[][] dna = new int[dna_length][];

            /*for (int i = 0; i < dna_length; i++)
                dna[i] = id;*/

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

                NetworkTest nt = new NetworkTest(this);

                score = nt.Do();
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
            return 0.15f;
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