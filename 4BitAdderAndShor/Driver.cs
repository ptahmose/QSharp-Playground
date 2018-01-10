using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using McMaster.Extensions.CommandLineUtils;

namespace Quantum._4BitAdderAndGrover
{
    class Driver
    {
        private static CmdLineArguments cmdLineArguments;

        static void Main(string[] args)
        {
            cmdLineArguments = new CmdLineArguments(args);
            switch (cmdLineArguments.Operation)
            {
                case CmdLineArguments.OperationToExecute.TestAdderInPureState:
                    Test4BitAdderPureState(PhysicalConsole.Singleton);
                    break;
                case CmdLineArguments.OperationToExecute.TestAdderWithEntangledInput:
                    TestEntangle4BitAdder(PhysicalConsole.Singleton, cmdLineArguments.Repeats);
                    break;
                case CmdLineArguments.OperationToExecute.FindSummands:
                    _4BitAdderGroverTest(PhysicalConsole.Singleton, cmdLineArguments.Repeats, 7);
                    break;
            }


            //Test4BitAdderOracle();
            //Test4BitAdderPureState();
            //TestEntangle4BitAdder(50);
            //_4BitAdderGroverTest(100, /*71*/7);
        }

        static int _4BitAdderGroverTest(IConsole console, int repeats, int groverIterations)
        {
            int successfulCount = 0;

            using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
            {
                for (int i = 0; i < repeats; ++i)
                {
                    // Each operation has a static method called Run which takes a simulator as
                    // an argument, along with all the arguments defined by the operation itself.  
                    var task = Operation.Run(sim, 2, groverIterations);

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

        static void Test4BitAdderPureState(IConsole console)
        {
            console.WriteLine("Testing the 4-bit adder with all possible inputs");
            console.WriteLine(string.Empty);
            int errorsEncountered = 0;
            using (var sim = new QuantumSimulator())
            {
                for (int a = 0; a < 16; ++a)
                {
                    for (int b = 0; b < 16; ++b)
                    {
                        var res = Full4BitAdder.Run(sim, a, b).Result;
                        console.Write($"{a}+{b} = {res}");
                        if (a + b != res)
                        {
                            var prevColor = console.ForegroundColor;
                            console.ForegroundColor = ConsoleColor.Red;
                            console.Write(" WRONG");
                            console.ForegroundColor = prevColor;
                            errorsEncountered++;
                        }

                        console.WriteLine(string.Empty);
                    }
                }
            }

            if (errorsEncountered > 0)
            {
                var prevColor = console.ForegroundColor;
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine($"*** {errorsEncountered} error(s) occurred. ***");
                console.ForegroundColor = prevColor;
            }
            else
            {
                var prevColor = console.ForegroundColor;
                console.ForegroundColor = ConsoleColor.Green;
                console.WriteLine($"*** Everything OK. ***");
                console.ForegroundColor = prevColor;
            }
        }

        static void TestEntangle4BitAdder(IConsole console, int repeats)
        {
            console.WriteLine("Executing the 4-bit adder with entangled inputs");
            console.WriteLine(string.Empty);
            int errorsEncountered = 0;
            using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
            {
                for (int i = 0; i < repeats; ++i)
                {
                    var task = Full4BitAdderEntangled.Run(sim);

                    var result = task.Result;

                    console.Write($"{result.Item1} = {result.Item2} + {result.Item3}");
                    if (result.Item2 + result.Item3 != result.Item1)
                    {
                        var prevColor = console.ForegroundColor;
                        console.ForegroundColor = ConsoleColor.Red;
                        console.Write(" WRONG");
                        console.ForegroundColor = prevColor;
                        errorsEncountered++;
                    }

                    console.WriteLine(string.Empty);
                }
            }

            if (errorsEncountered > 0)
            {
                var prevColor = console.ForegroundColor;
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine($"*** {errorsEncountered} error(s) occurred. ***");
                console.ForegroundColor = prevColor;
            }
            else
            {
                var prevColor = console.ForegroundColor;
                console.ForegroundColor = ConsoleColor.Green;
                console.WriteLine($"*** Everything OK. ***");
                console.ForegroundColor = prevColor;
            }
        }

        static void Test4BitAdderOracle()
        {
            using (var sim = new QuantumSimulator())
            {
                int inputValue = 0b0000_0_0001_0011;
                var task = Quantum._4BitAdderAndGrover.Test4BitAdderOracle.Run(sim, inputValue);
                var r = task.Result;
                Console.WriteLine($"(0) -> {r}");
            }
        }
    }
}