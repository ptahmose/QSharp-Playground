namespace Quantum._4BitAdderAndGrover
{
    #region using directives
    using Microsoft.Quantum.Simulation.Core;
    using Microsoft.Quantum.Simulation.Simulators;
    using System;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Quantum.Simulation.Simulators.QCTraceSimulators.Circuits;
    #endregion

    class Driver
    {
        private static CmdLineArguments cmdLineArguments;

        static void Main(string[] args)
        {
            cmdLineArguments = new CmdLineArguments(args);

            if (!cmdLineArguments.AreArgumentsValid)
            {
                return;
            }

            switch (cmdLineArguments.Operation)
            {
                case CmdLineArguments.OperationToExecute.TestAdderInPureState:
                    Test4BitAdderPureState(PhysicalConsole.Singleton);
                    break;
                case CmdLineArguments.OperationToExecute.TestAdderWithEntangledInput:
                    TestEntangle4BitAdder(PhysicalConsole.Singleton, cmdLineArguments.Repeats);
                    break;
                case CmdLineArguments.OperationToExecute.FindSummands:
                    _4BitAdderGroverTest(PhysicalConsole.Singleton, cmdLineArguments);
                    break;
                case CmdLineArguments.OperationToExecute.TestAdderOracle:
                    Test4BitAdderOracle(PhysicalConsole.Singleton, cmdLineArguments.SummandA, cmdLineArguments.SummandB, cmdLineArguments.ExpectedResult);
                    break;
            }
        }

        static int _4BitAdderGroverTest(IConsole console, CmdLineArguments arguments)
        {
            console.WriteLine($"Finding the summands which give the specified sum ({cmdLineArguments.ExpectedResult}), using {cmdLineArguments.GroverIterations} Grover-iterations.");
            console.WriteLine(string.Empty);

            int successfulCount = 0;

            using (var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true))
            {
                for (int i = 0; i < cmdLineArguments.Repeats; ++i)
                {
                    // Each operation has a static method called Run which takes a simulator as
                    // an argument, along with all the arguments defined by the operation itself.  
                    var result = Operation.Run(sim, cmdLineArguments.ExpectedResult, cmdLineArguments.GroverIterations).Result;

                    if (result.Item1 == Result.One)
                    {
                        successfulCount++;
                        string resultAsBinary = Convert.ToString(result.Item2, 2).PadLeft(14, '0').Insert(4, "'").Insert(6, "'").Insert(8, "'").Insert(13, "'"); ;
                        int a = ((int)result.Item2) & 0xf;
                        int b = (((int)result.Item2) >> 4) & 0xf;
                        var prevColor = console.ForegroundColor;
                        console.ForegroundColor = ConsoleColor.Green;
                        console.Out.WriteLine($"{i + 1}: a={a} b={b}  ; {result.Item2} {resultAsBinary}");
                        console.ForegroundColor = prevColor;
                    }
                    else
                    {
                        var prevColor = console.ForegroundColor;
                        console.ForegroundColor = ConsoleColor.Red;
                        console.Out.WriteLine($"{i + 1}: not successful");
                        console.ForegroundColor = prevColor;
                    }
                }
            }

            console.Out.WriteLine(
                $"Grover-Iterations {cmdLineArguments.GroverIterations}: {successfulCount} of {arguments.Repeats} had the desired result.");
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
                console.WriteLine("*** Everything OK. ***");
                console.ForegroundColor = prevColor;
            }
        }

        static void Test4BitAdderOracle(IConsole console, int a, int b, int expectedResult)
        {
            using (var sim = new QuantumSimulator())
            {
                //int inputValue = 0b0000_0_0001_0011;
                var task = Quantum._4BitAdderAndGrover.Test4BitAdderOracle.Run(sim, a, b, expectedResult);
                var r = task.Result;
                console.WriteLine(string.Format("expected result: {0}", expectedResult));
                console.WriteLine(string.Format("input a:  {0} ({1})", Convert.ToString(a, 2).PadLeft(4, '0'), a));
                console.WriteLine(string.Format("input b:  {0} ({1})", Convert.ToString(b, 2).PadLeft(4, '0'), b));
                console.WriteLine(string.Format("output a: {0} ({1})  <- must be same as input", Convert.ToString(r & 0xf, 2).PadLeft(4, '0'), r & 0xf));
                console.WriteLine(string.Format("output b: {0} ({1})  <- must be same as input", Convert.ToString((r >> 4) & 0xf, 2).PadLeft(4, '0'), (r >> 4) & 0xf));
                console.WriteLine(string.Format("complete output: {0}'{1}'{2}'{3}'{4}",Convert.ToString((r>>10)&0xf,2).PadLeft(4,'0'), Convert.ToString( (r>>9 )&1, 2).PadLeft(1, '0'), Convert.ToString((r >> 8)&1, 2).PadLeft(1, '0'), Convert.ToString((r >> 4)&0xf, 2).PadLeft(4, '0'), Convert.ToString((r) & 0xf, 2).PadLeft(4, '0')));
                console.Write(string.Format("flag-Bit: {0}", (r & 0x200) != 0 ? "true" : "false"));
                if (a+b==expectedResult)
                {
                    var prevColor = console.ForegroundColor;
                    console.ForegroundColor = ConsoleColor.Green;
                    console.WriteLine(" CORRECT");
                    console.ForegroundColor = prevColor;
                }
                else
                {
                    var prevColor = console.ForegroundColor;
                    console.ForegroundColor = ConsoleColor.Red;
                    console.WriteLine(" WRONG");
                    console.ForegroundColor = prevColor;
                }
            }
        }
    }
}