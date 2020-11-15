using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarVisual : Visual
{
    private float DAMAGE_STICKY_DURATION = 0.1f;
    private float DAMAGE_SHOW_LERP_FACTOR = 0.02f;
    private Character character;
    private int currentShownHP = -1;
    private float trailingHP = -1.0f;
    private float maxHP = 100;

    private SpriteRenderer hpBar;
    private float damageStickyStartTime = -1.0f;

    public void setCharacterAndInit(Character c) {
        character = c;
        hpBar = this.gameObject.GetComponent<SpriteRenderer>();

        character.registerVisual(this);
        maxHP = character.getMaxHP();
        currentShownHP = character.getCurrentHP();
        trailingHP = (float)currentShownHP;

        updateHPBarValues();
    }

    public override void setAlpha(float a)  {
        hpBar.material.SetFloat("_alpha", a);
    }

    private void captureNewDamageInstance() {
        int hp = character.getCurrentHP();
        if (currentShownHP == hp) {
            return;
        }
        print("new damage inc");

        currentShownHP = hp;
        damageStickyStartTime = Time.realtimeSinceStartup;
        updateHPBarValues();
    }

    private void updateHPBarValues() {
        
        float curHpPercentage = (float)currentShownHP/(float)maxHP;
        float lastHpPercentage = trailingHP/(float)maxHP;

        print ("Current HP : " + curHpPercentage.ToString()) ;

        hpBar.material.SetFloat("_hpPercentage", curHpPercentage);
        hpBar.material.SetFloat("_lastHpPercentage", lastHpPercentage);
    }

    private void updateVisual() {
        if (damageStickyStartTime > 0.0f) {
            float timeElapsed = Time.realtimeSinceStartup;
            if ((timeElapsed - damageStickyStartTime) < DAMAGE_STICKY_DURATION) {
                return;
            } else {
                damageStickyStartTime = -1.0f;
            }
        }
        
        if ((int)trailingHP == currentShownHP) {
            return;
        }

        trailingHP = Mathf.Lerp(trailingHP, currentShownHP, DAMAGE_SHOW_LERP_FACTOR);
        if ((int)trailingHP == currentShownHP) {
            trailingHP = (float)currentShownHP;
        }
        updateHPBarValues();
    }

    void Update()
    {

        updateVisual();
        captureNewDamageInstance();
        
    }
}
