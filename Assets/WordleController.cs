using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum LetterCorrectResult
{
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
            OnNewKeyword(Keyword);
        }
    }

    List<char> inputWord;
    List<string> guessWords;

    public event Action<string> OnNewKeyword;
    public event Action<int, int, char> OnAddLetter;
    public event Action<int, int> OnRemoveLetter;
    public event Action<int,LetterCorrectResult[]> OnSubmitInputWord;

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

        inputWord = new();
        guessWords = new();

        RandomKeyword();
    }

    void RandomKeyword()
    {
        Keyword = words[UnityEngine.Random.Range(0, words.Length)];
    }

    public bool InputLetter(char c)
    {
        if(inputWord.Count < 5)
        {
            int characterIdx = inputWord.Count;
            inputWord.Add(c);
            OnAddLetter(guessWords.Count(),characterIdx,c);
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
        if(inputWord.Count > 0)
        {
            var characterIdx = inputWord.Count -1;
            inputWord.RemoveAt(characterIdx);
            OnRemoveLetter(guessWords.Count(), characterIdx);
        }
    }

    public bool SubmitInputWord()
    {
        if (inputWord.Count == 5)
        {
            Debug.Log("Submit");
            var result = new LetterCorrectResult[5];
            string wordString = string.Empty;
            for (int i = 0; i < result.Length; i++)
            {
                if (char.ToUpper(inputWord[i]) == char.ToUpper(keyword[i]))
                {
                    result[i] = LetterCorrectResult.CORRECT;
                }
                else if (keyword.Contains(char.ToLower(inputWord[i])))
                {
                    result[i] = LetterCorrectResult.SPOT_INCORRECT;
                }
                else
                {
                    result[i] = LetterCorrectResult.INCORRECT;
                }

                wordString += inputWord[i];
            }

            OnSubmitInputWord(guessWords.Count, result);
            guessWords.Add(wordString);
            inputWord.Clear();

            if (result.All(i => i == LetterCorrectResult.CORRECT))
            {
                PlayerWinGame();
            }
            else if (guessWords.Count >= 6)
            {
                PlayerLoseGame();
            }

            return true;
        }

        return false;
    }

    void PlayerWinGame()
    {
        Debug.Log("You are Winner");
    }

    void PlayerLoseGame()
    {
        Debug.Log("You are Loser");
    }
}
