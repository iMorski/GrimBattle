using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class monsterController : Character
{
    void Start()
    {
        Character.CharacterStats stats = new Character.CharacterStats(100);
        stats.team = Game.TeamType.Monsters;
        stats.damage = 20;
        stats.critChance = 0.4f;
        stats.HP = 400;

        this.init(stats);
        this.setName("MONSTER");
    }
}
