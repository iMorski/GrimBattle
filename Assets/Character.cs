using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	public enum AnimationType {
		Idle,
		Run,
		IsHit,
		Death,
        Attack
	}    	

    public enum ActionSequenceType {
        MoveToNextScene,
        HitEnemy,
        ReceiveDamage,
        Die
    }

	private enum ActionType {
		Move,
		Hit,
        ReceiveDamage,
        Die
	} 

	private enum ActionState {
		Idle,
		InAction
	}
    private enum MoveType {
        ToEnemy,
        FromEnemy,
        ToNextScene
    }
    private struct ActionInfo {
        public ActionInfo(ActionType aType) {
            actionType = aType;
            damage = 0;
            destination = new Vector2(0.0f, 0.0f);
            associatedAnimType = AnimationType.Run;
        }

        public ActionInfo(ActionType aType, int dam, Vector2 dest) {
            actionType = aType;
            damage = dam;
            destination = dest;
            associatedAnimType = AnimationType.Run;
        }

        public ActionType actionType;
        public int damage;
        public Vector3 destination;
        public AnimationType associatedAnimType;
    };

    private List<ActionInfo> actionsQueue = new List<ActionInfo>();
    private ActionInfo currentActionInfo;

    public struct CharacterStats {
        public CharacterStats(int health) {
            HP = health;
            damage = 100;
            movSpeed = 1.0f;
            critChance = 0.1f;
            critMult = 2.1f;
        }

        public CharacterStats(int health, int dmg, float speed, float cChance, float cMult) {
            HP = health;
            damage = dmg;
            movSpeed = speed;
            critChance = cChance;
            critMult = cMult;
        }

        public int HP;
        public int damage;
        public float movSpeed;
        public float critChance;
        public float critMult;
    }

    private struct CharacterState {
        public int HP;
        public int damage;
        public float damageBuff;
        public float critChance;
        public float critMult;
        public float speed; // per unit time 
        public ActionState actionState;
        public bool alive;
        public bool isMoving;
        public Vector3 moveDirection;
        public Vector3 viewPortPosition;
        public Vector3 lastViewPortPosition; // position before last moveAction
        public  AnimationType curAnimationType;
    }

    public struct SceneInfo {
        public Animator animator;
        public Camera mainCamera;
        public GameObject gameObject;
    }

    private struct GameInfo {
        public Vector3 nextScenePosition;
        public Character curEnemy;
    }

    private Dictionary<AnimationType, string> animTypeToString;

    private CharacterState characterState;
    private SceneInfo sceneEntities;
    private GameInfo gameInfo;

    public void init(CharacterStats characterStats, SceneInfo sInfo, Dictionary<AnimationType, string> animBundle) {
        characterState = new CharacterState();
        characterState.HP = characterStats.HP;
        characterState.speed = characterStats.movSpeed;
        characterState.damage = characterStats.damage;
        characterState.damageBuff = 1.0f;
        characterState.critChance = characterStats.critChance;
        characterState.critMult = characterStats.critMult;
    	characterState.curAnimationType = AnimationType.Idle;
        characterState.isMoving = false;
        characterState.alive = true;
        characterState.actionState = ActionState.Idle;
        characterState.moveDirection = new Vector3(1.0f, 0.0f, 0.0f);

        sceneEntities = new SceneInfo();
        sceneEntities.animator = sInfo.animator;
        sceneEntities.mainCamera = sInfo.mainCamera;
        sceneEntities.gameObject = sInfo.gameObject;

        characterState.viewPortPosition = new Vector3(0.1f, 0.75f, sceneEntities.gameObject.transform.position.z);
        characterState.lastViewPortPosition = characterState.viewPortPosition;

        gameInfo = new GameInfo();
        gameInfo.nextScenePosition = new Vector3(1.2f, characterState.viewPortPosition.y,  characterState.viewPortPosition.z); 

        animTypeToString = animBundle;

        setViewPortPosition(characterState.viewPortPosition);
        startAnimation(AnimationType.Idle);
    }

    private void startAnimation(AnimationType animationType) {
        Debug.Log(animTypeToString[animationType]);

        sceneEntities.animator.StopPlayback();
        sceneEntities.animator.Play(animTypeToString[animationType]);
        characterState.curAnimationType = animationType;
    }

    private void setViewPortPosition(Vector3 vPpos) {
        Vector3 worldPos = sceneEntities.mainCamera.ViewportToWorldPoint(vPpos);
        worldPos.z = vPpos.z;
        Debug.Log("position set : " + worldPos.ToString());
        sceneEntities.gameObject.transform.position = worldPos;
        characterState.viewPortPosition = vPpos;
    }

    public Vector2 getViewPortPosition() {
        return characterState.viewPortPosition;
    }

    public int getAttackDamage() {
        bool willCrit = characterState.critChance >= Random.Range(0.0f, 1.0f);
        if (willCrit) {
             return (int)Mathf.Round((float)characterState.damage * characterState.damageBuff * characterState.critMult * characterState.damageBuff);
        } else {
            return (int)Mathf.Round((float)characterState.damage * characterState.damageBuff);
        }
    }

    public void setEnemy(Character enemy) {
        gameInfo.curEnemy = enemy;
    }

    private void startQueueAction(ActionInfo actionInfo) {
        characterState.actionState = ActionState.InAction;

        if (actionInfo.actionType == ActionType.Move) {
            characterState.isMoving = true;
        }
    }

    private void addAttackAction() {
        int damage = getAttackDamage();
        gameInfo.curEnemy.receiveDamage(damage);

        startAnimation(AnimationType.Attack);
    }

    private void addMoveAction(MoveType moveType) {
        ActionInfo movementInfo = new ActionInfo(ActionType.Move);
        if (moveType == MoveType.ToEnemy) {
            movementInfo.destination = gameInfo.curEnemy.getViewPortPosition(); 
        } else if(moveType == MoveType.ToNextScene) {
            // gameInfo.nextScenePosition = getScene().getNextScenePositionForPlayer(playerId);
            movementInfo.destination = gameInfo.nextScenePosition; 
        } else if(moveType == MoveType.FromEnemy) {
            movementInfo.destination = characterState.lastViewPortPosition;
        } else {
            Debug.LogError("Incorrect MoveType in addMoveAction !");
        }

        characterState.lastViewPortPosition = characterState.viewPortPosition;
        Vector3 toDestination = movementInfo.destination - characterState.viewPortPosition;
        characterState.moveDirection = toDestination.normalized;

        actionsQueue.Add(movementInfo);
    }

    public void receiveDamage(int damage) {
        characterState.HP -= damage;
        startAnimation(AnimationType.IsHit);

        if (characterState.HP <= 0) {
            clearActionQueueAndDie();
        }
    }
    private void clearActionQueueAndDie() {
        actionsQueue.Clear();
        startAnimation(AnimationType.Death);
        characterState.alive = false;
    }

    public void addActionSequence(ActionSequenceType actionSequenceType) {
        switch(actionSequenceType) {
            case ActionSequenceType.HitEnemy :
                addMoveAction(MoveType.ToEnemy);
                addAttackAction();
                addMoveAction(MoveType.FromEnemy);
                break;
            case ActionSequenceType.MoveToNextScene :
                addMoveAction(MoveType.ToNextScene);
                break;

            default :
                Debug.LogError("Incorrect ActionSequenceType in addActionSequence !");
                break;
        }
    }

    public void endAction() {
        Debug.Log(" END ACTION ");

    	characterState.actionState = ActionState.Idle;
    	startAnimation(AnimationType.Idle);
        characterState.isMoving = false;

        if (actionsQueue.Count != 0) {
            actionsQueue.RemoveAt(0);
        }
    }

    private void updateCurrentAction() {
        if (characterState.actionState == ActionState.Idle) {
            return;
        } 

         // update position
        if (characterState.isMoving) {
            // this.gameObject.transform.position = this.gameObject.transform.position;
            Vector3 left = currentActionInfo.destination - characterState.viewPortPosition;
            float distanceLeft = left.magnitude;
            float distanceToTravel = Time.deltaTime * characterState.speed;
            
            if (distanceToTravel > distanceLeft) {
                setViewPortPosition(currentActionInfo.destination);
                endAction();
            } else {
                setViewPortPosition(characterState.viewPortPosition + characterState.moveDirection * distanceToTravel);
            }
        }

        // update animation if still in action. TODO :: CHANGE THIS 
        if ((characterState.curAnimationType != currentActionInfo.associatedAnimType) && (characterState.actionState == ActionState.InAction)) {
            startAnimation(currentActionInfo.associatedAnimType);
        }
    }

    void Update()
    {
        if (characterState.alive == false) {
            return;
        }

        updateCurrentAction();

        // start new action from queue if any. Comes last on update()
        if (characterState.actionState == ActionState.Idle) {
            if (actionsQueue.Count != 0) {
                currentActionInfo = actionsQueue[0];
                startQueueAction(currentActionInfo);
            }
        }
    }
}
