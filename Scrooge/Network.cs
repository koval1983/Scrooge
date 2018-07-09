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
        private static readonly int input_layer_size = 25;
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

        public Network(int[][] dna)
        {
            id = ++last_id;
            DNA = dna;
        }

        public Network(int dna_length)
        {
            id = ++last_id;
            DNA = GetRandomDNA(dna_length);
        }
        
        public Network(int[][] dna, Network parent1, Network parent2) : this(dna)
        {
            SetNewParents(parent1, parent2);
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

            n.GetScore();

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

        private void MakeMatricesAndVectors()
        {
            if (values != null)
                return;

            int[] queue = new int[DNA.Length];       //порядок обработки нейронов, номер нода -> номер очереди
            byte[] map_outputs = new byte[DNA.Length];      //здесь отмечаем нейроны которые соединяются с последним слоем
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

                map_outputs[current_node] = (byte)(is_output ? 1 : 0);
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

            input = new float[input_layer_size];
            values = new float[DNA.Length];
            output = new float[output_layer_size];
        }

        private bool need_to_test = true;
        public float GetScore()
        {
            if (need_to_test)
            {
                need_to_test = false;

                MakeMatricesAndVectors();

                NetworkTest nt = new NetworkTest();

                score = nt.Do(this);

                input = null;
                values = null;
                output = null;
                matrices = null;
                incoming_vectors_keys = null;
                layers = null;
            }

            return score;
        }
        /*public float GetScore()
        {
            if (need_to_test)
            {
                need_to_test = false;

                MakeMatricesAndVectors();
            }

            NetworkTest nt = new NetworkTest();
            
            return nt.Do(this);
        }*/

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
            for (int i = 0; i < values.Length; i++)
                this.values[keys[i]] = values[i];
        }

        public float[] Query(float[] _input)
        {
            this.input = _input;

            float[] Input = MatrixTools.MultiplyMV(matrices[0], _input);//умножаем матрицу 0 на вектор входящего сигнала, получаем инпут слоя 1
            float[] Output = ActivationFunction(Input);//применяем функцию активации к инпуту слоя 1, получаем аутпут слоя 1

            SetVector(layers[0], Output);//записываем значения первого слоя

            for (int l = 1; l < layers.Length; l++)
            {
                //Console.WriteLine(MatrixTools.Matrix2String(matrices[l]));
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
        
        protected void PutErrors(int[] keys, float[] _values)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                errors[keys[i]] += _values[i];

                if (float.IsPositiveInfinity(errors[keys[i]]))
                    errors[keys[i]] = Single.MaxValue;
                else if(float.IsNegativeInfinity(errors[keys[i]]))
                    errors[keys[i]] = Single.MinValue;
            }
        }

        protected float[] GetErrors(List<int> keys)
        {
            float[] e = new float[keys.Count];

            for (int i = 0; i < e.Length; i++)
            {
                e[i] = errors[keys[i]];

                if (float.IsNaN(errors[keys[i]]))
                {
                    Console.WriteLine("NaN is gotten");
                    Console.WriteLine(errors[keys[i]]);
                    Console.ReadKey();
                }
                if (float.IsInfinity(errors[keys[i]]))
                {
                    Console.WriteLine("Infinity is gotten");
                    Console.WriteLine(errors[keys[i]]);
                    Console.ReadKey();
                }
            }
            return e;
        }

        protected float[] errors;
        public void Train(float[] expected_output)
        {
            errors = new float[DNA.Length];
            float[] output_error;
            float[] _error, tmp, prev_output, next_output;
            float[,] d;


            for (int i = matrices.Length - 1; i >= 0; i--)
            {
                if (i == matrices.Length - 1)
                {
                    next_output = output;
                    output_error = MatrixTools.SubtractVV(expected_output, output);
                    /*Console.WriteLine("=====================================================================");
                    Console.WriteLine("expected_output: " + MatrixTools.Vector2Str(expected_output));
                    Console.WriteLine("output: " + MatrixTools.Vector2Str(output));
                    Console.WriteLine("output_error: " + MatrixTools.Vector2Str(output_error));*/
                }
                else
                {
                    next_output = GetVector(layers[i].ToArray());
                    output_error = GetErrors(layers[i]);
                }

                if (i == 0)
                {
                    prev_output = input;
                }
                else
                {
                    prev_output = GetVector(incoming_vectors_keys[i]);
                }
                
                _error = MatrixTools.MultiplyVI(output_error, GetLearningRate());
                
                tmp = MatrixTools.SubtractFV(1, next_output);
                
                tmp = MatrixTools.MultiplyVV(_error, next_output, tmp);
                d = MatrixTools.MultiplyVVAsMatrix(tmp, prev_output);

                
                output_error = MatrixTools.MultiplyMV(MatrixTools.TransposeM(matrices[i]), _error);

                if (i > 0)
                    PutErrors(incoming_vectors_keys[i], output_error);
                
                matrices[i] = MatrixTools.AddMM(matrices[i], d);
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

        override public string ToString()
        {
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