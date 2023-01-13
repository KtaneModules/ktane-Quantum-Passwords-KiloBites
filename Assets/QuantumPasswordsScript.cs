using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;

public class QuantumPasswordsScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] arrows;
	public KMSelectable submit;

	public Material[] colors;
	public Material[] moduleColors;
	public MeshRenderer[] grid;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;
	private bool isActivated;
	private bool pause;

	private static readonly string[] words = { "Argue", "Blaze", "Cajun", "Depth", "Endow", "Foyer", "Gimpy", "Heavy", "Index", "Joker", "Kylix", "Lambs", "Mercy", "Nifty", "Omens", "Pupil", "Risky", "Stoic", "Taboo", "Unbox", "Viced", "Waltz", "Xerus", "Yuzus", "Zilch" };
	private static readonly int[] values = { 3, 1, 4, 1, 5, 4, 2, 1, 5, 3, 5, 3, 4, 2, 4, 5, 3, 4, 2, 4, 5, 3, 4, 2, 4 };
	private string[] selectedWords = new string[2];
	private int[] selectedValues = new int[2];
	private bool[][][] letterPatterns = new bool[2][][];
	private bool[][] alphabetPatterns = new string[]
	{
		".xxx.x...xxxxxxx...xx...x", // A
		"xxxx.x...xxxxx.x...xxxxx.", // B
		".xxxxx....x....x.....xxxx", // C
		"xxxx.x...xx...xx...xxxxx.", // D
		"xxxxxx....xxxxxx....xxxxx", // E
		"xxxxxx....xxxxxx....x....", // F
		".xxxxx....x..xxx...x.xxx.", // G
		"x...xx...xxxxxxx...xx...x", // H
		"xxxxx..x....x....x..xxxxx", // I
		"....x....x....xx...x.xxx.", // J
		"x...xx..x.xxx..x..x.x...x", // K
		"x....x....x....x....xxxxx", // L
		"x...xxx.xxx.x.xx...xx...x", // M
		"x...xxx..xx.x.xx..xxx...x", // N
		".xxx.x...xx...xx...x.xxx.", // O
		"xxxx.x...xxxxx.x....x....", // P
		"xxxxxx...xxxxxxx..x.x...x", // R
		"xxxxxx....xxxxx....xxxxxx", // S
		"xxxxx..x....x....x....x..", // T
		"x...xx...xx...xx...x.xxx.", // U
		"x...xx...x.x.x..x.x...x..", // V
		"x...xx...xx...xx.x.x.x.x.", // W
		"x...x.x.x...x...x.x.x...x", // X
		"x...xx...x.xxx...x....x..", // Y
		"xxxxx....x.xxx.x....xxxxx"  // Z
	}.Select(grid => grid.Select(cell => cell == 'x').ToArray()).ToArray();

	private int ix = 0;
	private int correctPos;

	void Awake()
    {

		moduleId = moduleIdCounter++;

		foreach (KMSelectable arrow in arrows)
		{
			arrow.OnInteract += delegate () { arrowPress(arrow); return false; };
		}

		submit.OnInteract += delegate () { submitPress(); return false; };

		Module.OnActivate += onActivate;
    }

	void onActivate()
	{
		isActivated = true;
		displayThings();
		determinePos();
	}

	private string shiftingText(string s, int count)
	{
		return s.Remove(0, count) + s.Substring(0, count);
	}


	void Start()
	{
		List<string> wordList = words.ToList();
		List<int> numList = values.ToList();

		for (var i = 0; i < 2; i++)
		{
			var gen = rnd.Range(0, wordList.Count);
			selectedWords[i] = wordList[gen];
			selectedValues[i] = values[gen];
			wordList.RemoveAt(gen);
			numList.RemoveAt(gen);
		}

		Debug.LogFormat("[Quantum Passwords #{0}] The words selected are: {1}", moduleId, selectedWords.Join(", "));

		for (int i = 0; i < 2; i++)
		{
			selectedWords[i] = shiftingText(selectedWords[i], rnd.Range(1, selectedWords[i].Count())); 
		}

        Debug.LogFormat("[Quantum Passwords #{0}] After shifting the words: {1}", moduleId, selectedWords.Join(", ").ToUpperInvariant());

        for (int i = 0; i < 2; i++)
		{
			letterPatterns[i] = new bool[5][];

			selectedWords[i] = selectedWords[i].ToUpperInvariant();

			for (int j = 0; j < 5; j++)
			{
				letterPatterns[i][j] = alphabetPatterns["ABCDEFGHIJKLMNOPRSTUVWXYZ".IndexOf(selectedWords[i][j])];
			}
		}
    }

	void determinePos()
	{
		var parOne = selectedValues[0] % 2;
		var parTwo = selectedValues[1] % 2;



		correctPos = parOne == parTwo ? Math.Min(selectedValues[0], selectedValues[1]) : Math.Max(selectedValues[0], selectedValues[1]);

		Debug.LogFormat("[Quantum Passwords #{0}] Both word's values {1}. Submit position {2}.", moduleId, parOne == parTwo ? "share the same parity" : "don't share the same parity", correctPos);
	}

	void submitPress()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

		if (moduleSolved || !isActivated || pause)
		{
			return;
		}

		if (ix == (correctPos - 1))
		{
			StartCoroutine(solveAnimation());
		}
		else
		{
			StartCoroutine(strikeAnimation());
		}
	}

	void arrowPress(KMSelectable arrow)
	{
		arrow.AddInteractionPunch(0.4f);

		if (moduleSolved || !isActivated || pause)
		{
			return;
		}

		for (int i = 0; i < 2; i++)
		{

			if (arrow == arrows[i])
			{
				switch (i)
				{
					case 0:
						Audio.PlaySoundAtTransform("Left", transform);
						ix--;
						break;
					case 1:
                        Audio.PlaySoundAtTransform("Right", transform);
                        ix++;
						break;
				}
				if (ix < 0)
				{
					ix = 0;
				}
				else if (ix > 4)
				{
					ix = 4;
				}
			}
		}

		displayThings();
	}

	void displayThings()
	{
        for (int i = 0; i < 25; i++)
        {
            grid[i].material = letterPatterns[0][ix][i] && letterPatterns[1][ix][i] ? colors[2] : letterPatterns[0][ix][i] || letterPatterns[1][ix][i] ? colors[1] : colors[0];
        }
    }

	IEnumerator solveAnimation()
	{
		yield return null;
		pause = true;

		string solveText = "YOUDIDIT";

		bool[][] solvePattern = new bool[8][];

		for (int i = 0; i < 8; i++)
		{
			solvePattern[i] = alphabetPatterns["ABCDEFGHIJKLMNOPRSTUVWXYZ".IndexOf(solveText[i])];
		}

		Debug.LogFormat("[Quantum Passwords #{0}] Position has been submitted correctly! Solved!", moduleId);

		Audio.PlaySoundAtTransform("Solve", transform);
		for (int i = 0; i < 25; i++)
		{
			grid[i].material = moduleColors[1];
			yield return new WaitForSeconds(0.01785f);
		}
		for (int i = 0; i < 25; i++)
		{
			grid[i].material = colors[2];
		}
		yield return new WaitForSeconds(0.238f);
		pause = false;
		moduleSolved = true;
		Module.HandlePass();
		int step = 0;
		while (step != 2)
		{
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    grid[j].material = solvePattern[i][j] ? moduleColors[1] : colors[0];
                }
				yield return new WaitForSeconds(0.238f);
				
				for (int k = 0; k < 25; k++)
				{
					grid[k].material = colors[1];
				}

				yield return new WaitForSeconds(0.238f);
            }
			step++;
        }

		for (int i = 0; i < 25; i++)
		{
			grid[i].material = moduleColors[1];
		}

	}

	IEnumerator strikeAnimation()
	{
		yield return null;
		pause = true;
		bool[] strikePattern = alphabetPatterns[22];
		var step = 0;

		Module.HandleStrike();
		Debug.LogFormat("[Quantum Passwords #{0}] Submitted position {1} when it expects position {2}. Strike!", moduleId, (ix + 1).ToString(), correctPos);
		while (step != 3)
		{
			for (int i = 0; i < 25; i++)
			{
				grid[i].material = strikePattern[i] ? moduleColors[0] : colors[0];
			}
			yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < 25; i++)
            {
                grid[i].material = colors[0];
            }
			yield return new WaitForSeconds(0.2f);
			step++;
        }
		displayThings();
		pause = false;
	}
	
	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} cycle to cycle through all grids. | !{0} submit 12345 to submit the position of the grid.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		yield return null;
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		var cycleActive = false;

		if (!isActivated || pause)
		{
			yield return !isActivated ? "sendtochaterror The module isn't activated yet!" : "sendtochaterror You cannot interact with the module at this time!";
			yield break;
		}

		if (split[0].EqualsIgnoreCase("CYCLE"))
		{
			if (cycleActive)
			{
				yield return "sendtochaterror You cannot interact with the module while it's cycling!";
				yield break;
			}
			cycleActive = true;

			if (ix != 0)
			{
                while (ix != 0)
                {
                    arrows[0].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
				yield return new WaitForSeconds(1.5f);
            }

			
			while (ix < 4)
			{
				arrows[1].OnInteract();
				yield return new WaitForSeconds(1.5f);
			}
			while (ix > 0)
			{
				arrows[0].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			cycleActive = false;
			yield break;
		}

		if (split[0].EqualsIgnoreCase("SUBMIT"))
		{
			if (split.Length == 1)
			{
				yield return "sendtochaterror Please specifiy the position you want to submit!";
				yield break;
			}
			else if (split[1].Length > 1)
			{
				yield return "sendtochaterror Please specify only 1 digit please!";
				yield break;
			}
			else if (!"12345".Contains(split[1]))
			{
				yield return "sendtochaterror " + split[1] + " is not a valid number!";
				yield break;
			}
			int temp;
			int.TryParse(split[1], out temp);

			while (ix != (temp - 1))
			{
				if (ix > (temp - 1))
				{
					arrows[0].OnInteract();		
				}
				else if (ix < (temp - 1))
				{
					arrows[1].OnInteract();
				}
                yield return new WaitForSeconds(0.1f);
            }
			submit.OnInteract();
		}
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;

		while (!isActivated || pause)
		{
			yield return true;
		}

		while (ix != (correctPos - 1))
		{
			if (ix > (correctPos - 1))
			{
				arrows[0].OnInteract();
			}
			else if (ix < (correctPos - 1))
			{
				arrows[1].OnInteract();
			}
			yield return new WaitForSeconds(0.1f);
		}
		submit.OnInteract();
    }


}





