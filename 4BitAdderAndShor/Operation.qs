namespace Quantum._4BitAdderAndGrover
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Bitwise ;


	operation AdderCarry(carryIn:Qubit, a:Qubit,b:Qubit,carryOut:Qubit):()
	{
		body
		{
			CCNOT(a,b,carryOut);
			CNOT(a,b);
			CCNOT(carryIn,b,carryOut);
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}

	operation Sum(carry:Qubit,a: Qubit, b:Qubit) : ()
	{
	    body
        {
			CNOT(a,b);
			CNOT(carry,b);
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}



	/// <summary>	 
	/// We add two 4-bits number, given by a1,a2,a3,a4 and b1,b2,b3,b4 (where a1/b1 are the respective least-significant bits).
	/// On return, the sum is put into b1,b2,b3,b4,carry. carry is assumed to be in state |0> on input.
	/// See https://arxiv.org/ftp/quant-ph/papers/0206/0206028.pdf.
	/// </summary>
	operation FourBitAdder(a1: Qubit,a2: Qubit,a3: Qubit,a4: Qubit,b1: Qubit,b2: Qubit,b3: Qubit,b4: Qubit,carry:Qubit, tempQubits:Qubit[]):()
	{
		body
		{
			AdderCarry(tempQubits[0],a1,b1,tempQubits[1]);
			AdderCarry(tempQubits[1],a2,b2,tempQubits[2]);
			AdderCarry(tempQubits[2],a3,b3,tempQubits[3]);
			AdderCarry(tempQubits[3],a4,b4,carry);
			CNOT(a4,b4);
			Sum(tempQubits[3],a4,b4);
			(Adjoint AdderCarry)(tempQubits[2],a3,b3,tempQubits[3]);
			Sum(tempQubits[2],a3,b3);
			(Adjoint AdderCarry)(tempQubits[1],a2,b2,tempQubits[2]);
			Sum(tempQubits[1],a2,b2);
			(Adjoint AdderCarry)(tempQubits[0],a1,b1,tempQubits[1]);
			Sum(tempQubits[0],a1,b1);
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}
	
	operation FourBitAdderOracle(execHadamard:Bool,expectedSum : Int, idxMarkedQubit: Int, qubits: Qubit[], tmpQubitIndices:Int[]):()
	{
		body
		{
		    // That's the "result"-qubit, the one we have to set if our function argument is a "good one". We determine
		    // if it is a "good one" by calculate the function, and then compare the result with "expectedSum".
		    // Note that we have to undo all operations, so that the state is not modified (with the exception of the flagQubit,
		    // of course) by the oracle.
			let flagQubit = qubits[idxMarkedQubit];
			
			// "dataRegister" contains the qubits we care about, or those are the input-qubits for the oracle. Note that
			//  we also exclude the "carry"-qubit (the 5th qubit of the result) because it is not used as a input, it must
			//  be |0>.
			let dataRegister = Exclude([idxMarkedQubit;tmpQubitIndices[0];tmpQubitIndices[1];tmpQubitIndices[2];tmpQubitIndices[3];8], qubits);
			
			if (execHadamard)
			{
				ApplyToEachCA(H, dataRegister);
			}

			let tmpQubits=[qubits[tmpQubitIndices[0]];qubits[tmpQubitIndices[1]];qubits[tmpQubitIndices[2]];qubits[tmpQubitIndices[3]]];

			FourBitAdder(qubits[0],qubits[1],qubits[2],qubits[3],
						 qubits[4],qubits[5],qubits[6],qubits[7],qubits[8],tmpQubits);

			// result is in [4] -> [8]  (where [4] is the LSB)
			// If we expect a zero, then we need to invert the qubit (and undo the inversion afterwards)
			if (And(expectedSum,1)==0)	{X(qubits[4]);}
			if (And(expectedSum,2)==0)	{X(qubits[5]);}
			if (And(expectedSum,4)==0)	{X(qubits[6]);}
			if (And(expectedSum,8)==0)	{X(qubits[7]);}
			if (And(expectedSum,16)==0)	{X(qubits[8]);}
			
			// we "and" the result-qubits (which have been inverted in case we expect a zero), so the flag-qubit
			//  is only set if the result is the same as "expectedSum"
			MultiControlledXClean(
				[qubits[4];qubits[5];qubits[6];qubits[7];qubits[8]],
				flagQubit);
			
			if (And(expectedSum,1)==0)	{X(qubits[4]);}
			if (And(expectedSum,2)==0)	{X(qubits[5]);}
			if (And(expectedSum,4)==0)	{X(qubits[6]);}
			if (And(expectedSum,8)==0)	{X(qubits[7]);}
			if (And(expectedSum,16)==0)	{X(qubits[8]);}
			
			(Adjoint FourBitAdder)(qubits[0],qubits[1],qubits[2],qubits[3],
						            qubits[4],qubits[5],qubits[6],qubits[7],qubits[8],tmpQubits);
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}

	function GroverStatePrepOracle(expectedSum : Int,tmpQuBitIndices:Int[]) : StateOracle
    {
        return StateOracle(FourBitAdderOracle(true,expectedSum, _, _, tmpQuBitIndices));
    }


	function GroverSearch( expectedSum : Int, nIterations: Int, idxMarkedQubit: Int,tmpQuBitIndices:Int[]) : (Qubit[] => () : Adjoint, Controlled)
    {
        return AmpAmpByOracle(nIterations, GroverStatePrepOracle(expectedSum,tmpQuBitIndices), idxMarkedQubit);
    }

    operation Operation (expectedSum:Int, nIterations:Int) : (Result,Int)
    {
        body
        {
		    // Allocate variables to store measurement results.
            mutable resultSuccess = Zero;
            mutable resultValue = 0;

             // Allocate 14 qubits. These are all in the |0〉 state.
             // We have 9 qubits for the 4-bit-adder, one flag-qubit for the Grover-amplitude-amplification (used by the
             // oracle-function) and additional 4 ancillary qubits (required by the 4-bit-adder).
            using (qubits = Qubit[10+4]) 
			{
				(GroverSearch( expectedSum, nIterations, 9, [10;11;12;13]))(qubits);

				 // Measure the marked qubit. On success, this should be One.
                set resultSuccess = M(qubits[9]);

				// Measure the state of the database register post-selected on
                // the state of the marked qubit.
                let resultElement = MultiM(qubits);
				set resultValue = PositiveIntFromResultArr(resultElement);

				// This resets all qubits to the |0〉 state, which is required 
                // before deallocation.
                ResetAll(qubits);
			}

			// Returns the measurement results of the algorithm.
            return (resultSuccess, resultValue);
        }
    }

    /// <summary>
    /// The argument a and b are used as the inputs for the 4-bit adder. Only the lowest 4 bits of the respective argument
    /// are used. We prepare the qubits in a pure state with those bits, and calculate the 5-bit sum. The sum is 
    /// then returned.
    /// </summary>
	operation Full4BitAdder(a:Int,b:Int):(Int)
	{
		body
		{
			mutable result=0;
			using (qubits = Qubit[9+4])
			{
				let tempQubits=[qubits[9];qubits[10];qubits[11];qubits[12]];
				
				SetFromIntegerBits(a, [0;1;2;3], qubits);
				SetFromIntegerBits(b, [4;5;6;7], qubits);
				
				FourBitAdder(qubits[0],qubits[1],qubits[2],qubits[3],
								qubits[4],qubits[5],qubits[6],qubits[7],
								qubits[8],
								tempQubits);
								
                set result = PositiveIntFromResultArr(MultiM([qubits[4];qubits[5];qubits[6];qubits[7];qubits[8]]));									

				ResetAll(qubits);
			}

			return (result);
		}
	}

	/// <summary>
	/// We use the 4-Bit-Adder and use entangled qubits as input. We then make a measurement, and reconstruct one of the
	/// summands manually.
	/// The tuple returned contains: the sum, summand #1 and (the reconstructed) summand #2.
	/// </summary>
	operation Full4BitAdderEntangled():(Int, Int, Int)
	{
		body
		{
			mutable sum=0;
			mutable summand1=0;
			mutable summand2=0;
			using (qubits = Qubit[9+4])
			{
				let tempQubits=[qubits[9];qubits[10];qubits[11];qubits[12]];

				H(qubits[0]);H(qubits[1]);H(qubits[2]);H(qubits[3]);
				H(qubits[4]);H(qubits[5]);H(qubits[6]);H(qubits[7]);
				
				FourBitAdder(qubits[0],qubits[1],qubits[2],qubits[3],
								qubits[4],qubits[5],qubits[6],qubits[7],
								qubits[8],
								tempQubits);

				set sum = PositiveIntFromResultArr(MultiM([qubits[4];qubits[5];qubits[6];qubits[7];qubits[8]]));
				set summand1 = PositiveIntFromResultArr(MultiM([qubits[0];qubits[1];qubits[2];qubits[3]]));
				
				// The second summand was overwritten with the result. We can recover it by running the adjoint operation
				//  on the result. Note that the qubits are all in pure states since we just have measured them, so this 
				//  essentially is a classic operation (or: should not be done on qubits but on cbits).
				(Adjoint FourBitAdder)(qubits[0],qubits[1],qubits[2],qubits[3],
										qubits[4],qubits[5],qubits[6],qubits[7],
										qubits[8],
										tempQubits);
										
                set summand2 = PositiveIntFromResultArr(MultiM([qubits[4];qubits[5];qubits[6];qubits[7]]));															

				ResetAll(qubits);
			}

			return (sum,summand1,summand2);
		}
	}

	operation Test4BitAdderOracle(a:Int,b:Int,expectedSum:Int):(Int)
	{
		body
		{
			mutable result=0;
			mutable v=1;
			// qubit 9 is marker bit, qbuit 10-13 are ancillary quits, qubit 8 is carry bit which must be zero on input
			using (qubits = Qubit[14]) 
			{
				SetFromIntegerBits(a, [0;1;2;3], qubits);
				SetFromIntegerBits(b, [4;5;6;7], qubits);

				FourBitAdderOracle(false,expectedSum,9,qubits,[10;11;12;13]);

				set result= PositiveIntFromResultArr(MultiM(qubits));

				ResetAll(qubits);
			}

			return result;
		}
	}

	// ------------------------------------------------------------------------------------------------------------

	operation MultiControlledXClean ( controls : Qubit[] , target : Qubit ) : () 
	{
        body {
            let numControls = Length(controls);
            if( numControls == 0 ) {
                X(target);
            } elif( numControls == 1 ) {
                CNOT(Head(controls),target);
            } elif( numControls == 2 ) {
                CCNOT(controls[1],controls[0],target);
            } else {
                let multiNot = 
                    ApplyMultiControlledCA(
                        ApplyToFirstThreeQubitsCA(CCNOT, _), CCNOTop(CCNOT), _, _ );
                multiNot(Rest(controls),[Head(controls);target]);
            }
        }
        adjoint auto 
        controlled( extraControls ) {
            MultiControlledXClean( extraControls + controls, target );
        }
        controlled adjoint auto
    }
	
	operation SetFromIntegerBits(v: Int, indices: Int[],qubits:Qubit[]):()
	{
		body
		{
			mutable mask=1;
			for (i in 0..(Length(indices) - 1))
			{
				SetFromInteger(And(v,mask), qubits[indices[i]]);
				set mask=2*mask;
			}
		}
	}

	/// <summary> Set the qubit to |0> if value==0, and to |1> otherwise.</summary>
	operation SetFromInteger(value: Int, q: Qubit) : ()
    {
        body
        {
			if (value!=0)
			{
				Set(One,q);
			}
			else
			{
				Set(Zero,q);
			}
        }
    }

	operation Set(desired: Result, q: Qubit) : ()
    {
        body
        {
            let current = M(q);
            if (desired != current)
            {
                X(q);
            }
        }
    }
}
