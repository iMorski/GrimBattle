using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public enum ButtonType {
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

    private List<UIButtonMosterScene> buttons = new List<UIButtonMosterScene>();
    private Dictionary<Game.teamType, List<Character>> characterListsByTeamType = new Dictionary<Game.teamType, List<Character>>();
    private int currentCharacterID = 0;
    private int currentEnemyID = 0;
    private Game.teamType currentTeamType = Game.teamType.Players;
    private bool waitingToSwitchTurns = false;

    public Dictionary<ButtonType, GameAction> buttonTypeToGameAction = new Dictionary<ButtonType, GameAction>(){
        {ButtonType.Attack, GameAction.Attack},
        {ButtonType.EndTurn, GameAction.EndTurn}
    };

    public Dictionary<GameAction, int> requestedActionsOfType = new Dictionary<GameAction, int>(){
        {GameAction.Attack, 0},
        {GameAction.EndTurn, 0},
        {GameAction.MoveToNextScreen, 0}
    };

    public Dictionary<GameAction, int> actionNumRestriction = new Dictionary<GameAction, int>(){
        {GameAction.Attack, 1},
        {GameAction.EndTurn, -1},
        {GameAction.MoveToNextScreen, -1}
    };

    public Camera getMainCamera() {
        return gameCamera;
    }

    public void registerCharacter(Character character) {
        Debug.Log(character.getTeamType().ToString());

        if (!characterListsByTeamType.ContainsKey(character.getTeamType())) {
            characterListsByTeamType.Add(character.getTeamType(), new List<Character>());
        }

        List<Character> teamList = characterListsByTeamType[character.getTeamType()];
        teamList.Add(character);
        character.setTeamID(teamList.Count - 1);

        initTurns();
        switchTurns();
    }

    public void registerButton(UIButtonMosterScene button) {
        buttons.Add(button);
    }

    private void enableButtons(bool enable) {
        foreach(UIButtonMosterScene button in buttons) {
            button.setEnabled(enable);
        }
    }

    public Vector3 getNextScenePositionForCharacterID(int characterID) {
        // TODO
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    private void initTurns() {
        currentCharacterID = -1;
        currentEnemyID = -1;
        currentTeamType = Game.teamType.Players; // current team Key

        enableButtons(true);
    }

    void Awake()
    {
        initTurns();
    }

    private void updateCurrentEnemy() {
        var enemiesList = getEnemyTeam();

        if (enemiesList.Count < 1) {
            Debug.Log("No enemies found!");

            return;
        } 

        Character enemy = enemiesList[0];
        for (int i = 0; i < enemiesList.Count; ++i) {
            enemy = enemiesList[i];
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


    private void switchTurnsIfPossible() {
        if (getCurrentCharacter().isInAction()) {
            waitingToSwitchTurns = true;
            enableButtons(false);
        } else {
            switchTurns();
        }
    }

    private void switchTurns() {
        
        if (getCurrentTeam().Count <= (currentCharacterID + 1)) {
            switchTeams();

            currentCharacterID = 0;
            print ("WE'VE SWITCHED TEAMS SUCCESSFULLY");
        } else {
            currentCharacterID++;
        }
        
        updateCurrentEnemy();
        enableButtons(true);
    }

    private void startGameAction(GameAction actionType) {
        if (waitingToSwitchTurns) {
            Debug.Log("WAITING FOR TURN SWITCH, CAN'T USE GAMEACTION NOW");
            return;
        }

        Debug.Log("I'm " + getCurrentCharacter().getName() + "; I will " + actionType.ToString());

        switch (actionType) {
            case GameAction.Attack : 
                getCurrentCharacter().addActionSequence(Character.ActionSequenceType.HitEnemy);
                break;
            case GameAction.EndTurn :
                switchTurnsIfPossible();
                break;
            case GameAction.MoveToNextScreen :
                getCurrentCharacter().addActionSequence(Character.ActionSequenceType.MoveToNextScene);
                break;
        }

        requestedActionsOfType[actionType]++;
    }

    public void endedAllActions(Character character) {
        if (!waitingToSwitchTurns) {
            return;
        }
        
        if (character.getTeamType() == currentTeamType && character.getTeamID() == getCurrentCharacter().getTeamID()) {
            waitingToSwitchTurns = false;
            switchTurns();
        }
    }

    public void startClickAction(ButtonType buttonType) {
        startGameAction(buttonTypeToGameAction[buttonType]);
    }

    public bool canPerformAction(GameAction actionType) {
        bool can = true;
        can &= (!waitingToSwitchTurns);

        if (actionNumRestriction[actionType] == -1) {
            // do nothing, leave can true; -1 == no action number restriction
        } else if (requestedActionsOfType[actionType] >= actionNumRestriction[actionType]) {
            can &= false;
        }

        //check for resource or other restriction here

        return can;
    }

    public bool getPressable(ButtonType buttonType) {
        if (canPerformAction(buttonTypeToGameAction[buttonType])) {
            return true;
        };

        return false;
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
