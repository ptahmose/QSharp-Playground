namespace Quantum._4BitAdderAndShor
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Bitwise ;

	operation Set (desired: Result, q1: Qubit) : ()
    {
        body
        {
            let current = M(q1);
            if (desired != current)
            {
                X(q1);
            }
        }
    }

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
	/// We add two 4-bits number, given by a1,a2,a3,a4 and b1,b2,b3,b4 (where a1/b1 are the least-significant bits).
	/// On return, the sum is put into b1,b2,b3,b4,carry.
	/// </summary>
	operation FourBitAdder(a1: Qubit,a2: Qubit,a3: Qubit,a4: Qubit,b1: Qubit,b2: Qubit,b3: Qubit,b4: Qubit,carry:Qubit, tempQubits:Qubit[]):()
	{
		body
		{
			//using (tempQubits=Qubit[4])
			//{
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
			//}
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}
	
	operation FourBitAdderOracle(expectedSum : Int, idxMarkedQubit: Int, startQubits: Qubit[], tmpQubitIndices:Int[]):()
	{
		body
		{
			let flagQubit = startQubits[idxMarkedQubit];
            //let dataRegister = Exclude([idxMarkedQubit], startQubits);
			let dataRegister = Exclude([idxMarkedQubit;tmpQubitIndices[0];tmpQubitIndices[1];tmpQubitIndices[2];tmpQubitIndices[3];8], startQubits);
			ApplyToEachCA(H, dataRegister);
			let tmpQubits=[startQubits[tmpQubitIndices[0]];startQubits[tmpQubitIndices[1]];startQubits[tmpQubitIndices[2]];startQubits[tmpQubitIndices[3]]];

			FourBitAdder(startQubits[0],startQubits[1],startQubits[2],startQubits[3],
						 startQubits[4],startQubits[5],startQubits[6],startQubits[7],startQubits[8],tmpQubits);

			// result is in [8] -> [4]  (where [4] is the LSB)
			// look for [4]=0, [5]=1, [6]=0, [7]=0, [8]=0
			X(startQubits[4]);X(startQubits[6]);X(startQubits[7]);X(startQubits[8]);
			//X(startQubits[tmpQubitIndices[0]]);X(startQubits[tmpQubitIndices[1]]);X(startQubits[tmpQubitIndices[2]]);X(startQubits[tmpQubitIndices[3]]);
			//MultiControlledXClean(
			//	[dataRegister[4];dataRegister[5];dataRegister[6];dataRegister[7];dataRegister[8];
			//	startQubits[tmpQubitIndices[0]];startQubits[tmpQubitIndices[1]];startQubits[tmpQubitIndices[2]];startQubits[tmpQubitIndices[3]]],
			//	flagQubit);
			MultiControlledXClean(
				[startQubits[4];startQubits[5];startQubits[6];startQubits[7];startQubits[8]],
				flagQubit);
			
			X(startQubits[4]);X(startQubits[6]);X(startQubits[7]);X(startQubits[8]);
			//X(startQubits[tmpQubitIndices[0]]);X(startQubits[tmpQubitIndices[1]]);X(startQubits[tmpQubitIndices[2]]);X(startQubits[tmpQubitIndices[3]]);
			(Adjoint FourBitAdder)(startQubits[0],startQubits[1],startQubits[2],startQubits[3],
						 startQubits[4],startQubits[5],startQubits[6],startQubits[7],startQubits[8],tmpQubits);
		}

		adjoint auto
		controlled auto
        adjoint controlled auto
	}

	function GroverStatePrepOracle(expectedSum : Int,tmpQuBitIndices:Int[]) : StateOracle
    {
        return StateOracle(FourBitAdderOracle(expectedSum, _, _, tmpQuBitIndices));
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

             // Allocate 10 qubits. These are all in the |0〉
            // state.
            using (qubits = Qubit[10+4]) 
			{
				(GroverSearch( expectedSum, nIterations, 9,[10;11;12;13]))(qubits);

				 // Measure the marked qubit. On success, this should be One.
                set resultSuccess = M(qubits[9]);

				// Measure the state of the database register post-selected on
                // the state of the marked qubit.
                let resultElement = MultiM(qubits);
				set resultValue = PositiveIntFromResultArr(resultElement);

				    // These reset all qubits to the |0〉 state, which is required 
                // before deallocation.
                ResetAll(qubits);
			}

			// Returns the measurement results of the algorithm.
            return (resultSuccess, resultValue);
        }
    }

	operation Full4BitAdder(a:Int,b:Int):(Int)
	{
		body
		{
			mutable result=0;
			using (qubits = Qubit[9+4])
			{
				let tempQubits=[qubits[9];qubits[10];qubits[11];qubits[12]];
				SetFromInteger(And(a,1), qubits[0]);
				SetFromInteger(And(a,2), qubits[1]);
				SetFromInteger(And(a,4), qubits[2]);
				SetFromInteger(And(a,8), qubits[3]);
				SetFromInteger(And(b,1), qubits[4]);
				SetFromInteger(And(b,2), qubits[5]);
				SetFromInteger(And(b,4), qubits[6]);
				SetFromInteger(And(b,8), qubits[7]);
				FourBitAdder(qubits[0],qubits[1],qubits[2],qubits[3],
								qubits[4],qubits[5],qubits[6],qubits[7],
								qubits[8],
								tempQubits);
				let s0=M(qubits[4]);
				let s1=M(qubits[5]);
				let s2=M(qubits[6]);
				let s3=M(qubits[7]);
				let s4=M(qubits[8]);
				if (s0==One)
				{
					set result=Or(result,1);
				}
				if (s1==One)
				{
					set result=Or(result,2);
				}
				if (s2==One)
				{
					set result=Or(result,4);
				}
				if (s3==One)
				{
					set result=Or(result,8);
				}
				if (s4==One)
				{
					set result=Or(result,16);
				}

				ResetAll(qubits);
			}

			return (result);
		}
	}

	operation Test4BitAdderOracle(a:Int):(Int)
	{
		body
		{
			mutable result=0;
			mutable v=1;
			using (qubits = Qubit[13]) 
			{
				for (i in 0..(Length(qubits) - 1))
				{
					SetFromInteger(And(a,v), qubits[i]);
					set v=v+v;
				}

				//let tempQubits=[qubits[9];qubits[10];qubits[11];qubits[12]];
				let tempQubits=[qubits[12];qubits[11];qubits[10];qubits[9]];

				FourBitAdder(qubits[0],qubits[1],qubits[2],qubits[3],
								qubits[4],qubits[5],qubits[6],qubits[7],
								qubits[8],
								tempQubits);

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

	operation SetFromInteger(desired: Int, q1: Qubit) : ()
    {
        body
        {
            let current = M(q1);
			if (desired!=0)
			{
				if (current!=One)
				{
					X(q1);
				}
			}
			else
			{
				if (current!=Zero)
				{
					X(q1);
				}
			}
        }
    }
}
