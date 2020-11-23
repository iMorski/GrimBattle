using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : Character
{
    void Start()
    {
        Character.BaseStatsCharacter stats = new Character.BaseStatsCharacter(100);
        stats.team = Game.TeamType.Players;
        stats.maxHP = 150;
        stats.damage = 40;
        stats.critChance = 0.7f;

        this.init(stats);
        this.setName("PLAYER");
    }
}
