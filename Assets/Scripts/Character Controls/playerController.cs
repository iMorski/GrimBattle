using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : Character
{
    void Start()
    {
        Character.CharacterStats stats = new Character.CharacterStats(100);
        stats.team = Game.teamType.Players;

        this.init(stats);
        this.setName("PLAYER");
    }
}
