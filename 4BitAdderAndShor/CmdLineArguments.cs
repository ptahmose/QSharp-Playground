using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            TestAdderWithEntangledInput
        }
    }

    partial class CmdLineArguments
    {
        private OperationToExecute operationToExecute;
        private int repeats;

        private CommandLineApplication cmdLineApp;

        public CmdLineArguments(string[] args)
        {
            this.operationToExecute = OperationToExecute.Invalid;
            this.repeats = 1;
            this.cmdLineApp = new CommandLineApplication();
            this.cmdLineApp.Name = "4BitAdderAndGrover";
            this.cmdLineApp.Description = "A Q#-learning-project, playing with a quantum-4-bit-adder";
            this.cmdLineApp.HelpOption("-?|-h|--help");

            var operationOption = this.cmdLineApp.Option("-o|--operation<value>",
                "The operation to execute",
                CommandOptionType.SingleValue);
            
            var repeatsOption = this.cmdLineApp.Option("-r| --repeats<value>", "How many times to repeat the operation", CommandOptionType.SingleValue);
            repeatsOption.Validators.Add(new MustBePositiveInteger());

            this.cmdLineApp.OnExecute(
                () =>
                {
                    if (repeatsOption.HasValue())
                    {
                        this.Repeats = int.Parse(repeatsOption.Value());
                    }

                    if (operationOption.HasValue())
                    {
                        if (string.CompareOrdinal("TestAdderInPureState", operationOption.Value()) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderInPureState;
                        }
                        else if (string.CompareOrdinal("TestAdderOracle", operationOption.Value()) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderOracle;
                        }
                        else if (string.CompareOrdinal("TestAdderWithEntangledInput", operationOption.Value()) == 0)
                        {
                            this.Operation = OperationToExecute.TestAdderWithEntangledInput;
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
    }
}
