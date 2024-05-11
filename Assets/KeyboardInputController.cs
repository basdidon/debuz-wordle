using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputController : MonoBehaviour
{
    WordleController WordleController { get; set; }
    InputActionMap ActionMap { get; set; }

    private void Awake()
    {
        WordleController = WordleController.Instance;

        ActionMap = new InputActionMap();

        foreach(var key in WordleController.Keys)
        {
            var action = ActionMap.AddAction($"{key}", binding: $"<KeyBoard>/{key}");
            action.performed += ctx =>
            {
                Debug.Log(key);
                WordleController.InputLetter(key);
            };
        }

        var enterAction = ActionMap.AddAction("enter", binding: $"<KeyBoard>/Enter");
        enterAction.performed += _ => WordleController.SubmitInputWord();

        var backspaceAction = ActionMap.AddAction("backspace", binding: $"<KeyBoard>/Backspace");
        backspaceAction.performed += _ => WordleController.RemoveLetter();

        // Set input active state depend on a game state
        WordleController.OnWinGame += (_,_) => ActionMap.Disable(); 
        WordleController.OnLoseGame += _ => ActionMap.Disable();
        WordleController.OnStartOver += () => ActionMap.Enable();
    }

    private void OnEnable() => ActionMap.Enable();
    private void OnDisable()=>  ActionMap.Disable();
}
