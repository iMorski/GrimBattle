using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIButtonMosterScene : MonoBehaviour
{
    private Animator Animator;
    [SerializeField] private Game game;
    [SerializeField] private Game.ButtonType buttonType;
    // [SerializeField] private TextMeshPro buttonText;
    [SerializeField] private TextMeshProUGUI buttonText;
    

    private Dictionary<bool, Color> buttonColors = new Dictionary<bool, Color>();

    private bool buttonEnabled = true;
    private void Awake()
    {
        Animator = GetComponent<Animator>(); 
        buttonEnabled = true;

        buttonColors[true] = buttonText.color;
        buttonColors[false] = new Color(buttonText.color.grayscale, buttonText.color.grayscale,buttonText.color.grayscale,  buttonText.color.a);
    }

    private void Start() {
        game.registerButton(this);
    }

    public void Push()
    {
        if (buttonEnabled) {
            Animator.Play("Button-Push");
            game.startClickAction(buttonType);   
        }

        // button can get disabled by action-limit per turn
        setEnabled(game.getPressable(buttonType));
    }

    public void setEnabled(bool enabled) {
        if (buttonEnabled == enabled) {
            return;
        }
        buttonEnabled = enabled;

        buttonText.color = buttonColors[enabled];
    }

    public bool getEnabled() {
        return buttonEnabled;
    }
}
