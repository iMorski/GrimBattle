using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public enum ClickType {
        Attack,
        EndTurn
    }
    public enum GameAction {
        Attack,
        EndTurn,
        MoveToNextScreen
    }

    [SerializeField] private Camera gameCamera;
    private List<Character> characterList = new List<Character>();
    private int currentCharacterID = 0;
    private int currentEnemyID = 0;
    private List<int> availableEnemyIDs = new List<int>();


    public Camera getMainCamera() {
        return gameCamera;
    }
    public Character getCurrentCharacter() {
        return characterList[currentCharacterID];
    }

    public Character getCurrentEnemy() {
        return characterList[currentEnemyID];
    }

    private Character getCharacter(int ID) {
        return characterList[ID];
    }

    public void registerCharacter(Character character) {
        characterList.Add(character);
        character.setGameID(characterList.Count - 1);
    }

    public Vector3 getNextScenePositionForCharacterID(int characterID) {
        // TODO
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    void Start()
    {
        
    }

    void updateTurns() {

    }

    private void updateCurrentEnemy() {
        availableEnemyIDs.Clear();
        int currentTeam = getCurrentCharacter().getTeam();

        for (int i = 0; i < characterList.Count; ++i) {
            if (characterList[i].getTeam() != currentTeam && characterList[i].getAlive()) {
                availableEnemyIDs.Add(i);
            }
        }

        if(availableEnemyIDs.Count > 0) {
            currentEnemyID = availableEnemyIDs[0];
        } else {
            // TODO : ALL POTENTIAL ENEMIES ARE DEAD. END THIS SCENE 
        }
    }
    private void switchTurns() {
        int charactersN = characterList.Count;
        currentCharacterID++;
        currentCharacterID %= charactersN;

        updateCurrentEnemy();
    }

    private void startGameAction(GameAction actionType) {
        if (getCurrentCharacter().isInAction()) {
            Debug.Log(" Character Still In Action. Please wait for it to finish before acting again !");
            return;
        }

        Debug.Log("I'm " + getCurrentCharacter().getName() + "; I will " + actionType.ToString());

        switch (actionType) {
            case GameAction.Attack : 
                getCurrentCharacter().addActionSequence(Character.ActionSequenceType.HitEnemy);
                break;
            case GameAction.EndTurn :
                switchTurns();
                break;
            case GameAction.MoveToNextScreen :
                getCurrentCharacter().addActionSequence(Character.ActionSequenceType.MoveToNextScene);
                break;
        }
    }

    public void startClickAction(ClickType clickType) {
        switch(clickType) {
            case ClickType.Attack :
                Debug.Log(" CLICKTYPE.ATTACK ACTION ");
                startGameAction(GameAction.Attack);

                break;
            case ClickType.EndTurn :
                Debug.Log(" CLICKTYPE.ENDTURN ACTION ");
                startGameAction(GameAction.EndTurn);

                break;
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        // updateTurns();

        // if (Input.GetButtonDown ("Fire1")) {
        //     // startClickAction(ClickType.Attack);
        //     startGameAction(GameAction.Attack);
        // }
    }
}
