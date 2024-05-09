using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum WordCorrectness
{
    DEFAULT,
    INCORRECT,
    SPOT_INCORRECT,
    CORRECT
}

public class WordleController : MonoBehaviour
{
    public static WordleController Instance { get; private set; }

    string[] words;

    string keyword;
    string Keyword
    {
        get => keyword;
        set
        {
            keyword = value;
            OnNewKeyword?.Invoke(Keyword);
        }
    }

    StringBuilder InputWordSB { get; set; }
    List<string> guessWords;

    public int InputLineIdx => guessWords.Count;
    public int InputLetterIdx => InputWordSB.Length;

    public event Action<string> OnNewKeyword;
    public event Action<int, int, char> OnAddLetter;
    public event Action<int, int> OnRemoveLetter;
    public event Action<int, string,WordCorrectness[]> OnAcceptInputWord;
    public event Action<int, int> OnSubmitNotCompleteInputWord;
    public event Action<int> OnRejectInputWord;

    public event Action<string, int> OnWinGame;
    public event Action<string> OnLoseGame;
    public event Action OnStartOver;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Debug.Log(File.Exists("Assets/valid-wordle-words.txt"));

        words = File.ReadAllLines("Assets/valid-wordle-words.txt");

        InputWordSB = new(5);
        guessWords = new();
        RandomKeyword();
    }

    public void StartOver()
    {
        InputWordSB.Clear();
        guessWords.Clear();
        RandomKeyword();
        OnStartOver?.Invoke();
        Debug.Log("Start Over");
    }

    void RandomKeyword()
    {
        Keyword = words[UnityEngine.Random.Range(0, words.Length)];
    }

    public bool InputLetter(char c)
    {
        int length = InputWordSB.Length;
        if(length < 5)
        {
            int characterIdx = length;
            InputWordSB.Append(c);
            OnAddLetter?.Invoke(guessWords.Count,characterIdx,c);
            return true;
        }
        else
        {
            Debug.Log("full");
            return true;
        }

    }

    public void RemoveLetter()
    {
        int length = InputWordSB.Length;
        if (length > 0)
        {
            var characterIdx = length - 1;
            InputWordSB.Remove(characterIdx,1);
            OnRemoveLetter?.Invoke(guessWords.Count, characterIdx);
        }
    }

    bool IsValidWord(string input)
    {
        foreach (var word in words)
        {
            if (word == input)
            {
                return true;
            }
        }
        return false;
    }

    public void SubmitInputWord()
    {
        int length = InputWordSB.Length;

        if (length == 5)
        {
            Debug.Log("Submit");
            var correctnessResult = new WordCorrectness[5];
            string wordString = InputWordSB.ToString().ToLower();  // caching string

            if (!IsValidWord(wordString) || guessWords.Contains(wordString))
            {
                OnRejectInputWord?.Invoke(guessWords.Count);
                return;
            }


            for (int i = 0; i < 5; i++)
            {
                if (wordString[i] == keyword[i]) // all keywords are lowercase
                {
                    correctnessResult[i] = WordCorrectness.CORRECT;
                }
            }
            DebugLetterCorrectResult("Correct check: ",correctnessResult);

            for (int i = 0; i < 5; i++)
            {
                if (correctnessResult[i] == WordCorrectness.CORRECT)
                    continue;

                for (int j = 0; j < 5; j++)
                {
                    if(keyword[i] == wordString[j] && correctnessResult[j] == WordCorrectness.DEFAULT)
                    {
                        correctnessResult[j] = WordCorrectness.SPOT_INCORRECT;
                        break;
                    }
                }
            }
            DebugLetterCorrectResult("SpotIncorrect check: ", correctnessResult);

            for (int i =0;i < 5; i++)
            {
                if(correctnessResult[i] == WordCorrectness.DEFAULT)
                {
                    correctnessResult[i] = WordCorrectness.INCORRECT;
                }
            }
            DebugLetterCorrectResult("incorrect check : ", correctnessResult);

            OnAcceptInputWord?.Invoke(guessWords.Count, wordString, correctnessResult);
            guessWords.Add(wordString);
            InputWordSB.Clear();

            if (WinGameCheck(correctnessResult))
            {
                PlayerWinGame();
            }else if (LoseGameCheck())
            {
                PlayerLoseGame();   
            }
        }
        else
        {
            OnSubmitNotCompleteInputWord?.Invoke(guessWords.Count, InputWordSB.Length);
        }
    }

    void DebugLetterCorrectResult(string prefix, WordCorrectness[] correctnessResult)
    {
        StringBuilder sb = new();
        sb.Append(prefix);

        for(int i = 0;i < 5; i++)
        {
            sb.Append($"{correctnessResult[i]} ,");
        }

        Debug.Log(sb.ToString());
    }

    bool WinGameCheck(WordCorrectness[] correctnessResult)
    {
        for(int i = 0; i < 5; i++)
        {
            if(correctnessResult[i] != WordCorrectness.CORRECT)
            {
                return false;
            }
        }

        return true;
    }

    bool LoseGameCheck() => guessWords.Count >= 6;

    void PlayerWinGame()
    {
        Debug.Log("end game");
        OnWinGame?.Invoke(keyword,guessWords.Count);
    }

    void PlayerLoseGame()
    {
        OnLoseGame?.Invoke(keyword);
    }

}
