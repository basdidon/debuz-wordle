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

    char[] keys;
    public IReadOnlyList<char> Keys => keys;

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
    [SerializeField] List<string> guessWords;

    public int InputLineIdx => guessWords.Count;
    public int InputLetterIdx => InputWordSB.Length;

    public event Action<string> OnNewKeyword;
    public event Action<int, int, char> OnAddLetter;
    public event Action<int, int> OnRemoveLetter;
    public event Action<int, string,WordCorrectness[]> OnAcceptInputWord;
    public event Action<int,int> OnRejectInputWord;

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

        keys = new char[26];
        for (int i = 0; i < 26; i++)
        {
            char c = (char)(i + 'A');
            keys[i] = c;
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

    public void InputLetter(char c)
    {
        int length = InputWordSB.Length;
        if(length < 5)
        {
            int characterIdx = length;
            InputWordSB.Append(c);
            OnAddLetter?.Invoke(guessWords.Count,characterIdx,c);
        }
        else
        {
            OnRejectInputWord?.Invoke(guessWords.Count,5);
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
        Debug.Log("Submit");
        int length = InputWordSB.Length;

        if (length != 5)
        {
            OnRejectInputWord?.Invoke(guessWords.Count, InputWordSB.Length);
            return;
        }

        var correctnessResult = new WordCorrectness[5];
        string inputWord = InputWordSB.ToString().ToLower();  // caching string

        if (!IsValidWord(inputWord) || guessWords.Contains(inputWord))
        {
            OnRejectInputWord?.Invoke(guessWords.Count, 5);
            return;
        }

        Debug.Log($"{inputWord} : {keyword}");
        for (int i = 0; i < 5; i++)
        {
            // all keywords are lowercase
            correctnessResult[i] = inputWord[i] == keyword[i] ? WordCorrectness.CORRECT : WordCorrectness.INCORRECT;
        }

        for (int i = 0; i < 5; i++)
        {
            if (correctnessResult[i] == WordCorrectness.CORRECT)
                continue;

            for (int j = 0; j < 5; j++)
            {
                if (keyword[i] == inputWord[j] && correctnessResult[j] == WordCorrectness.INCORRECT)
                {
                    correctnessResult[j] = WordCorrectness.SPOT_INCORRECT;
                    break;
                }
            }  
        }

        OnAcceptInputWord?.Invoke(guessWords.Count, inputWord, correctnessResult);
        guessWords.Add(inputWord);
        InputWordSB.Clear();

        if (WinGameCheck(correctnessResult))
        {
            PlayerWinGame();
        }
        else if (LoseGameCheck())
        {
            PlayerLoseGame();
        }



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
        OnWinGame?.Invoke(keyword,guessWords.Count);
    }

    void PlayerLoseGame()
    {
        OnLoseGame?.Invoke(keyword);
    }

    /*
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
    */
}
