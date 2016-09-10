using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostCode;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var origin = new GhostLine<int>();
            var doubler = new GhostLine<int>();
            origin.AddConsumer(doubler);
            var tripler = new GhostLine<int>();
            doubler.AddConsumer(tripler);
            var adder = new GhostLine<int>();
            adder.ExecuteInParallel = true;
            origin.AddConsumer(adder);

            Console.WriteLine("Starting ... press any key.");
            Console.ReadLine();

            origin.AddWorkUnit(5);
            origin.AddWorkUnit(6);
            origin.AddWorkUnit(7);
            origin.AddWorkUnit(8);
            origin.AddWorkUnit(9);
            origin.AddWorkUnit(10);
            origin.AddWorkUnit(11);
            origin.AddWorkUnit(12);
            origin.AddWorkUnit(13);

            origin.Process = w =>
            {
                Console.WriteLine("Value: " + w);
                return w;
            };

            doubler.Process = w =>
            {
                int x = w * 2;
                Console.WriteLine("Doubled value: " + x);
                return x;
            };

            tripler.Process = w =>
            {
                int x = w * 3;
                Console.WriteLine("Tripled value: " + x);
                return x;
            };

            adder.Process = w =>
            {
                int x = w + 500;
                Console.WriteLine("Added: " + x);
                return x;
            };

            origin.ProcessCompleted += (o, e) =>
            {
                Console.WriteLine("Origin completed.");
            };

            origin.Begin();
            origin.CompleteAdding();

            Console.ReadLine();
        }
    }
}
