using System;
using UnityEngine;

namespace BoardDefence.Data
{
    public enum AttackDirection : byte
    {
        Forward,
        AllAround
    }

    [Serializable]
    public class DefenceItemData
    {
        public int Id;
        public int Damage;
        public float Range;
        public float AttackInterval;
        public AttackDirection Direction;
        public float ProjectileSpeed = 10f;
        public Color CardColor = new Color(0.2f, 0.6f, 0.9f);
    }

    [Serializable]
    public class DefenceEntry
    {
        public string Key;
        public DefenceItemData Value;
    }
}
