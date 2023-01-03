using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RNG = UnityEngine.Random;

public class FourOperandsScript : ModuleScript
{

	public TextMesh[] operandUIs;
	private int[] operands;

	public TextMesh answerUI;
	private int answer = 0;

	private char[] intendedAnswer;

	public KMSelectable[] choosableOperators;
	public KMSelectable[] operatorHolders;

	public TextMesh[] operatorSlots;
	private char?[] chosenOperators = new char?[3];
	private int slotIndex = 0;

	public TextMesh[] allTM;
	[SerializeField]
	public static readonly Color defaultColor = new Color32(101, 142, 176, 255);
	public static readonly Color strikeColor = new Color32(214, 113, 113, 255);
	public static readonly Color solveColor = new Color32(129, 214, 113, 255);

	private static readonly char[] possibleOperators = new char[] { '+', '-', '×', '÷' };
	private static readonly char[] TPOperators = new char[] { '+', '-', '*', '/' };

	private bool isUserInputPossible = true;

	// Use this for initialization
	void Start()
	{
		GenerateAnswer();
		choosableOperators.Assign(onInteract: (i) => { if (IsSolved || !isUserInputPossible) return; SelectOperator(i); });
		operatorHolders.Assign(onInteract: (i)=> { if (IsSolved || !isUserInputPossible) return; RevertOperator(i); });
	}

	private void RevertOperator(int op)
	{
		PlaySound("enihs" + op);
		chosenOperators[op] = null;
		operatorSlots[op].text = "▢";
		ChangeIndex();
	}

	private void ChangeIndex()
	{
		if (chosenOperators.All(x => x != null)) slotIndex = 3;
		else slotIndex = chosenOperators.IndexOf(x => x == null);
	}

	private void SelectOperator(int op)
	{
		PlaySound("shine" + slotIndex);
		chosenOperators[slotIndex] = possibleOperators[op];
		operatorSlots[slotIndex].text = chosenOperators[slotIndex].ToString();
		ChangeIndex();
		if (slotIndex == 3) CheckAnswer();
	}

	private void CheckAnswer()
	{
		Log("Submitted answer:");
		if (CalculateAnswer(chosenOperators.WhereNotNull().ToArray()) == answer)
		{
			Log("Result is the same. Module solved.");
			Solve();
			PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
			allTM.ForEach(tm => tm.color = solveColor);
		}
		else
		{
			isUserInputPossible = false;
			Log("Not the same result. Strike occured.");
			Strike();
			StartCoroutine(ResetAnswer());
		}
	}

	private IEnumerator ResetAnswer()
	{
		allTM.ForEach(tm => tm.color = strikeColor);
		yield return new WaitForSeconds(2f);
		for (int i = chosenOperators.Length - 1; i >= 0; i--)
		{
			RevertOperator(i);
			yield return new WaitForSeconds(.5f);
		}
		allTM.ForEach(tm => tm.color = defaultColor);
		isUserInputPossible = true;
	}

	private void GenerateAnswer()
	{
		operands = new int[4];
		do
		{
			for (int i = 0; i < operandUIs.Length; i++)
			{
				operands[i] = RNG.Range(2, 10);
				operandUIs[i].text = operands[i].ToString();
			}
			intendedAnswer = new char[3];

			for (int i = 0; i < intendedAnswer.Length; i++)
			{
				intendedAnswer[i] = possibleOperators[RNG.Range(0, ((i == intendedAnswer.Length - 1) ? possibleOperators.Length - 1 : possibleOperators.Length))]; //Divide won't appear as last operator because it's lame (Division slander)
			}
			Log("Generated result:");
			answer = CalculateAnswer(intendedAnswer);
		}
		while (answer == -1); //No division by 0 should be allowed in the intended answer
		answerUI.text = answer.ToString();
	}

	private int CalculateAnswer(char[] operators)
	{
		int res = 0;
		for (int i = 0; i < operators.Length; i++)
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
					if(Math.Min(previousTerm, operands[i + 1]) == 0)
                    {
						Log("Division by zero. That's bad...");
						return -1;
                    }
					res = Math.Max(previousTerm, operands[i + 1]) / Math.Min(previousTerm, operands[i + 1]);
					log += "{0}/{1}={2}".Form(Math.Max(previousTerm, operands[i + 1]), Math.Min(previousTerm, operands[i + 1]), res);
					break;
			}
			Log(log);
		}
		Log("Full equation: {0}{1}{2}{3}{4}{5}{6}={7}", operands[0], operators[0], operands[1], operators[1], operands[2], operators[2], operands[3], res);
		return res;
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"[!{0} set/submit/press #] to set an operator. You can set up to 3 at the same time, and they will be set in reading order in the empty slots. Input operators using the following characters : +,-,*,/. [!{0} unset/remove #] will remove the operator in the #th place. You can remove multiple operators in the same command.";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
	{
		string[] commands = command.Split(" ");
		if (commands.Length >= 2 && commands.Length <= 4)
		{
			if (new string[] { "set", "submit", "contains" }.Contains(commands[0]))
			{
				if (commands.Skip(1).All(o => o.Length == 1 && TPOperators.Contains(o[0])))
				{
					yield return null;
					yield return new WaitUntil(() => isUserInputPossible);
					foreach (int cPos in commands.Skip(1).Select(o => TPOperators.IndexOf(o[0])))
					{
						choosableOperators[cPos].OnInteract();
						yield return new WaitForSeconds(.25f);
					}
				}
			}
			else if (new string[] { "remove", "unset" }.Contains(commands[0]))
			{
				if (commands.Skip(1).All(i => Enumerable.Range(1, 3).Select(a => a.ToString()).Contains(i)))
				{
					yield return null;
					yield return new WaitUntil(() => isUserInputPossible);
					foreach (int oPos in commands.Skip(1).Select(i => int.Parse(i) - 1))
					{
						operatorHolders[oPos].OnInteract();
					}
				}
			}
		}
	}

	private IEnumerator TwitchHandleForcedSolve()
	{
		yield return new WaitUntil(() => isUserInputPossible);
		for(int i = 0; i < chosenOperators.Length; i++)
        {
			if(chosenOperators[i]!=null && chosenOperators[i] != intendedAnswer[i])
            {
				RevertOperator(i);
				yield return new WaitForSeconds(.25f);
            }
        }
        while (!IsSolved)
        {
			choosableOperators[possibleOperators.IndexOf(intendedAnswer[slotIndex])].OnInteract();
			yield return new WaitForSeconds(.25f);
        }
	}
}


