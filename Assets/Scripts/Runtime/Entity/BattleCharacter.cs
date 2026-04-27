using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleCharacter : MonoBehaviour
{
    public int Speed;

    public int GetBuffValue(BuffType buff)
    {
        return 0;
    }
}
