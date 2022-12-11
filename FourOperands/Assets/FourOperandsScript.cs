using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RNG = UnityEngine.Random;

public class FourOperandsScript : ModuleScript {

	public TextMesh[] operandUIs;
    private int[] operands;

	public TextMesh answerUI;
	private int answer = 0;

	private char[] intendedAnswer;

	public KMSelectable[] choosableOperators;

	public TextMesh[] operatorSlots;
	private char[] chosenOperators = new char[3];
	private int slotIndex = 0;

	private static readonly char[] possibleOperators = new char[] { '+', '-', '×', '÷' };

	// Use this for initialization
	void Start () {

		GenerateAnswer();
		choosableOperators.Assign(onInteract: SelectOperator);
	}

    private void SelectOperator(int op)
    {
		Log(op);
		chosenOperators[slotIndex] = possibleOperators[op];
		operatorSlots[slotIndex].text = chosenOperators[slotIndex].ToString();
		if (++slotIndex == 3) slotIndex = 0;
    }

    private void GenerateAnswer()
    {
		operands = new int[4];
		for(int i = 0; i < operandUIs.Length; i++)
        {
			operands[i] = RNG.Range(1, 10);
			operandUIs[i].text = operands[i].ToString();
        }
		intendedAnswer = new char[3];
		
		for(int i = 0; i < intendedAnswer.Length; i++)
        {
			intendedAnswer[i] = possibleOperators[RNG.Range(0, possibleOperators.Length)];
        }
		answer = CalculateAnswer(intendedAnswer);
		answerUI.text = answer.ToString();
    }

    private int CalculateAnswer(char[] operators)
    {
		int res = 0;
		for(int i = 0; i < operators.Length; i++)
        {
			string log = "Step {0} is : ".Form(i + 1);
			int previousTerm = i == 0 ? operands[0] : res;
            switch (operators[i])
            {
				case '+':
					res = previousTerm + operands[i + 1];
					log += "{0}+{1}={2}".Form(previousTerm, operands[i + 1], res);
					break;
				case '-':
					res = Math.Max(previousTerm, operands[i + 1]) - Math.Min(previousTerm, operands[i + 1]);
					log += "{0}-{1}={2}".Form(Math.Max(previousTerm, operands[i + 1]), Math.Min(previousTerm, operands[i + 1]), res);
					break;
				case '×':
					res = previousTerm * operands[i + 1];
					log += "{0}*{1}={2}".Form(previousTerm, operands[i + 1], res);
					break;
				case '÷':
					res = Math.Max(previousTerm, operands[i + 1]) / Math.Min(previousTerm, operands[i + 1]);
					log += "{0}/{1}={2}".Form(Math.Max(previousTerm, operands[i + 1]), Math.Min(previousTerm, operands[i + 1]), res);
					break;
			}
			Log(log);
        }
		return res;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
