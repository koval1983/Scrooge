using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class Network : IComparable
    {
        private static readonly int input_layer_size = 10 * 4;
        private static readonly int output_layer_size = 3;
        private static readonly int relation_degree = 1;
        private static int last_id = 0;
        private readonly int id;
        private int[][] parents = new int[0][];
        public readonly int[][] DNA;
        private int[] queue;//очередь обработки нодов
        //private byte[] map_inputs;
        private byte[] map_outputs;
        private static Random rand = new Random();
        private float score = -1;

        private Network()
        {
            id = ++last_id;
        }

        public Network(int[][] dna, Network parent1, Network parent2) : this()
        {
            DNA = dna;
            
            SetNewParents(parent1, parent2);

            MakeQueue();
        }

        public Network(int DNALength) : this()
        {
            DNA = GetRandomDNA(DNALength);
            
            //MakeQueue();
        }

        private void MakeQueue()
        {
            queue = new int[DNA.Length];
            map_outputs = new byte[DNA.Length];

            int node_link_to, current_node_level;
            bool is_output;
            for (int current_node = 0; current_node < DNA.Length; current_node++)
            {
                current_node_level = queue[current_node];
                
                is_output = true;
                
                for (int i = 0; i < DNA[current_node].Length; i++)
                {
                    node_link_to = current_node + DNA[current_node][i];

                    if(node_link_to < DNA.Length)
                    {
                        queue[node_link_to] = Math.Max(queue[node_link_to], current_node_level + 1);
                        is_output = false;
                    }
                }

                map_outputs[current_node] = (byte)(is_output ? 1 : 0);
            }
            
                /*Console.WriteLine(Dna2Str(DNA));

                string res = "queue   [ ";

                for (int j = 0; j < queue.Length; j++)
                {
                    res += queue[j] + " ";
                }

                res += "] ";

                Console.WriteLine(res);

            res = "outputs [ ";

            for (int j = 0; j < map_outputs.Length; j++)
            {
                res += map_outputs[j] + " ";
            }

            res += "] ";

            Console.WriteLine(res);

            Console.ReadKey(true);*/
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
            int[][] new_dna1 = new int[divider1 + other.DNA.Length - divider2][];

            Array.Copy(DNA, new_dna1, divider1);
            Array.Copy(other.DNA, divider2, new_dna1, divider1, other.DNA.Length - divider2);

            children.Add(new Network(new_dna1, this, other));

            //for the second child we take the first part of other and the second part of self 
            int[][] new_dna2 = new int[divider2 + DNA.Length - divider1][];

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

        /*private byte[][] _GetRandomDNA(int length)
        {
            byte[][] dna = new byte[length][];
            
            for (int i = 0; i < dna.Length; i++)
            {
                dna[i] = new byte[dna.Length - i - 1];

                for (int j = 0; j < dna[i].Length; j++)
                {
                    dna[i][j] = (byte)rand.Next(0, 2);
                }
            }

            //Console.WriteLine(Dna2Str(dna));
            
            return dna;
        }*/

        private int[][] GetRandomDNA(int length)
        {
            int[][] dna = new int[length][];
            int l;
            for (int i = 0; i < dna.Length; i++)
            {
                dna[i] = new int[dna.Length - i - 1];
                l = 0;
                for (int j = 0; j < dna[i].Length; j++)
                {
                    
                    if (rand.Next(0, 2) == 0)
                        continue;

                    dna[i][l] = j + 1;
                    l++;
                }
                Array.Resize<int>(ref dna[i], l);
            }
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
            return rand.Next();
        }

        public string Dna2Str(int[][] dna)
        {
            string res = " [ ";

            for (int i = 0; i < dna.Length; i++)
            {
                res += " [ ";

                for (int j = 0; j < dna[i].Length; j++)
                {
                    res += dna[i][j] + " ";
                }

                res += "] ";
            }

            res += "]";

            return res;
        }

        public string PrintDna()
        {
            return Dna2Str(DNA);
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
    }
}