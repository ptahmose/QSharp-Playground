using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;

namespace Quantum._4BitAdderAndShor
{
    class Driver
    {
        static void Main(string[] args)
        {
            Test4BitAdderOracle();
            Test4BitAdder();
            _4BitAdderGroverTest(1000, /*71*/1);
        }

        static int _4BitAdderGroverTest(int repeats, int groverIterations)
        {
            int successfulCount = 0;

            using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
            {
                for (int i = 0; i < repeats; ++i)
                {
                    // Each operation has a static method called Run which takes a simulator as
                    // an argument, along with all the arguments defined by the operation itself.  
                    var task = Operation.Run(sim, 50, groverIterations);

                    var result = task.Result;

                    if (result.Item1 == Result.One)
                    {
                        successfulCount++;
                        Console.WriteLine(result.Item2);
                    }
                }
            }

            Console.WriteLine(
                $"Grover-Iterations {groverIterations}: {successfulCount} of {repeats} had the desired result.");
            return successfulCount;
        }

        static void Test4BitAdder()
        {
            using (var sim = new QuantumSimulator())
            {
                for (int a = 0; a < 16; ++a)
                {
                    for (int b = 0; b < 16; ++b)
                    {
                        var res = Full4BitAdder.Run(sim, a, b).Result;
                        Console.WriteLine($"{a}+{b} = {res}");
                    }
                }
            }
        }

        static void Test4BitAdderOracle()
        {
            using (var sim = new QuantumSimulator())
            {
                int inputValue = 0b0000_0_0001_0011;
                var task = Quantum._4BitAdderAndShor.Test4BitAdderOracle.Run(sim, inputValue);
                var r = task.Result;
                Console.WriteLine($"(0) -> {r}");
            }
        }
    }
}