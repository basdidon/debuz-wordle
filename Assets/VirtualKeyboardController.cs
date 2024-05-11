using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class VirtualKeyboardController : MonoBehaviour
{
    WordleController WordleController { get; set; }
    Dictionary<char, WordCorrectness> CorrectnessStates { get; set; }
    Dictionary<char, Button> KeyButtons { get; set; }
    Button EnterBtn { get; set; }
    Button BackspaceBtn { get; set; }

    VisualElement container;

    private void Awake()
    {
        WordleController = WordleController.Instance;

        CorrectnessStates = new(26);
        KeyButtons = new(26);

        container = GetComponent<UIDocument>().rootVisualElement.Q("virtual-keyboard");
        BindElements();

        WordleController.OnAcceptInputWord += OnAcceptInputWordHandle;
        WordleController.OnWinGame += (_, _) => DisableInput();
        WordleController.OnLoseGame += _ => DisableInput();
        WordleController.OnStartOver += OnStartOverHandle;

    }

    void BindElements()
    {
        foreach(var key in WordleController.Keys) 
        {
            char lowerKey = char.ToLower(key);
            var btn = container.Q<Button>($"{lowerKey}-btn");
            if (btn == null)
                Debug.Log(key);
            btn.clicked += () => WordleController.InputLetter(key);

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");

            btn.focusable = false;

            CorrectnessStates.Add(key, WordCorrectness.DEFAULT);
            KeyButtons.Add(key, btn);
        }

        EnterBtn = container.Q<Button>("enter-btn");
        EnterBtn.clicked += WordleController.SubmitInputWord;
        EnterBtn.focusable = false;

        BackspaceBtn = container.Q<Button>("backspace-btn");
        BackspaceBtn.clicked += WordleController.RemoveLetter;
        BackspaceBtn.focusable = false;
    }

    private void OnEnable() => EnableInput();
    private void OnDisable() => DisableInput();

    void OnAcceptInputWordHandle(int lineIdx,string inputWord,WordCorrectness[] wordCorrect)
    {
        for(int i = 0; i < 5; i++)
        {
            SetCorrectnessState(inputWord[i], wordCorrect[i]);
        }
    }

    void SetCorrectnessState(char c, WordCorrectness correctnessState)
    {
        char upperKey = char.ToUpper(c);

        if (!CorrectnessStates.ContainsKey(upperKey) || CorrectnessStates[upperKey] > correctnessState)
            return;

        CorrectnessStates[upperKey] = correctnessState;

        if (!KeyButtons.ContainsKey(upperKey))
            return;

        var btn = KeyButtons[upperKey];

        btn.RemoveFromClassList("key-btn--incorrect");
        btn.RemoveFromClassList("key-btn--spot-incorrect");
        btn.RemoveFromClassList("key-btn--correct");

        if (correctnessState == WordCorrectness.INCORRECT)
        {
            btn.AddToClassList("key-btn--incorrect");
        }
        else if (correctnessState == WordCorrectness.SPOT_INCORRECT)
        {
            btn.AddToClassList("key-btn--spot-incorrect");
        }
        else if (correctnessState == WordCorrectness.CORRECT)
        {
            btn.AddToClassList("key-btn--correct");
        }
    }
    

    void EnableInput()
    {
        foreach(var key in KeyButtons.Keys)
        {
            KeyButtons[key].SetEnabled(true);
        }
        EnterBtn.SetEnabled(true);
        BackspaceBtn.SetEnabled(true);
    }

    void DisableInput()
    {
        foreach (var key in KeyButtons.Keys)
        {
            KeyButtons[key].SetEnabled(false);
        }
        EnterBtn.SetEnabled(false);
        BackspaceBtn.SetEnabled(false);
    }

    public void OnStartOverHandle()
    {
        foreach (var key in WordleController.Keys)
        {
            CorrectnessStates[key] = WordCorrectness.DEFAULT;
            var btn = KeyButtons[key];

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");
        }

        EnableInput();
    }
}