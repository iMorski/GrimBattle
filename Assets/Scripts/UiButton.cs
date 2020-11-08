using TMPro;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    private TMP_InputField Input;
    private Animator Animator;

    private void Awake()
    {
        Input = GetComponent<TMP_InputField>();
        Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        FB.ConnectionStepChange += OnConnectionStepChange;
    }

    private void OnConnectionStepChange()
    {
        if (!(FB.MyData["Nick"] != "") && (!(FB.ConnectionStep != 3.1) || !(FB.ConnectionStep != 4.2)))
        {
            if (PlayerPrefs.HasKey("Nick"))
            {
                FB.MyData["Nick"] = PlayerPrefs.GetString("Nick");
            }
            else
            {
                FB.MyData["Nick"] = FB.MyName;
            }
            
            Input.text = FB.MyData["Nick"];
            FB.SetValue();
        }
    }

    public void Push()
    {
        Animator.Play("Button-Push");
    }

    public void Change()
    {
        if (Input.text != FB.MyData["Nick"])
        {
            PlayerPrefs.SetString("Nick", FB.MyData["Nick"]);
            
            FB.MyData["Nick"] = Input.text;
            FB.SetValue();
        }
    }
}
