using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class VirtualKeyboardController : MonoBehaviour
{
    Dictionary<char, WordCorrectness> CorrectnessStates { get; set; }
    Dictionary<char, Button> KeyButtons { get; set; }

    public bool IsEnable { get; private set; } = false;

    VisualElement container;

    private void Awake()
    {
        Debug.Log(2);
        CorrectnessStates = new(26);
        KeyButtons = new(26);

        container = GetComponent<UIDocument>().rootVisualElement.Q("visual-keyboard");
        BindElements();

        WordleController.Instance.OnAcceptInputWord += OnAcceptInputWordHandle;
        WordleController.Instance.OnStartOver += OnStartOverHandle;

    }

    void BindElements()
    {
        foreach(var key in WordleController.Instance.Keys) 
        {
            char lowerKey = char.ToLower(key);
            var btn = container.Q<Button>($"{lowerKey}-btn");
            if (btn == null)
                Debug.Log(key);
            btn.clicked += () => OnClickKeyButton(key);

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");

            btn.focusable = false;

            CorrectnessStates.Add(key, WordCorrectness.DEFAULT);
            KeyButtons.Add(key, btn);
        }

        var enterBtn = container.Q<Button>("enter-btn");
        enterBtn.clicked += OnClickEnterButton;
        enterBtn.focusable = false;

        var backspaceBtn = container.Q<Button>("backspace-btn");
        backspaceBtn.clicked += OnClickBackspaceButton;
        backspaceBtn.focusable = false;
    }

    private void OnEnable()
    {
        IsEnable = true;
    }

    private void OnDisable()
    {
        IsEnable = false;
    }

    void OnAcceptInputWordHandle(int lineIdx,string inputWord,WordCorrectness[] wordCorrect)
    {
        for(int i = 0; i < 5; i++)
        {
            SetCorrectionState(inputWord[i], wordCorrect[i]);
        }
    }

    void OnClickKeyButton(char c)
    {
        if(IsEnable) WordleController.Instance.InputLetter(c);
    }

    void OnClickEnterButton()
    {
        if (IsEnable) WordleController.Instance.SubmitInputWord();
    }

    void OnClickBackspaceButton()
    {
        if (IsEnable) WordleController.Instance.RemoveLetter();
    }

    public void SetCorrectionState(char c, WordCorrectness correctionState)
    {
        if (!CorrectnessStates.ContainsKey(c))
            return;

        if (CorrectnessStates[c] < correctionState)
        {
            CorrectnessStates[c] = correctionState;

            if (!KeyButtons.ContainsKey(c))
                return;

            var btn = KeyButtons[c];

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");

            if (correctionState == WordCorrectness.INCORRECT)
            {
                btn.AddToClassList("key-btn--incorrect");
            }
            else if (correctionState == WordCorrectness.SPOT_INCORRECT)
            {
                btn.AddToClassList("key-btn--spot-incorrect");
            }
            else if (correctionState == WordCorrectness.CORRECT)
            {
                btn.AddToClassList("key-btn--correct");
            }
        }
    }

    public void OnStartOverHandle()
    {
        foreach (var key in WordleController.Instance.Keys)
        {
            CorrectnessStates[key] = WordCorrectness.DEFAULT;
            var btn = KeyButtons[key];

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");
        }
    }
}