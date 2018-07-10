using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Scrooge
{
    class Farm
    {
        private byte stuck_counter = 0;
        static readonly byte stuck_counterlimit = 15;

        private Network bestNetwork;
        private float bestResult;

        public void Run(int poulation_size)
        {
            Network[] generation = LoadGeneration();

            if(generation.Length == 0)
                generation = GetFirstGeneration(poulation_size);
            
            Console.WriteLine("evolution started...\n");

            int generation_number = 0;

            while (generation.Length > 0)
                generation = GetNextGeneration(generation, generation_number++);

            Console.WriteLine("\npopulation has died out");
        }
        
        public Network[] GetNextGeneration(Network[] generation, int generation_number)
        {
            Console.WriteLine("\ngeneration " + generation_number + " begin...");
            Console.WriteLine("size: " + generation.Length);
            Console.WriteLine("crossing...");

            Network first, second;

            int
                counter = 0,
                total = (int)Math.Pow(generation.Length, 2);

            List<Network> children = new List<Network>(total);
            
            //Thread[] threads = new Thread[10];
            Thread[] threads = new Thread[2];
            int t = 0, index = 0;

            for (int i = 0; i < generation.Length; i++)
            {
                first = generation[i];
                
                for (int j = 0; j < generation.Length; j++)
                {
                    second = generation[j];

                    if (first.IsRelated(second))
                        second = new Network(first.DNA.Length);

                    t = 0;

                    while (true)
                    {
                        if (threads[t] == null || !threads[t].IsAlive)
                        {
                            threads[t] = new Thread(Cross);

                            threads[t].Start(new Couple(first, second, index++, children));

                            break;
                        }

                        if (++t >= threads.Length)
                            t = 0;
                    }
                    //children.Add(first.Cross(second));

                    Console.Write("\rready: {0} / {1}   ", ++counter, total);
                }
            }

            for (int i = 0; i < threads.Length; i++)
            {
                if (threads[i] != null && threads[i].IsAlive)
                    threads[i].Join();
            }

            Console.WriteLine("\nwe've got "+ children.Count +" children");
            Console.WriteLine("sorting...");

            children.Sort();
            
            if (children.Count > 0)
            {
                Console.WriteLine("best score: " + children[0].GetScore() +"; length: "+ children[0].DNA.Length);
                Console.WriteLine("worst score: " + children[children.Count - 1].GetScore() + "; length: " + children[children.Count - 1].DNA.Length);
                
                /*if (bestResult > 0 && bestResult == children[0].GetScore() && bestNetwork.IsRelated(children[0]))
                    stuck_counter++;
                else
                    stuck_counter = 0;

                Console.WriteLine("stuck counter: " + stuck_counter);*/
            }

            generation = FilterBrothersAndSisters(children, generation.Length, generation_number);

            /*if (stuck_counter >= stuck_counterlimit)
            {
                generation[0] = new Network();
                Console.WriteLine("leader is changed");
            }
            else
            {
                bestNetwork = children[0];
                bestResult = children[0].GetScore();
            }*/
            
            return generation;
        }

        static void Cross(object couple)
        {
            Couple c = (Couple)couple;

            c.Cross();
        }

        protected Network[] FilterBrothersAndSisters(List<Network> children, int length, int generation_number)
        {
            Network[] new_generation = children.GetRange(0, length).ToArray();
            
            Save(new_generation);

            return new_generation;
        }

        private static string saved_networks = @"D:\generations\save.txt";
        private void Save(Network[] generation)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(saved_networks))
            {
                foreach (Network n in generation)
                {
                    file.WriteLine(n.Dna2StringToSave());
                }
            }
        }

        public Network[] LoadGeneration()
        {
            Console.WriteLine("trying to load from file \"{0}\"...", saved_networks);

            List<Network> generation = new List<Network>();

            try
            {
                StreamReader file = new StreamReader(saved_networks);
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    generation.Add(new Network(Network.SavedStringToDNA(line)));
                }

                file.Close();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("error: {0}", e.Message);
            }
            
            Console.WriteLine("{0} Networks is loaded", generation.Count);

            return generation.ToArray();
        }

        public Network[] GetFirstGeneration(int size)
        {
            Network[] g = new Network[size];
            
            for (int i = 0; i < g.Length; i++)
                g[i] = new Network(10);
            
            return g;
        }
    }
}