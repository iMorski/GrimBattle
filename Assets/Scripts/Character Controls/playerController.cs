using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : Character
{
    void Start()
    {
        Character.CharacterStats stats = new Character.CharacterStats(100);
        stats.team = Game.teamType.Players;
        stats.HP = 150;
        stats.damage = 40;
        stats.critChance = 0.7f;

        this.init(stats);
        this.setName("PLAYER");
    }
}
