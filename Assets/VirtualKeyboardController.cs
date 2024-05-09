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
        CorrectnessStates = new(26);
        KeyButtons = new(26);

        container = GetComponent<UIDocument>().rootVisualElement.Q("visual-keyboard");
        BindElements();

        WordleController.Instance.OnAcceptInputWord += OnAcceptInputWordHandle;
        WordleController.Instance.OnStartOver += OnStartOverHandle;

    }

    void BindElements()
    {
        for (char c = 'a'; c <= 'z'; c++) // a-z
        {
            var _c = c;
            var btn = container.Q<Button>($"{c}-btn");
            btn.clicked += () => OnClickKeyButton(_c);

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");

            CorrectnessStates.Add(c, WordCorrectness.DEFAULT);
            KeyButtons.Add(c, btn);
        }

        var enterBtn = container.Q<Button>("enter-btn");
        enterBtn.clicked += OnClickEnterButton;

        var backspaceBtn = container.Q<Button>("backspace-btn");
        backspaceBtn.clicked += OnClickBackspaceButton;
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
        for (char c = 'a'; c <= 'z'; c++)
        {
            CorrectnessStates[c] = WordCorrectness.DEFAULT;
            var btn = KeyButtons[c];

            btn.RemoveFromClassList("key-btn--incorrect");
            btn.RemoveFromClassList("key-btn--spot-incorrect");
            btn.RemoveFromClassList("key-btn--correct");
        }
    }
}