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

    public enum teamType {
        Players,
        Monsters
    }

    [SerializeField] private Camera gameCamera;
    private List<Character> characterList = new List<Character>();

    private Dictionary<Game.teamType, List<Character>> characterListsByTeamType = new Dictionary<Game.teamType, List<Character>>();
    private int currentCharacterID = 0;
    private int currentEnemyID = 0;
    private Game.teamType currentTeamType = Game.teamType.Players;
    private List<int> availableEnemyIDs = new List<int>();


    public Camera getMainCamera() {
        return gameCamera;
    }

    public void registerCharacter(Character character) {
        Debug.Log(character.getTeam().ToString());

        if (!characterListsByTeamType.ContainsKey(character.getTeam())) {
            characterListsByTeamType.Add(character.getTeam(), new List<Character>());
        }

        List<Character> teamList = characterListsByTeamType[character.getTeam()];
        teamList.Add(character);
        character.setTeamID(teamList.Count - 1);

        initTurns();
        switchTurns();
    }

    public Vector3 getNextScenePositionForCharacterID(int characterID) {
        // TODO
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    private void initTurns() {
        currentCharacterID = -1;
        currentEnemyID = -1;
        currentTeamType = Game.teamType.Players; // current team Key
    }

    void Start()
    {
        initTurns();
    }

    void updateTurns() {

    }

    private void updateCurrentEnemy() {
        // availableEnemyIDs.Clear();
        // int currentTeamType = getCurrentCharacter().getTeam();

        // for (int i = 0; i < characterList.Count; ++i) {
        //     if (characterList[i].getTeam() != currentTeamType && characterList[i].getAlive()) {
        //         availableEnemyIDs.Add(i);
        //     }
        // }

        // if(availableEnemyIDs.Count > 0) {
        //     currentEnemyID = availableEnemyIDs[0];

        // } else {
        //     // TODO : ALL POTENTIAL ENEMIES ARE DEAD. END THIS SCENE 
        // }

        var enemiesList = getEnemyTeam();

        if (enemiesList.Count < 1) {
            Debug.Log("No enemies found!");

            return;
        } 

        Character enemy = enemiesList[0];
        for (int i = 0; i < enemiesList.Count; ++i) {
            enemy =  enemiesList[i];
            if (enemy.getAlive()) {
                // Find first enemy that's alive for now;
                currentEnemyID = i;
                break;
            }
        }
    }

    private void switchTeams() {
        // DUMB IMPLEMENTATION FOR NOW; Needs to be - find next team with > 0 alive members; if none - move to next scene
        if (currentTeamType == Game.teamType.Players) {
            currentTeamType = Game.teamType.Monsters; 
        } else {
            currentTeamType = Game.teamType.Players;
        }
    }

    private List<Character> getCurrentTeam() {
        return characterListsByTeamType[currentTeamType];
    }

    private List<Character> getEnemyTeam() {
        Game.teamType enemyTeamType = Game.teamType.Monsters;
        if (currentTeamType == Game.teamType.Monsters) {
            enemyTeamType = Game.teamType.Players;
        }

        if (!characterListsByTeamType.ContainsKey(enemyTeamType)) {
            characterListsByTeamType.Add(enemyTeamType, new List<Character>());
        }

        return characterListsByTeamType[enemyTeamType];
    }

    public Character getCurrentCharacter() {
        return getCurrentTeam()[currentCharacterID];
    }

    public Character getCurrentEnemy() {
        if (getEnemyTeam().Count < currentEnemyID + 1) {
            Debug.LogError("Some actions called on enemy that doesn't exist.");
        }

        // Debug.Log(getEnemyTeam()[currentEnemyID].getName());
        return getEnemyTeam()[currentEnemyID];
    }

    private void switchTurns() {
        if (getCurrentTeam().Count <= (currentCharacterID + 1)) {
            switchTeams();
        }
        
        currentCharacterID++;

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
                // Debug.Log(" CLICKTYPE.ATTACK ACTION ");
                startGameAction(GameAction.Attack);

                break;
            case ClickType.EndTurn :
                // Debug.Log(" CLICKTYPE.ENDTURN ACTION ");
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
