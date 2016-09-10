using System;
using GhostCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ProducerConsumerTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void BasicCompletionTest()
        {
            // arrange
            var origin = new GhostLine<int>();

            var doubler = new GhostLine<int>();
            origin.AddConsumer(doubler);

            origin.AddWorkUnit(5);
            origin.AddWorkUnit(6);
            origin.AddWorkUnit(7);
            origin.AddWorkUnit(8);

            bool completed = false;
            origin.ProcessCompleted += (o, e) =>
            {
                Console.WriteLine("Origin completed.");
                completed = true;
            };

            // Act
            origin.Begin();
            origin.CompleteAdding();

            Thread.Sleep(3000);

            // Assert
            Debug.Assert(completed == true);

        }

        [TestMethod]
        public void BasicComputationTest()
        {
            // arrange
            var origin = new GhostLine<int>();

            origin.Process = w =>
            {
                return w;
            };

            var doubler = new GhostLine<int>();
            origin.AddConsumer(doubler);

            origin.AddWorkUnit(5);
            origin.AddWorkUnit(6);
            origin.AddWorkUnit(7);
            origin.AddWorkUnit(8);

            var outputList = new List<int>();

            doubler.Process = w =>
            {
                int x = w * 2;
                outputList.Add(x);
                return x;
            };

            // Act
            origin.Begin();
            origin.CompleteAdding();

            Thread.Sleep(10000);

            bool success = (outputList.Any( ol => ol == 10) 
                && outputList.Any(ol => ol == 12) 
                && outputList.Any(ol => ol == 14) 
                && outputList.Any(ol => ol == 16));

            // Assert
            Debug.Assert(success);

        }
    }
}
