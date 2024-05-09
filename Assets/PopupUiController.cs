using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PopupUiController : MonoBehaviour
{
    VisualElement rootContainer;

    Label title;
    Label line_0;
    Label line_1;
    Label line_2;
    Label line_3;

    Button startOverBtn;
    Button quitBtn;

    private void Awake()
    {
        rootContainer = GetComponent<UIDocument>().rootVisualElement.Q("popup-menu");

        title = rootContainer.Q<Label>("popup-title");
        line_0 = rootContainer.Q<Label>("popup-line-0");
        line_1 = rootContainer.Q<Label>("popup-line-1");
        line_2 = rootContainer.Q<Label>("popup-line-2");
        line_3 = rootContainer.Q<Label>("popup-line-3");

        startOverBtn = rootContainer.Q<Button>("start-over-btn");
        startOverBtn.clicked += OnStartOverClick;
        quitBtn = rootContainer.Q<Button>("quit-btn");

        WordleController.Instance.OnWinGame += OnWinGameHandle;
        WordleController.Instance.OnLoseGame += OnLoseGameHandle;
    }

    public void Display()
    {
        Debug.Log("display popup");
        rootContainer.AddToClassList("popup-container--up");
    }

    public void Hide()
    {
        rootContainer.RemoveFromClassList("popup-container--up");
    }

    public void OnWinGameHandle(string keyword, int guessCount)
    {
        title.text = "Hooray !!!";
        line_0.text = "You have complete a word within : ";
        line_1.text = $"{guessCount}";
        line_2.text = guessCount == 1 ? "time" : "times";
        line_3.text = "Play again?";
        Display();
    }

    public void OnLoseGameHandle(string keyword)
    {
        title.text = "Eww, Loser !!!";
        line_0.text = "a keyword is : ";
        line_1.text = keyword.ToUpper();
        line_2.text = "it very very close";
        line_3.text = "Do you wanna try again?";
        Display();
    }

    void OnStartOverClick()
    {
        WordleController.Instance.StartOver();
        Hide();
    }
}
