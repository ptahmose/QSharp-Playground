namespace Quantum.QSharpApplication1
{
    open Microsoft.Quantum.Diagnostics;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    operation Set (desired: Result, q1: Qubit) : Unit
    {
        body (...)
        {
            let current = M(q1);
            if (desired != current)
            {
                X(q1);
            }
        }
    }

	// Prepare a Qbit in the state as specified by "Initial". Then measure the Qbit. We repeat
	// this "Count"-times, and return a tuple of integers which specify how many times we found
	// a |0> and how many times a |1>.
	operation SetAndMeasure (count : Int, initial: Result) : (Int,Int)
    {
        body (...)
        {
            mutable numOnes = 0;
            using (qubits = Qubit[1])
            {
                for (test in 1..count)
                {
                    Set (initial, qubits[0]);

                    let res = M(qubits[0]);

                    // Count the number of ones we saw:
                    if (res == One)
                    {
                        set numOnes = numOnes + 1;
                    }
                }

                Set(Zero, qubits[0]);
            }

            // Return number of times we saw a |0> and number of times we saw a |1>
            return (count-numOnes, numOnes);
        }
    }

	// Prepare a Qbit in the state as specified by "Initial", and apply Hadamard-gate. Then measure the Qbit. We repeat
	// this "Count"-times, and return a tuple of integers which specify how many times we found
	// a |0> and how many times a |1>.
	operation SetInSuperpositionAndMeasure (count : Int, initial: Result) : (Int,Int)
    {
        body (...)
        {
            mutable numOnes = 0;
            using (qubits = Qubit[1])
            {
                for (test in 1..count)
                {
                    Set (initial, qubits[0]);
					          H(qubits[0]);
                    let res = M (qubits[0]);

                    // Count the number of ones we saw:
                    if (res == One)
                    {
                        set numOnes = numOnes + 1;
                    }
                }

                Set(Zero, qubits[0]);
            }

            // Return number of times we saw a |0> and number of times we saw a |1>
            return (count-numOnes, numOnes);
        }
    }

	// returns (1) number of 1st qbit found |0>
	//         (2) number of 1st qbit found |1>
	//         (3) number of 1st qbit is same as 2nd
	operation TwoQBitsInEntanglementAndMeasure (count : Int, initial: Result) : (Int,Int,Int)
    {
        body (...)
        {
            mutable numOnes = 0;
			      mutable qbit1And2Agree = 0;
            using (qubits = Qubit[2])
            {
                for (test in 1..count)
                {
                    Set(initial, qubits[0]);
					          Set(Zero, qubits[1]);

                    //DumpMachine();
					          H(qubits[0]);
                    //DumpMachine();
					          CNOT(qubits[0], qubits[1]);	// if [0] is |1> then flip [1]
                    //DumpMachine();

                    let res1 = M (qubits[0]);
					          let res2 = M (qubits[1]);

                    // Count the number of ones we saw:
                    if (res1 == One)
                    {
                        set numOnes = numOnes + 1;
                    }

					          if (res1 == res2)
					          {
						          set qbit1And2Agree=qbit1And2Agree+1;
					          }
                }

                Set(Zero, qubits[0]);
        				Set(Zero, qubits[1]);
            }

            // Return number of times we saw a |0> and number of times we saw a |1>
            return (count-numOnes, numOnes,qbit1And2Agree);
        }
    }
}
