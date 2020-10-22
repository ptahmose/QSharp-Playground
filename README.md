# QSharp-Playground
Playing around with Q#

Trying to learn some of the basic concepts of quantum computing with [Q#](https://docs.microsoft.com/en-us/quantum/quantum-qr-intro?view=qsharp-preview). Q# is Microsoft's domain-specific programming language used for expressing quantum algorithms. It is part of the [Microsoft Quantum Development Kit](https://www.microsoft.com/en-us/quantum/development-kit) - which is an [early preview version](https://blogs.microsoft.com/ai/future-quantum-microsoft-releases-free-preview-quantum-development-kit/).

You will find in this repository:
- an implementation of a quantum 4-bit adder
- a oracle function using this 4-bit adder
- leverage this oracle function by applying Grover's algorithm in order to search for two summands which give a specified sum


## 4-qubit adder

The implementation of the 4-bit adder follows the approach detailed [here](https://arxiv.org/ftp/quant-ph/papers/0206/0206028.pdf). This *Conventional Quantum Plain Adder* takes two 4-qubit inputs and produces a 5-qubit output. In addition, we need to have 4 ancillary qubits.

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

Of course - this is quite useless. It is mind-blowing that the quantum computer is able to do the computation for all possible inputs **at once** in **one step**, but what's the use if we are only able to get **one** of those results - and we are not even able to control which one. Or... can we?

## Grover's Algorithm

It seems to turn out, that we actually **can control** (to some extent) which of the results we get when measuring. That's what [Grover's Search Algorithm](https://en.wikipedia.org/wiki/Grover%27s_algorithm) is about (I found this [paper](https://arxiv.org/abs/quant-ph/9809016v2) very instructive).
The ingredient to Grover's algorithm is an *oracle function* which has the task of
* bring the input/output qubits in superposition
* determine whether the input qubits give the result we are looking for
* if so, set the flag-qubit to |1>

This allows us to specify a result, and then determine the input which gives the result. So, we can solve the question "for which input do I get this result?".

We need to repeat Grover's algorithm Pi/4*Sqrt(2^n/k) times in order to get maximum likelihood of measuring the desired result (where the flag-qubit is |1>) - where n is the number of input qubits and k is the multiplicity of "good inputs". The latter is unknown in general - which then goes beyond the basic application of Grover.

This example determines which summands give a sum of 29 (and repeat this calculation 3 times):

    > .\4BitAdderAndGrover.exe --operation FindSummands -e 29 -g 9 -r 3
    Finding the summands which give the specified sum (29), using 9 Grover-iterations.
    
    1: a=15 b=14  ; 751 0000'1'0'1110'1111
    2: a=15 b=14  ; 751 0000'1'0'1110'1111
    3: a=14 b=15  ; 766 0000'1'0'1111'1110
    Grover-Iterations 9: 3 of 3 had the desired result.

We had to iterate 9 times because <img src="https://render.githubusercontent.com/render/math?math=\frac{\pi}{4}\cdot\sqrt{\frac{2^8}{2}}"> is approximately 9 (and there are two possible sets of summands). If we use an inappropriate number of iterations, we are more likely to measure a "bad state":

    > .\4BitAdderAndGrover.exe --operation FindSummands -e 29 -g 3 -r 5
    Finding the summands which give the specified sum (29), using 3 Grover-iterations.
    
    1: a=15 b=14  ; 751 0000'1'0'1110'1111
    2: not successful
    3: a=14 b=15  ; 766 0000'1'0'1111'1110
    4: a=15 b=14  ; 751 0000'1'0'1110'1111
    5: not successful
    Grover-Iterations 3: 3 of 5 had the desired result.
