using System;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.GroverTest
{
    class Driver
    {
        static void Main(string[] args)
        {
            /*  using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
              {
                  // We define the size `N` = 2^n of the database to searched in terms of 
                  // number of qubits `n`. 
                  int nDatabaseQubits = 8;
                  var databaseSize = Math.Pow(2.0, nDatabaseQubits);

                  QArray<long> markedElements = new QArray<long>() { 0, 39, 101, 234 };

                  // We now perform Grover iterates to amplify the marked subspace.
                  int nIterations = 3;


                  // Each operation has a static method called Run which takes a simulator as
                  // an argument, along with all the arguments defined by the operation itself.  
                  var task = ApplyGroverSearch.Run(sim, markedElements, nIterations, nDatabaseQubits);

                  var result = task.Result;

              }*/
            int successfulCnt = PerformSearch(100,20);
            successfulCnt = PerformSearch(100, 3);
            successfulCnt = PerformSearch(100, 2);
            successfulCnt = PerformSearch(100, 1);
            successfulCnt = PerformSearch(100, 0);

        }

        static int PerformSearch(int repeats, int groverIterations)
        {
            int successfulCount = 0;

            using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
            {
                // We define the size `N` = 2^n of the database to searched in terms of 
                // number of qubits `n`. 
                int nDatabaseQubits = 8;
                var databaseSize = Math.Pow(2.0, nDatabaseQubits);

                QArray<long> markedElements = new QArray<long>() {23};//{ 0, 39, 101, 234 }};

                // We now perform Grover iterates to amplify the marked subspace.
                int nIterations = groverIterations;

                for (int i = 0; i < repeats; ++i)
                {
                    // Each operation has a static method called Run which takes a simulator as
                    // an argument, along with all the arguments defined by the operation itself.  
                    var task = ApplyGroverSearch.Run(sim, markedElements, nIterations, nDatabaseQubits);

                    var result = task.Result;

                    if (result.Item1 == Result.One)
                    {
                        successfulCount++;
                    }
                }
            }

            Console.WriteLine(
                $"Grover-Iterations {groverIterations}: {successfulCount} of {repeats} had the desired result.");
            return successfulCount;
        }
    }
}