namespace Quantum.QSharpApplication1
{
    using Microsoft.Quantum.Simulation.Core;
    using Microsoft.Quantum.Simulation.Simulators;
    using System;

    class Driver
    {
        static void Main(string[] args)
        {
            PrepareAndMeasure(1000);

            PrepareInSuperpositionAndMeasure(1000);

            PrepareTwoQBitsInEntanglementAndMeasure(1000);

            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        static void PrepareAndMeasure(int repeats)
        {
            using (var sim = new QuantumSimulator())
            {
                Console.WriteLine($"Intialize Qbit to Zero and measure ({repeats} repeats):");
                var res = SetAndMeasure.Run(sim, repeats, Result.Zero).Result;
                var (numZeros, numOnes) = res;
                Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes}");
                Console.WriteLine();


                Console.WriteLine($"Intialize Qbit to One and measure ({repeats} repeats):");
                res = SetAndMeasure.Run(sim, repeats, Result.One).Result;
                (numZeros, numOnes) = res;
                System.Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes}");
                Console.WriteLine();
            }
        }

        static void PrepareInSuperpositionAndMeasure(int repeats)
        {
            using (var sim = new QuantumSimulator())
            {
                Console.WriteLine($"Intialize Qbit to superposition H(|0>) and measure ({repeats} repeats):");
                var res = SetInSuperpositionAndMeasure.Run(sim, repeats, Result.Zero).Result;
                var (numZeros, numOnes) = res;
                Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes}");
                Console.WriteLine();


                Console.WriteLine($"Intialize Qbit to H(|1>) and measure ({repeats} repeats):");
                res = SetInSuperpositionAndMeasure.Run(sim, repeats, Result.One).Result;
                (numZeros, numOnes) = res;
                System.Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes}");
                Console.WriteLine();
            }
        }

        static void PrepareTwoQBitsInEntanglementAndMeasure(int repeats)
        {
            using (var sim = new QuantumSimulator())
            {
                Console.WriteLine($"Intialize 2 entangled Qbits to a superposition of |00> and |11>, and measure ({repeats} repeats):");
                var res = TwoQBitsInEntanglementAndMeasure.Run(sim, repeats, Result.Zero).Result;
                var (numZeros, numOnes, qbitsAgree) = res;
                Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes} Qbits had same value: {qbitsAgree}");
                Console.WriteLine();


                Console.WriteLine($"Intialize 2 entangled Qbits to a superposition of |00> and |11>, and measure ({repeats} repeats):");
                res = TwoQBitsInEntanglementAndMeasure.Run(sim, repeats, Result.One).Result;
                (numZeros, numOnes, qbitsAgree) = res;
                System.Console.WriteLine($"-> Result: |0>: {numZeros}  |1>: {numOnes} Qbits had same value: {qbitsAgree}");
                Console.WriteLine();
            }
        }
    }
}
