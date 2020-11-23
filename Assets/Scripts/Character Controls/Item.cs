using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType {
        Staff,
        Sword,
        Ring
    }

    public enum ItemEnum {
        Staff_Common,
        Staff_Uncommon,
        Staff_Rare,
        Sword_Common,
        Sword_Uncommon,
        Sword_Rare,
        Ring_Common,
        Ring_Uncommon,
        Ring_Rare
    }


    [System.Serializable] public struct StatImpact {
        public Character.CharacterPropertyType statType;
        public float impact; // multiplier for statType property
    }

    public ItemEnum itemEnum;
    public ItemType itemType = ItemType.Staff;
    public Character.CharacterType[] compatibleCharacterTypes;
    public StatImpact[] itemImpact;
}
