using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace Quantum._4BitAdderAndGrover
{
    partial class CmdLineArguments
    {
        public enum OperationToExecute
        {
            Invalid,
            TestAdderInPureState,
            TestAdderOracle,
            TestAdderWithEntangledInput,
            FindSummands
        }
    }

    partial class CmdLineArguments
    {
        private OperationToExecute operationToExecute;
        private int repeats;

        private int groverIterations;

        private int expectedResult;

        private CommandLineApplication cmdLineApp;

        public CmdLineArguments(string[] args)
        {
            this.operationToExecute = OperationToExecute.Invalid;
            this.repeats = 1;
            this.groverIterations = 1;
            this.expectedResult = 1;
            this.cmdLineApp = new CommandLineApplication();
            this.cmdLineApp.Name = "4BitAdderAndGrover";
            this.cmdLineApp.Description = "A Q#-learning-project, playing with a quantum-4-bit-adder";
            this.cmdLineApp.HelpOption("-?|-h|--help");

            var operationOption = this.cmdLineApp.Option("-o|--operation<value>",
                "The operation to execute (one of TestAdderInPureState, TestAdderOracle, TestAdderWithEntangledInput, FindSummands)",
                CommandOptionType.SingleValue);
            
            var repeatsOption = this.cmdLineApp.Option("-r| --repeats<value>", "How many times to repeat the operation", CommandOptionType.SingleValue);
            repeatsOption.Validators.Add(new MustBePositiveInteger());
            
            var groverIterationsOption = this.cmdLineApp.Option("-g| --groveriterations<value>", "How many Grover-iterations to perform (in case of 'FindSummands'-operation)", CommandOptionType.SingleValue);
            repeatsOption.Validators.Add(new MustBePositiveInteger());
            
            var expectedResultOption = this.cmdLineApp.Option("-e| --expectedresult<value>", "The result for find the summands for (in case of 'FindSummands'-operation)", CommandOptionType.SingleValue);
            repeatsOption.Validators.Add(new MustBeNonNegativeInteger());

            this.cmdLineApp.OnExecute(
                () =>
                {
                    if (repeatsOption.HasValue())
                    {
                        this.Repeats = int.Parse(repeatsOption.Value());
                    }

                    if (operationOption.HasValue())
                    {
                        if (string.Compare("TestAdderInPureState", operationOption.Value(),StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderInPureState;
                        }
                        else if (string.Compare("TestAdderOracle", operationOption.Value(),StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderOracle;
                        }
                        else if (string.Compare("TestAdderWithEntangledInput", operationOption.Value(),StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderWithEntangledInput;
                        }
                        else if (string.Compare("FindSummands", operationOption.Value(),StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.Operation = OperationToExecute.FindSummands;                            
                        }
                    }

                    if (this.Operation == OperationToExecute.FindSummands)
                    {
                        if (groverIterationsOption.HasValue())
                        {
                            this.GroverIterations = int.Parse(groverIterationsOption.Value());
                        }

                        if (expectedResultOption.HasValue())
                        {
                            this.ExpectedResult = int.Parse(expectedResultOption.Value());
                        }
                    }
                });

            this.cmdLineApp.Execute(args);
        }

        public OperationToExecute Operation
        {
            get => this.operationToExecute;
            private set { this.operationToExecute = value; }
        }

        public int Repeats
        {
            get => this.repeats;
            private set { this.repeats = value; }
        }
        
        public int GroverIterations
        {
            get => this.groverIterations;
            private set => this.groverIterations = value;
        }
        
        public int ExpectedResult
        {
            get => this.expectedResult;
            private set => this.expectedResult = value;
        }

        private class MustBePositiveInteger : IOptionValidator
        {
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                // This validator only runs if there is a value
                if (!option.HasValue()) return ValidationResult.Success;

                var val = option.Value();

                int i;
                if (int.TryParse(val, out i) == false)
                {
                    return new ValidationResult($"The argument for --{option.LongName} is not a valid integer.");
                }

                if (i <= 0)
                {
                    return new ValidationResult($"The argument for --{option.LongName} must be a positive integer.");
                }

                return ValidationResult.Success;
            }
        }
        
        private class MustBeNonNegativeInteger : IOptionValidator
        {
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                // This validator only runs if there is a value
                if (!option.HasValue()) return ValidationResult.Success;

                var val = option.Value();

                if (int.TryParse(val, out int i) == false)
                {
                    return new ValidationResult($"The argument for --{option.LongName} is not a valid integer.");
                }

                if (i < 0)
                {
                    return new ValidationResult($"The argument for --{option.LongName} must be a non-negative integer.");
                }

                return ValidationResult.Success;
            }
        }

    }
}
