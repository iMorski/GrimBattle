using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // character generated position consts below : 
    private const float GAME_Z_CONST = 10.0f;
    private const float DISTANCE_BETWEEN_CHARACTERS = 0.1f;
    private const float MAX_GROUND_Y = 0.5f;
    private const float MIN_CHARACTER_Y = 0.2f;
    public enum ButtonType {
        Attack,
        EndTurn
    }
    public enum GameAction {
        Attack,
        EndTurn,
        MoveToNextScreen
    }

    public enum TeamType {
        Players,
        Monsters
    }

    [SerializeField] private Camera gameCamera;
    private List<Character> characterList = new List<Character>();

    private List<UIButtonMosterScene> buttons = new List<UIButtonMosterScene>();
    private Dictionary<Game.TeamType, List<Character>> characterListsByTeamType = new Dictionary<Game.TeamType, List<Character>>();
    private int currentCharacterID = 0;
    private int currentEnemyID = 0;
    private Game.TeamType currentTeamType = Game.TeamType.Players;
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

    public GameObject characterParent;
    [System.Serializable] public struct CharacterPrefab {
        public Character.CharacterType characterType;
        public GameObject prefab;
    }
    public CharacterPrefab[] characterTypeToPrefabArray;

    private Dictionary<Character.CharacterType, GameObject> characterTypeToPrefab = new Dictionary<Character.CharacterType, GameObject>();

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
        currentTeamType = Game.TeamType.Players; // current team Key

        enableButtons(true);
    }

    private void loadMonsters() {
        PlayerDataList dataList = JsonUtility.FromJson<PlayerDataList>(System.IO.File.ReadAllText(PathGetter.getScriptFolderPath(this) + "/MonsterInfo.json"));

        lastPlayerVPPosX = 1.0f;
        foreach(PlayerData d in dataList.data) {

            print ("TYPEEEEE : " + d.characterType.ToString());

            GameObject newCharacterObject = Instantiate(characterTypeToPrefab[(Character.CharacterType)d.characterType], characterParent.transform);

            customCharacterController chController = newCharacterObject.GetComponent<customCharacterController>();

            chController.setTeamAndInit(TeamType.Monsters);

            lastPlayerVPPosX -= DISTANCE_BETWEEN_CHARACTERS;
            Vector3 vpPos = new Vector3(lastPlayerVPPosX, Random.Range(MIN_CHARACTER_Y, MAX_GROUND_Y), GAME_Z_CONST);
            chController.setViewPortPosition(vpPos);

            // monsters face left, while all of our textures are facing right
            newCharacterObject.GetComponent<SpriteRenderer>().flipX = true;
            chController.attackPixelOffset = chController.attackPixelOffset * (-1);
        }
    }

    private float lastPlayerVPPosX = 0.0f;
    // private void createGameCharacter(PlayerData data) {
    // }
    private void loadPlayers() {
        PlayerDataList dataList = JsonUtility.FromJson<PlayerDataList>(System.IO.File.ReadAllText(PathGetter.getScriptFolderPath(this) + "/PlayerInfo.json"));

        lastPlayerVPPosX = 0.0f;
        foreach(PlayerData d in dataList.data) {
            GameObject newCharacterObject = Instantiate(characterTypeToPrefab[d.characterType], characterParent.transform);

            customCharacterController chController = newCharacterObject.GetComponent<customCharacterController>();

            chController.setTeamAndInit(TeamType.Players);

            lastPlayerVPPosX += DISTANCE_BETWEEN_CHARACTERS;
            Vector3 vpPos = new Vector3(lastPlayerVPPosX, Random.Range(MIN_CHARACTER_Y, MAX_GROUND_Y), GAME_Z_CONST);
            chController.setViewPortPosition(vpPos);
        }
    }

    private void populatePrefabDictionary() {
        for(int i = 0; i < characterTypeToPrefabArray.Length; ++i) {
            characterTypeToPrefab.Add(characterTypeToPrefabArray[i].characterType, characterTypeToPrefabArray[i].prefab);
        }
    }

    void Awake()
    {
        populatePrefabDictionary();
        loadMonsters();
        loadPlayers();
    }

    [System.Serializable] public class PlayerData {
        public int playerRegID;
        public int characterID; 
        public Character.CharacterType characterType;
        public Character.BaseStatsCharacter stats;
    }

    // [System.Serializable] PlayerData[] playerCharacterData;

    [System.Serializable] public class PlayerDataList {
        public List<PlayerData> data = new List<PlayerData>();
    } 
    private void writePlayerInfo(List<Character> playersTeam) {
        PlayerDataList dataList = new PlayerDataList();
        foreach(Character ch in playersTeam) {
            PlayerData d = new PlayerData();
            d.stats = playersTeam[0].getCharacterStats();
            d.playerRegID = 0; // player ID in data -- TODO 
            d.characterID = 0; // character ID for current player (if player has multiple characters) -- TODO
            dataList.data.Add(d);
        }
    
        string players = JsonUtility.ToJson(dataList);

        System.IO.File.WriteAllText(PathGetter.getScriptFolderPath(this) + "/PlayerInfo.json", players);
        System.IO.File.WriteAllText(PathGetter.getScriptFolderPath(this) + "/MonsterInfo.json", players);
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
        if (currentTeamType == Game.TeamType.Players) {
            currentTeamType = Game.TeamType.Monsters; 
        } else {
            currentTeamType = Game.TeamType.Players;
        }

        // writePlayerInfo(getPlayersTeam());
    }

    private List<Character> getPlayersTeam() {
        return characterListsByTeamType[Game.TeamType.Players];
    }
    private List<Character> getCurrentTeam() {
        if (!characterListsByTeamType.ContainsKey(currentTeamType)) {
            characterListsByTeamType.Add(currentTeamType, new List<Character>());
        }

        return characterListsByTeamType[currentTeamType];
    }

    private List<Character> getEnemyTeam() {
        Game.TeamType enemyTeamType = Game.TeamType.Monsters;
        if (currentTeamType == Game.TeamType.Monsters) {
            enemyTeamType = Game.TeamType.Players;
        }

        if (!characterListsByTeamType.ContainsKey(enemyTeamType)) {
            characterListsByTeamType.Add(enemyTeamType, new List<Character>());
        }

        return characterListsByTeamType[enemyTeamType];
    }

    public Character getCurrentCharacter() {
        print (currentCharacterID.ToString());
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
            // print ("WE'VE SWITCHED TEAMS SUCCESSFULLY");
        } else {
            currentCharacterID++;
        }
        
        updateCurrentEnemy();
        enableButtons(true);


    }

    private void startGameAction(GameAction actionType) {
        if (waitingToSwitchTurns) {
            // Debug.Log("WAITING FOR TURN SWITCH, CAN'T USE GAMEACTION NOW");
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
