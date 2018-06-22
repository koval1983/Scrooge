using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Network[] generation = GetFirstGeneration(poulation_size, 500);
            
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
            
            int decile = generation.Length / 10;

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
                    
                    children.Add(first.Cross(second));
                }

                if (i > 0 && i % decile == 0)
                    Console.Write('.');
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

            generation = FilterBrothersAndSisters(children, generation.Length);

            if (stuck_counter >= stuck_counterlimit)
            {
                generation[0] = new Network(generation[0].DNA.Length);
                Console.WriteLine("leader is changed");
            }
            else
            {
                bestNetwork = children[0];
                bestResult = children[0].GetScore();
            }

            return generation;
        }

        protected Network[] FilterBrothersAndSisters(List<Network> children, int length)
        {
            Console.WriteLine("filtering...");

            Network[] new_generation = new Network[length];
            Network pretender;

            int last_place = 0;
            int decile = children.Count / 10;
            int sum_of_dna_length = 0;

            for (int i = 0; i < children.Count; i++)
            {
                pretender = children[i];

                for (int j = 0; j < last_place; j++)
                {
                    if(pretender.IsRelated(new_generation[j]))
                        goto loop0;
                }

                new_generation[last_place++] = pretender;
                sum_of_dna_length += pretender.DNA.Length;

            loop0:;

                if (i > 0 && i % decile == 0)
                    Console.Write('.');
            }

            Console.WriteLine();

            int aliens = 0;
            int DNALength = sum_of_dna_length / last_place;

            for (; last_place < length; last_place++)
            {
                new_generation[last_place] = new Network(DNALength);
                aliens++;
            }

            Console.WriteLine("aliens: " + aliens);

            return new_generation;
        }

        public Network[] GetFirstGeneration(int size, int DNALength)
        {
            Network[] generation = new Network[size];

            for (int i = 0; i < size; i++)
                generation[i] = new Network(DNALength);

            return generation;
        }
    }
}