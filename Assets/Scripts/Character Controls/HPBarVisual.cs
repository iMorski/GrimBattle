using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HPBarVisual : Visual
{
    private float DAMAGE_STICKY_DURATION = 0.1f;
    private static float DAMAGE_SHOW_LERP_FACTOR = 0.02f;
    private float HPBAR_VIEWPORT_HEIGHT = 0.01f; // TODO : CALCULATE THIS 
    private float OFFSET_LENGTH_MULTIPLIER = 0.4f;
    private float FLOATING_TEXT_SPEED = 0.01f;
    private float FLOATING_TEXT_TIME_BEFORE_FADE = 0.6f;
    private static float FLOATING_TEXT_FADESPEED = 1.0f;
    private float CRIT_PAUSE_TIME = 0.3f;
    private float DAMAGE_FONTSIZE = 2.5f;
    private float CRIT_FONTSIZE = 4.0f;
    private Character character;
    private int currentShownHP = -1;
    private float trailingHP = -1.0f;
    private float maxHP = 100;

    private SpriteRenderer hpBar;
    private float damageStickyStartTime = -1.0f;

    private GameObject textPrefab;

    private class MovingDamageInstance { 
        private float DAMAGE_TEXT_LERP_FACTOR = 0.02f;
        private float EPS = 0.001f;
        public MovingDamageInstance(GameObject o, Vector3 viewPortPos, float upLen, float time, Character.Damage d) {
            gameObject = o;
            vpPos = viewPortPos;
            upLenVP = upLen;
            timeCreated = time;
            damage = d;
        }

        public bool updateAlpha(float targetAlpha) {
            Color curColor = gameObject.GetComponent<TextMeshPro>().color;
            float a = curColor.a;
            if (Mathf.Abs(a - targetAlpha) < EPS) {
                return true;
            }
            a = Mathf.Lerp(curColor.a, targetAlpha, DAMAGE_TEXT_LERP_FACTOR / FLOATING_TEXT_FADESPEED);
            if (Mathf.Abs(a - targetAlpha) < EPS) {
                a = targetAlpha;
            }
            curColor.a = a;
            gameObject.GetComponent<TextMeshPro>().color = curColor;

            return false;
        }

        public GameObject gameObject;
        public Vector3 vpPos;
        public float upLenVP; // vector that is used for upwards movement
        public float timeCreated;

        public Character.Damage damage;

    }

    List<MovingDamageInstance> dependentDamageText = new List<MovingDamageInstance>();

    public void init(Character c, GameObject prefab) {
        character = c;
        textPrefab = prefab;
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

    private void generateDamageText(Character.Damage damageInfo) {
        var transform = this.gameObject.GetComponent<Transform>();

        Vector2 hpBarDimensions = new Vector2(transform.lossyScale.x, transform.lossyScale.y);

        GameObject damageTextObject = Instantiate(textPrefab, this.gameObject.transform);
        damageTextObject.transform.localScale = new Vector2(1.0f, 1.0f) / hpBarDimensions;

        TextMeshPro tmp = damageTextObject.GetComponent<TextMeshPro>();
        if (damageInfo.damage > 0) {
            tmp.text = "-" + damageInfo.damage.ToString();
        } else { 
            tmp.text = "+" + (-damageInfo.damage).ToString();
        }

        Vector2 r = Random.insideUnitCircle;
        Vector3 offset = new Vector3(r.x, r.y, 0.0f);
        offset.y = Mathf.Abs(offset.y) + HPBAR_VIEWPORT_HEIGHT;
        offset = offset.normalized;
        
        Vector3 hpBarVPPos = character.getGameCamera().WorldToViewportPoint(hpBar.transform.position);
        Vector3 characterVPPos = character.getViewPortPosition();
  
        Vector3 VPHeight = (hpBarVPPos - characterVPPos);
        float desiredMaxLen = VPHeight.magnitude * OFFSET_LENGTH_MULTIPLIER;

        float offsetLen = Random.Range(desiredMaxLen / 2.0f, desiredMaxLen);
        offset *= offsetLen;

        Vector3 vpPos = hpBarVPPos + offset;
        setViewPortPosition(hpBarVPPos + offset, damageTextObject);

        MovingDamageInstance instance = new MovingDamageInstance(damageTextObject, vpPos, offsetLen * FLOATING_TEXT_SPEED, Time.realtimeSinceStartup, damageInfo); 
        dependentDamageText.Add(instance); 
    }

    private void setViewPortPosition(Vector3 vPpos, GameObject gObject) {
        Vector3 worldPos = character.getGameCamera().ViewportToWorldPoint(vPpos);
        gObject.transform.position = worldPos;
    }

    public void invokeDamage(Character.Damage damage) {
        int hp = character.getCurrentHP();

        generateDamageText(damage);

        currentShownHP = hp;
        damageStickyStartTime = Time.realtimeSinceStartup;
        updateHPBarValues();
    }

    private void updateHPBarValues() {
        float curHpPercentage = (float)currentShownHP/(float)maxHP;
        float lastHpPercentage = trailingHP/(float)maxHP;

        hpBar.material.SetFloat("_hpPercentage", curHpPercentage);
        hpBar.material.SetFloat("_lastHpPercentage", lastHpPercentage);
        hpBar.material.SetFloat("_maxHP", (float)maxHP);
        hpBar.material.SetFloat("_hpBlock", 50.0f);
    }

    private void updateHpBarVisual() {
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

    private float mix(float a, float b, float m) {
        return a * (1.0f - m) + b * m;
    }
    private float saturate (float a) {
        if (a > 1.0f) {
            return 1.0f;
        } else if (a < 0.0f) {
            return 0.0f;
        } else return a;
    }
    private void updateFloatingText() {
        for (int i = 0; i < dependentDamageText.Count; ++i) {

            float isCrit = (dependentDamageText[i].damage.isCrit) ? 1.0f : 0.0f;

            // scale font up if isCrit
            if (dependentDamageText[i].damage.isCrit) {
                float fontSize = dependentDamageText[i].gameObject.GetComponent<TextMeshPro>().fontSize;
                if (Mathf.Abs(fontSize - CRIT_FONTSIZE) > 0.001f) {
                    float mixC = saturate((Time.realtimeSinceStartup - dependentDamageText[i].timeCreated) / CRIT_PAUSE_TIME);
                    fontSize = mix(fontSize, CRIT_FONTSIZE, mixC);
                    dependentDamageText[i].gameObject.GetComponent<TextMeshPro>().fontSize = fontSize;
                }
            }

            // update position 
            if (Time.realtimeSinceStartup - dependentDamageText[i].timeCreated > CRIT_PAUSE_TIME * isCrit) {
                Vector3 newVpPos = dependentDamageText[i].vpPos;
                newVpPos.y += dependentDamageText[i].upLenVP;
                setViewPortPosition(newVpPos, dependentDamageText[i].gameObject);
                dependentDamageText[i].vpPos = newVpPos;
            }
            
            // update alpha and existence
            if (Time.realtimeSinceStartup - dependentDamageText[i].timeCreated > (FLOATING_TEXT_TIME_BEFORE_FADE + CRIT_PAUSE_TIME * isCrit)) {
                bool reachedAlphaTarget = dependentDamageText[i].updateAlpha(0.0f);
                if (reachedAlphaTarget) {
                    Destroy(dependentDamageText[i].gameObject);
                    dependentDamageText.RemoveAt(i);
                }
            }
        }
    }

    void Update()
    {
        updateHpBarVisual();
        updateFloatingText();
    }
}
