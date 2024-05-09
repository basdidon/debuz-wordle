using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System;

[RequireComponent(typeof(UIDocument))]
public class UiController : MonoBehaviour
{
    public UIDocument UiDoc { get; private set; }

    VisualElement root;
    VisualElement[,] letterContainers; 
    Label keywordTxt;


    enum LetterContainerState {
        DEFAULT,
        INCORRECT,
        SPOT_INCORRECT,
        CORRECT,
        INPUT
    }

    private void Awake()
    {
        UiDoc = GetComponent<UIDocument>();

        root = UiDoc.rootVisualElement;
        
        letterContainers = new VisualElement[6, 5];
        for(int i = 0; i < letterContainers.GetLength(0); i++)
        {
            VisualElement wordContainer = root.Q($"word-{i}");
            for(int j = 0; j< letterContainers.GetLength(1); j++)
            {
                letterContainers[i, j] = wordContainer.Q($"letter-{j}");
            }
        }

        keywordTxt = root.Q<Label>("keyword-txt");

        WordleController.Instance.OnNewKeyword += OnNewKeywordHandle;
        WordleController.Instance.OnAddLetter += OnAddLetterHandle;
        WordleController.Instance.OnRemoveLetter += OnRemoveLetterHandle;
        WordleController.Instance.OnAcceptInputWord += OnAcceptInputWordHandle;
        WordleController.Instance.OnSubmitNotCompleteInputWord += ShakeWord;
        WordleController.Instance.OnRejectInputWord += (lineIdx) => ShakeWord(lineIdx,5);

        WordleController.Instance.OnStartOver += OnStartOverHandle;

        DOTween.Init().SetCapacity(200, 10);
    }

    #region WordleController event handler
    void OnNewKeywordHandle(string newKeyword)
    {
        keywordTxt.text = newKeyword.ToUpper();
    }

    void OnAddLetterHandle(int lineIdx,int characterIdx,char newLetter)
    {
        SetLetter(lineIdx, characterIdx, newLetter.ToString().ToUpper());
        SetStateToLetterContainer(LetterContainerState.INPUT, lineIdx, characterIdx);
    }

    void OnRemoveLetterHandle(int lineIdx, int characterIdx)
    {
        SetLetter(lineIdx, characterIdx, string.Empty);
        SetStateToLetterContainer(LetterContainerState.DEFAULT, lineIdx, characterIdx);
    }

    void OnAcceptInputWordHandle(int lineIdx, string inputWord,WordCorrectness[] result)
    {
        Debug.Log("OnAcceptInputWordHandle start");
        for (int i = 0; i < result.Length; i++)
        {
            LetterContainerState letterContainerState = result[i] switch 
            {
                WordCorrectness.CORRECT => LetterContainerState.CORRECT,
                WordCorrectness.INCORRECT => LetterContainerState.INCORRECT,
                WordCorrectness.SPOT_INCORRECT => LetterContainerState.SPOT_INCORRECT,
                _ => throw new System.InvalidOperationException()
            };

            var _i = i;
            FlipLetter(lineIdx, _i, () => SetStateToLetterContainer(letterContainerState, lineIdx, _i));
        }
    }

    void OnStartOverHandle()
    {
        foreach(var letterContainer in letterContainers)
        {
            // remove classes
            letterContainer.RemoveFromClassList("letter-container--preview");
            letterContainer.RemoveFromClassList("letter-container--before-preview");
            letterContainer.RemoveFromClassList("letter-container--incorrect");
            letterContainer.RemoveFromClassList("letter-container--spot-incorrect");
            letterContainer.RemoveFromClassList("letter-container--correct");

            var letterTxt = letterContainer.Q<Label>();
            letterTxt.text = string.Empty;
        }
    }

    #endregion

    void SetLetter(int lineIdx, int letterIdx, string newString)
    {
        Debug.Log($"set letter {newString}");
        var letterTxt = letterContainers[lineIdx,letterIdx].Q<Label>();
        letterTxt.text = newString;
    }

    void SetStateToLetterContainer(LetterContainerState letterContainerState, int lineIdx, int letterIdx)
    {
        var letterContainer = letterContainers[lineIdx,letterIdx];
        // remove classes
        letterContainer.RemoveFromClassList("letter-container--preview");
        letterContainer.RemoveFromClassList("letter-container--before-preview");
        letterContainer.RemoveFromClassList("letter-container--incorrect");
        letterContainer.RemoveFromClassList("letter-container--spot-incorrect");
        letterContainer.RemoveFromClassList("letter-container--correct");

        if (letterContainerState == LetterContainerState.INPUT)
        {
            letterContainer.AddToClassList("letter-container--before-preview");
            //letterContainer.AddToClassList("letter-container--preview");  // 
            StartCoroutine(AddClassToListNextFrame(letterContainer, "letter-container--preview"));
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


    #region DOTween animation
    void ShakeWord(int lineIdx, int length)
    {
        var strength = 4;
        float timePerLoop = 0.08f;
        var halfTimePerLoop = timePerLoop/2;

        for(int i = 0; i < length; i++)
        {
            Sequence sequence = DOTween.Sequence();
            var letterContainer = letterContainers[lineIdx, i];

            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(-strength, 0, 0), halfTimePerLoop));
            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(strength, 0, 0), timePerLoop).SetLoops(3,LoopType.Yoyo));
            sequence.Append(DOTween.To(() => letterContainer.transform.position, (x) => letterContainer.transform.position = x, new Vector3(0, 0, 0), halfTimePerLoop));
            sequence.Play();
        }
    }

    void FlipLetter(int lineIdx, int letterIdx, Action OnFlip)
    {
        float duration = .2f;
        Sequence sequence = DOTween.Sequence();
        var letterContainer = letterContainers[lineIdx, letterIdx];

        sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, new Vector3(0, 1, 1), duration));
        sequence.AppendInterval(duration);
        sequence.AppendCallback(() => OnFlip());
        sequence.Append(DOTween.To(() => letterContainer.transform.scale, (x) => letterContainer.transform.scale = x, new Vector3(1, 1, 1), duration));
        sequence.Play();
    }
    #endregion
}
