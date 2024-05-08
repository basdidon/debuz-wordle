using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System;

[RequireComponent(typeof(UIDocument))]
public class UiController : MonoBehaviour
{
    public UIDocument UiDoc { get; private set; }

    VisaulKeyboardController VisaulKeyboardController { get; set; }

    VisualElement root;
    VisualElement visualKeyboard;

    Label keywordTxt;

    enum LetterContainerState {
        DEFAULT,
        INPUT,
        INCORRECT,
        SPOT_INCORRECT,
        CORRECT
    }

    private void Awake()
    {
        UiDoc = GetComponent<UIDocument>();

        VisaulKeyboardController = new();

        root = UiDoc.rootVisualElement;
        visualKeyboard = root.Q("visual-keyboard");
        VisaulKeyboardController.BindElements(visualKeyboard);

        keywordTxt = root.Q<Label>("keyword-txt");

        WordleController.Instance.OnNewKeyword += OnNewKeywordHandle;
        WordleController.Instance.OnAddLetter += OnAddLetterHandle;
        WordleController.Instance.OnRemoveLetter += OnRemoveLetterHandle;
        WordleController.Instance.OnAcceptInputWord += OnAcceptInputWordHandle;
        WordleController.Instance.OnSubmitNotCompleteInputWord += ShakeWord;
        WordleController.Instance.OnRejectInputWord += (lineIdx) => ShakeWord(lineIdx,5);

        DOTween.Init();
    }

    #region WordleController event handler
    void OnNewKeywordHandle(string newKeyword)
    {
        keywordTxt.text = newKeyword.ToUpper();
    }

    void OnAddLetterHandle(int lineIdx,int characterIdx,char newLetter)
    {
        SetLetter(lineIdx, characterIdx, newLetter.ToString());
        SetStateToLetterContainer(LetterContainerState.INPUT, lineIdx, characterIdx);
    }

    void OnRemoveLetterHandle(int lineIdx, int characterIdx)
    {
        SetLetter(lineIdx, characterIdx, string.Empty);
        SetStateToLetterContainer(LetterContainerState.DEFAULT, lineIdx, characterIdx);
    }

    void OnAcceptInputWordHandle(int lineIdx,LetterCorrectResult[] result)
    {
        for (int i = 0; i < result.Length; i++)
        {
            LetterContainerState letterContainerState;
            if (result[i] == LetterCorrectResult.CORRECT)
            {
                letterContainerState = LetterContainerState.CORRECT;
            }
            else if (result[i] == LetterCorrectResult.SPOT_INCORRECT)
            {
                letterContainerState = LetterContainerState.SPOT_INCORRECT;
            }
            else
            {
                letterContainerState = LetterContainerState.INCORRECT;
            }

            FlipLetter(lineIdx,i, () => SetStateToLetterContainer(letterContainerState, lineIdx, i));
            //SetStateToLetterContainer(letterContainerState, lineIdx, _i);
            /*
            Sequence sequence = DOTween.Sequence();
            var wordContainer = root.Q($"word-{lineIdx}");
            var letterContainer = wordContainer.Q($"letter-{_i}");
            sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, new Vector3(0, 1, 1), .2f).OnComplete(() => SetStateToLetterContainer(letterContainerState, lineIdx, _i))) //
                .Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, Vector3.one, .2f))
                .Restart();
            */
        }
    }
    #endregion

    void SetLetter(int lineIdx, int characterIdx, string newString)
    {
        var wordContainer = root.Q($"word-{lineIdx}");
        var letterContainer = wordContainer.Q($"letter-{characterIdx}");
        var letterTxt = letterContainer.Q<Label>();
        letterTxt.text = newString;
    }

