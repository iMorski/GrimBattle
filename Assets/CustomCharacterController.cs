using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    public class Character {
    	public uint HP = 100;
    	public float speed = 1.0f;

    	// ADD UNITY API -- PASS ANIMATION OBJECT TO SCRIPT
    	public Animation animationObject;

		public enum AnimationTypes {
			Idle,
			Run,
			IsHit,
			Death,
            Attack
		}    	

		public enum SceenMoveStates {
			Moving,
			Static
		}

		public enum ActionTypes {
			StartRunning,
			StartHitting
		}

		public enum ActionStates {
			Idle,
			InAction
		}

		public Dictionary<AnimationTypes, Animation> animationTypeToObject = new Dictionary<AnimationTypes, Animation>();
		public AnimationTypes curAnimationType = AnimationTypes.Idle;
		public SceenMoveStates screenMoveState = SceenMoveStates.Static;
		public ActionStates actionState = ActionStates.Idle;

    	public Character(uint health, float movSpeed, Dictionary<string, Animation> animationsBundle) {
    		HP = health;
    		speed = movSpeed;
    		curAnimationType = AnimationTypes.Idle;
    		screenMoveState = SceenMoveStates.Static;

    		// animationObject = anim;
            populateAnimationsDictionary(animationsBundle);
    	}

        public void populateAnimationsDictionary(Dictionary<string, Animation> animationsBundle) {
            foreach (AnimationTypes animationType in (AnimationTypes[]) System.Enum.GetValues(typeof(AnimationTypes)))
            {
                Animation a = animationsBundle[animationType.ToString()];
                if (a == null) {
                    Debug.LogError("No animation with name " + animationType.ToString() + " in bundle.");
                } else {
                    Debug.Log("Animation " + animationType.ToString() + " set.");
                    animationTypeToObject.Add(animationType, a);
                }
            }
        }

    	public void startAnimation(AnimationTypes animationType) {
    		// UNITY API TO GO HERE -- START ANIMATION BASED ON animationType 
    		// startAnimation(animationTypeToObject[animationType]);
    	}

    	public void stopCurrentAnimation() {
    		// UNITY API TO GO HERE -- STOP ANIMATION BASED ON ACTIVE ANIMATION
    		// stopAnimation(curAnimationType);
    	}

    	public bool startAction(ActionTypes actionType) {
    		if (actionState == ActionStates.InAction) {

    			// UNITY LOG << "TRYING TO PERFORM ACTION BEFORE THE OTHER ONE IS FINISHED"

    			return false;
    		}

    		actionState = ActionStates.InAction;

    		stopCurrentAnimation();

    		switch (actionType) {
    			case (ActionTypes.StartRunning) :
    			break;

    			case (ActionTypes.StartHitting) :
    			break;
    		}

            return true;
    	}

    	public void endAction(ActionTypes actionType) {
    		actionState = ActionStates.Idle;
    		// START IDLE ANIMATION ? 
    	}
    }

    public Animation idle;
    public Animation run;
    public Animation attack;
    public Animation death;
    public Animation isHit;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string, Animation> animationsBundle = new Dictionary<string, Animation>();
        animationsBundle.Add("Idle", idle);
        animationsBundle.Add("Run", run);
        animationsBundle.Add("IsHit", isHit);
        animationsBundle.Add("Death", death);
        animationsBundle.Add("Attack", attack);

        Character ch = new Character(100, 1.0f, animationsBundle);
    }

    // Update is called once per frame
    void Update()
    {
;
    }
}
