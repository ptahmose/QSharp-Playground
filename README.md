# QSharp-Playground
Playing around with Q#

Trying to learn some of the basic concepts of quantum computing with [Q#](https://docs.microsoft.com/en-us/quantum/quantum-qr-intro?view=qsharp-preview). Q# is Microsoft's domain-specific programming language used for expressing quantum algorithms. It is part of the [Microsoft Quantum Development Kit](https://www.microsoft.com/en-us/quantum/development-kit) - which is an [early preview version](https://blogs.microsoft.com/ai/future-quantum-microsoft-releases-free-preview-quantum-development-kit/).

You will find in this repository:
- an implementation of a quantum 4-bit adder
- a oracle function using this 4-bit adder
- leverage this oracle function by applying Grover's algorithm in order to search for two summands which give a specified sum


## 4-qubit adder

The implementation of the 4-bit follows the approach detailed [here](https://arxiv.org/ftp/quant-ph/papers/0206/0206028.pdf). This *Conventional Quantum Plain Adder* takes two 4-qubit inputs and produces a 5-qubit output. In addition, we need to have 4 ancillary qubits.

The prototype of the adder is:

    operation FourBitAdder(a1: Qubit,a2: Qubit,a3: Qubit,a4: Qubit,b1: Qubit,b2: Qubit,b3: Qubit,b4: Qubit,carry:Qubit, tempQubits:Qubit[]):()

Notes:
- The result is placed in the registers b1, b2, b3, b4, carry.
- Quantum operations have to be reversible, which this gate ensures.
- The 4 ancillary registers (tempQubits) have to be in pure state |0> on input, and they are |0> on output.
- The carry-qubit has to be in state |0> on input.

## 4-qubit adder in action

The command-line program "4BitAdderAndGrover.exe" allows to call this quantum 4-bit-adder with the inputs in a mixed state, and in a sense we calculate the sum for all possible inputs **at once** - and if we do a measurement on the result, we get **one** of those possibilities.

Here is an example:

    >.\4BitAdderAndGrover.exe -o TestAdderWithEntangledInput -r 10
    Executing the 4-bit adder with entangled inputs

    20 = 7 + 13
    19 = 11 + 8
    26 = 14 + 12
    5 = 1 + 4
    23 = 12 + 11
    11 = 4 + 7
    11 = 5 + 6
    8 = 7 + 1
    13 = 11 + 2
    16 = 5 + 11
    *** Everything OK. ***