    void SetStateToLetterContainer(LetterContainerState letterContainerState, int lineIdx, int characterIdx)
    {
        // remove old styles
        var wordContainer = root.Q($"word-{lineIdx}");
        var letterContainer = wordContainer.Q($"letter-{characterIdx}");
        var letterTxt = letterContainer.Q<Label>();

        letterContainer.RemoveFromClassList("letter-container--preview");
        letterContainer.RemoveFromClassList("letter-container--before-preview");
        letterContainer.RemoveFromClassList("letter-container--incorrect");
        letterContainer.RemoveFromClassList("letter-container--spot-incorrect");
        letterContainer.RemoveFromClassList("letter-container--correct");
        letterTxt.RemoveFromClassList("letter-txt--preview");

        if (letterContainerState == LetterContainerState.INPUT)
        {
            letterContainer.AddToClassList("letter-container--before-preview");
            //letterContainer.AddToClassList("letter-container--preview");  // 
            StartCoroutine(AddClassToListNextFrame(letterContainer, "letter-container--preview"));
            letterTxt.AddToClassList("letter-txt--preview");
        }
        else if (letterContainerState == LetterContainerState.INCORRECT)
        {
            letterContainer.AddToClassList("letter-container--incorrect");
        }
        else if (letterContainerState == LetterContainerState.SPOT_INCORRECT)
        {
            letterContainer.AddToClassList("letter-container--spot-incorrect");

        }
        else if (letterContainerState == LetterContainerState.CORRECT)
        {
            letterContainer.AddToClassList("letter-container--correct");
        }
    }

    IEnumerator AddClassToListNextFrame(VisualElement visualElement,string className)
    {
        yield return new WaitForEndOfFrame();
        visualElement.AddToClassList(className);
    }

    
    // animation
    void ShakeWord(int lineIdx, int length)
    {
        var strength = 4;
        float timePerLoop = 0.08f;
        var halfTimePerLoop = timePerLoop/2;

        for(int i = 0; i < length; i++)
        {
            Sequence sequence = DOTween.Sequence();
            var wordContainer = root.Q($"word-{lineIdx}");
            var letterContainer = wordContainer.Q($"letter-{i}");
            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(-strength, 0, 0), halfTimePerLoop));
            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(strength, 0, 0), timePerLoop).SetLoops(3,LoopType.Yoyo));
            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(0, 0, 0), halfTimePerLoop));
            sequence.Play();
        }
    }

    void FlipLetter(int lineIdx, int letterIdx,Action OnHalfComplete)
    {
        Sequence sequence = DOTween.Sequence();
        var wordContainer = root.Q($"word-{lineIdx}");
        var letterContainer = wordContainer.Q($"letter-{letterIdx}");
        sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, new Vector3(0, 1, 1), .2f));
        sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, Vector3.one, .2f));
        sequence.OnStart(() => OnHalfComplete());
        sequence.Play();
    }

    void FlipWord(int lineIdx, Action OnHalfComplete)
    {
        for (int i = 0; i < 5; i++)
        {
            Sequence sequence = DOTween.Sequence();
            var wordContainer = root.Q($"word-{lineIdx}");
            var letterContainer = wordContainer.Q($"letter-{i}");
            sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, new Vector3 (0,1,1), .2f).OnComplete(()=> OnHalfComplete()));
            sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, Vector3.one, .2f));
            sequence.Play();
        }
    }
}

public class VisaulKeyboardController
{ 
    public void BindElements(VisualElement visualElement)
    {
        for (char c = 'A'; c <= 'Z'; c++) // A-Z
        {
            var _c = c;
            var btn = visualElement.Q<Button>($"{char.ToLower(_c)}-btn");
            btn.clicked += () => OnClickKeyButton(_c);
        }

        var enterBtn = visualElement.Q<Button>("enter-btn");
        enterBtn.clicked += OnClickEnterButton;

        var backspaceBtn = visualElement.Q<Button>("backspace-btn");
        backspaceBtn.clicked += OnClickBackspaceButton;
    }

    void OnClickKeyButton(char c)
    {
        Debug.Log($"{c} clicked");
        WordleController.Instance.InputLetter(c);
    }

    void OnClickEnterButton()
    {
        Debug.Log($"enter-button clicked");
        WordleController.Instance.SubmitInputWord();
    }

    void OnClickBackspaceButton()
    {
        Debug.Log($"backspace-btn clicked");
        WordleController.Instance.RemoveLetter();

    }
}
