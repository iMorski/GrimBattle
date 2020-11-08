using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonMosterScene : MonoBehaviour
{
    private Animator Animator;
    [SerializeField] private Game game;
    [SerializeField] private Game.ClickType clickType;
    private void Awake()
    {
        Animator = GetComponent<Animator>(); 
        Debug.Log("GOt ANIMATOR");
    }

    public void Push()
    {
        Animator.Play("Button-Push");
        game.startClickAction(clickType);        
    }
}
