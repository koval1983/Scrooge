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
        public static readonly Random rand = new Random();
        private static readonly int relation_degree = 1;
        private static readonly int input_layer_size = 3;
        private static readonly int output_layer_size = 3;

        private readonly int id;
        private int[][] parents = new int[0][];
        public readonly int[][] DNA;
        private float score;

        private float[] input;
        private float[] values;//тут лежат значения каждого нейрона (после функции активации)
        private float[] output;

        private float[][,] matrices;
        private int [][] incoming_vectors_keys;//"входящие" векторы. weights_matrice x incoming_vector = layer_input
        private List<int>[] layers;//слои - массив (номер слоя -> массив с номерами нейронов)
        
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
        
        private void MakeMatricesAndVectors()
        {
            if (values != null)
                return;

            int[]  queue           = new int[DNA.Length];       //порядок обработки нейронов, номер нода -> номер очереди
            byte[] map_outputs     = new byte[DNA.Length];      //здесь отмечаем нейроны которые соединяются с последним слоем
            List<int>[] links_from = new List<int>[DNA.Length]; //здесь ДНК наоборот - номер нейрона -> список с номерами "входящих" связей
            
            int node_link_to, current_node_level, max_level = 0;
            bool is_output;
            for (int current_node = 0; current_node < DNA.Length; current_node++)
            {
                current_node_level = queue[current_node];
                
                is_output = true;
                
                for (int i = 0; i < DNA[current_node].Length; i++)
                {
                    node_link_to = current_node + DNA[current_node][i];

                    if (node_link_to >= DNA.Length)
                        continue;

                    if (links_from[node_link_to] == null)
                        links_from[node_link_to] = new List<int>();

                    links_from[node_link_to].Add(current_node);
                    
                    queue[node_link_to] = Math.Max(queue[node_link_to], current_node_level + 1);

                    max_level = Math.Max(queue[node_link_to], max_level);

                    is_output = false;
                }

                map_outputs[current_node] = (byte) (is_output? 1 : 0);
            }
            
            layers = new List<int>[max_level + 1];

            for (int i = 0; i < queue.Length; i++)
            {
                if (layers[queue[i]] == null)
                    layers[queue[i]] = new List<int>();

                layers[queue[i]].Add(i);
            }

            //строим векторы и матрицы
            List<int> vecOutKeys = new List<int>();

            matrices = new float[layers.Length + 1][,];
            incoming_vectors_keys = new int[layers.Length + 1][];

            matrices[0] = MatrixTools.FillRandomMatrix(layers[0].Count, input_layer_size);//матрица между Vi и Vh0
            
            for (int layer = 1; layer < layers.Length; layer++)
            {
                incoming_vectors_keys[layer] = GetLayerIncomeKeys(layers[layer], links_from);

                matrices[layer] = MatrixTools.FillRandomMatrix(layers[layer], incoming_vectors_keys[layer], links_from);
            }

            incoming_vectors_keys[incoming_vectors_keys.Length - 1] = GetOutputKeys(map_outputs);
            matrices[matrices.Length - 1] = MatrixTools.FillRandomMatrix(output_layer_size, incoming_vectors_keys[incoming_vectors_keys.Length - 1].Length);

            input  = new float[input_layer_size];
            values = new float[DNA.Length];
            output = new float[output_layer_size];
        }

        protected int[] GetOutputKeys(byte[] map)
        {
            List<int> keys = new List<int>();

            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == 1)
                    keys.Add(i);
            }

            return keys.ToArray<int>();
        }

        protected int[] GetLayerIncomeKeys(List<int> Layer, List<int>[] links_from)
        {
            List<int> list = new List<int>();
            
            int Neuron;
            for (int neuron = 0; neuron < Layer.Count; neuron++)
            {
                Neuron = Layer[neuron];

                for (int i = 0; i < links_from[Neuron].Count; i++)
                {
                    list.Add(links_from[Neuron][i]);
                }
            }

            return list.Distinct().ToArray<int>();
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
            int l;
            for (int i = 0; i < dna_length; i++)
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

        public float[] GetVector(int[] keys)
        {
            float[] v = new float[keys.Length];

            for (int i = 0; i < v.Length; i++)
                v[i] = values[keys[i]];

            return v;
        }

        public void SetVector(List<int> keys, float[] values)
        {
            Console.WriteLine(MatrixTools.Vector2Str(keys.ToArray<int>()));
            Console.WriteLine(MatrixTools.Vector2Str(this.values));
            for (int i = 0; i < values.Length; i++)
                this.values[keys[i]] = values[i];
            Console.WriteLine(MatrixTools.Vector2Str(this.values));
        }

        public float[] Query(float[] _input)
        {
            MakeMatricesAndVectors();

            this.input = _input;

            float[] Input = MatrixTools.MultiplyMV(matrices[0], _input);//умножаем матрицу 0 на вектор входящего сигнала, получаем инпут слоя 1
            float[] Output = ActivationFunction(Input);//применяем функцию активации к инпуту слоя 1, получаем аутпут слоя 1

            SetVector(layers[0], Output);//записываем значения первого слоя

            for (int l = 1; l < layers.Length; l++)
            {
                Input = MatrixTools.MultiplyMV(matrices[l], GetVector(incoming_vectors_keys[l]));
                Output = ActivationFunction(Input);

                SetVector(layers[l], Output);
            }

            Input = MatrixTools.MultiplyMV(matrices[matrices.Length - 1], GetVector(incoming_vectors_keys[incoming_vectors_keys.Length - 1]));

            return output = ActivationFunction(Input);
        }

        public float GetLearningRate()
        {
            return 0.15f;
        }

        /*public float[] Train(float[] target, float[] input)
        {
            float[] answer = Query(input);
            //float[][,] matrices = GetMatrices();
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
        }*/

        protected float[] ActivationFunction(float[] vector)
        {
            return vector;
            /*for(int i = 0; i < vector.Length; i++)
                vector[i] = Sigmoid(vector[i]);
            return vector;*/
        }

        protected float Sigmoid(float f)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-f));
        }

        override public string ToString()
        {
            MakeMatricesAndVectors();

            string s = "Network #" + id + "\n";

            s += DnaStr();
            s += LayersStr();
            s += LayersValuesStr();
            s += LayersKeysStr();
            s += MatricesAndVectorsStr();

            for (int i = 0; i < matrices.Length; i++)
            {
                s += MatrixTools.Matrix2String(matrices[i]);

                /*if(i >= 1)
                    s += MatrixTools.Vector2String(layers[i].ToArray());*/
            }

            return s;
        }

        public string LayersKeysStr()
        {
            string str = "KEYS:===============================================================\n";

            for (int l = 0; l < incoming_vectors_keys.Length; l++)
            {
                str += l.ToString();

                //if (l == 0) continue;
                
                str += "| ";

                if(incoming_vectors_keys[l] != null)
                    foreach (int k in incoming_vectors_keys[l])
                        str += k + " ";

                str += "\n";
            }

            return str;
        }

        public string MatricesAndVectorsStr()
        {
            string s = "MATRICES & VECTORS:=================================================\n";

            return s;
        }
        public string LayersValuesStr()
        {
            if (layers == null)
                return "";

            string str = "VALUES:=============================================================\n";

            int max_len = (layers.Length - 1).ToString().Length + 1;

            for (int l = 0; l < layers.Length; l++)
            {
                str += l.ToString();

                for (int i = l.ToString().Length; i < max_len; i++)
                    str += " ";

                str += "| ";

                foreach (int w in layers[l])
                    str += values[w] + " ";
                
                str += "\n";
            }

            return str;
        }
        public string LayersStr()
        {
            if (layers == null)
                return "";

            string str = "LAYERS:=============================================================\n";

            int max_len = (layers.Length - 1).ToString().Length + 1;

            for (int l = 0; l < layers.Length; l++)
            {
                str += l.ToString();

                for (int i = l.ToString().Length; i < max_len; i++)
                    str += " ";

                str += "| ";

                foreach(int w in layers[l])
                    str += w +" ";

                str += "\n";
            }

            return str;
        }
        public string DnaStr()
        {
            string str = "DNA:================================================================\n";

            int max_len = (DNA.Length - 1).ToString().Length + 1;

            for (int neuron = 0; neuron < DNA.Length; neuron++)
            {
                str += neuron.ToString();

                for (int i = neuron.ToString().Length; i < max_len; i++)
                    str += " ";

                str += "| ";

                for (int link_to = 0; link_to < DNA[neuron].Length; link_to++)
                {
                    str += DNA[neuron][link_to] +" ";
                }

                str += "\n";
            }
            
            return str;
        }
    }
}