using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputController : MonoBehaviour
{
    InputActionMap ActionMap { get; set; }

    private void Awake()
    {
        ActionMap = new InputActionMap();

        foreach(var key in WordleController.Instance.Keys)
        {
            var action = ActionMap.AddAction($"{key}", binding: $"<KeyBoard>/{key}");
            action.performed += ctx =>
            {
                Debug.Log(key);
                WordleController.Instance.InputLetter(key);
            };
        }

        var enterAction = ActionMap.AddAction("enter", binding: $"<KeyBoard>/Enter");
        enterAction.performed += _ => WordleController.Instance.SubmitInputWord();

        var backspaceAction = ActionMap.AddAction("backspace", binding: $"<KeyBoard>/Backspace");
        backspaceAction.performed += _ => WordleController.Instance.RemoveLetter();

        // event 
    }

    private void OnEnable()
    {
        ActionMap.Enable();
    }

    private void OnDisable()
    {
        ActionMap.Disable();
    }
}
