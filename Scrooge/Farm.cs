using System;
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
            Network[] generation = GetFirstGeneration(poulation_size);
            
            Console.WriteLine("evolution started...\n");

            int generation_number = 0;

            while (generation.Length > 0)
                generation = GetNextGeneration(generation, generation_number++);

            Console.WriteLine("\nevolution has finished");
        }
        
        public Network[] GetNextGeneration(Network[] generation, int generation_number)
        {
            Console.WriteLine("\ngeneration " + generation_number + " begin...");
            Console.WriteLine("size: " + generation.Length);
            Console.WriteLine("crossing...");

            Network first, second;

            List<Network> children = new List<Network>();
            
            int decile = generation.Length / 10 + 1;

            Thread[] threads = new Thread[3];
            int t = 0, index = 0;

            for (int i = 0; i < generation.Length; i++)
            {
                first = generation[i];
                
                for (int j = 0; j < generation.Length; j++)
                {
                    second = generation[j];

                    if (i == j)
                    {
                        continue;
                        /*second = new Network(first.DNA.Length);*/
                    }

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
                }

                if (i > 0 && i % decile == 0)
                    Console.Write('.');
            }

            for (int i = 0; i < threads.Length; i++)
            {
                if (threads[i] != null && threads[i].IsAlive)
                    threads[i].Join();
            }

            Console.WriteLine();

            Console.WriteLine("we've got "+ children.Count +" children");
            Console.WriteLine("sorting...");

            children.Sort();
            
            if (children.Count > 0)
            {
                Console.WriteLine("best score: " + children[0].GetScore());
                Console.WriteLine("worst score: " + children[children.Count - 1].GetScore());
                
                /*if (bestResult > 0 && bestResult == children[0].GetScore() && bestNetwork.IsRelated(children[0]))
                    stuck_counter++;
                else
                    stuck_counter = 0;

                Console.WriteLine("stuck counter: " + stuck_counter);*/
            }

            generation = FilterBrothersAndSisters(children, generation.Length, generation_number);

            if (stuck_counter >= stuck_counterlimit)
            {
                generation[0] = new Network();
                Console.WriteLine("leader is changed");
            }
            else
            {
                bestNetwork = children[0];
                bestResult = children[0].GetScore();
            }

            return generation;
        }

        static void Cross(object couple)
        {
            Couple c = (Couple)couple;

            c.Cross();
        }

        protected Network[] FilterBrothersAndSisters(List<Network> children, int length, int generation_number)
        {
            Console.WriteLine("filtering...");

            Network[] new_generation = new Network[length];
            Network pretender;

            int last_place = 0;
            int decile = children.Count / 10;

            for (int i = 0; i < children.Count; i++)
            {
                pretender = children[i];

                for (int j = 0; j < last_place; j++)
                {
                    if(pretender.IsRelated(new_generation[j]))
                        goto loop0;
                }

                new_generation[last_place++] = pretender;

            loop0:;

                if (i > 0 && decile > 0 && i % decile == 0)
                    Console.Write('.');
            }

            Console.WriteLine();

            Network[] toSave = new Network[last_place];

            Array.Copy(new_generation, toSave, last_place);

            Save(toSave, generation_number);

            int aliens = 0;
            
            for (; last_place < length; last_place++)
            {
                new_generation[last_place] = new Network();
                aliens++;
            }

            Console.WriteLine("aliens: " + aliens);

            return new_generation;
        }

        protected void Save(Network[] list, int generation_number)
        {
            string[] lines = new string[list.Length];

            for (int i = 0; i < list.Length; i++)
            {
                lines[i] = string.Join(" ", list[i].DNA);
            }
                

            System.IO.File.WriteAllLines("/generations/generation_"+ generation_number +"("+ list[0].GetScore() + ").txt", lines);

            Console.WriteLine("generation#"+ generation_number +" is saved");
        }

        public Network[] GetFirstGeneration(int size)
        {
            Network[] generation = new Network[size];

            for (int i = 0; i < size; i++)
                generation[i] = new Network();

            return generation;
        }
    }
}