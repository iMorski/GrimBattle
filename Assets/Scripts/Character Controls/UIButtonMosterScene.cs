using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonMosterScene : MonoBehaviour
{
    private float BUTTON_DISABLED_COLORMULT = 0.5f;
    private Animator Animator;
    [SerializeField] private Game game;
    [SerializeField] private Game.ButtonType buttonType;
    [SerializeField] private UnityEngine.UI.Image icon;
    
    private Dictionary<bool, Color> buttonColors = new Dictionary<bool, Color>();

    private bool buttonEnabled = true;
    private void Awake()
    {
        Animator = GetComponent<Animator>(); 
        buttonEnabled = true;

        buttonColors[true] = icon.color;
        buttonColors[false] = new Color(icon.color.grayscale, icon.color.grayscale,icon.color.grayscale,  icon.color.a);
        buttonColors[false] = buttonColors[false] * BUTTON_DISABLED_COLORMULT;
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

        icon.color = buttonColors[enabled];
    }

    public bool getEnabled() {
        return buttonEnabled;
    }
}
