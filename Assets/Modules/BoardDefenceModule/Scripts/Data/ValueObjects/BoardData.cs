using System;
using UnityEngine;

namespace BoardDefence.Data
{
    [Serializable]
    public class BoardData
    {
        public int Columns = 4;
        public int Rows = 8;
        public int PlaceableRowsFromBottom = 4;
        public float CellSize = 1f;
        public float CellSpacing = 0.1f;
        public Color PlaceableZoneColor = new Color(0.2f, 0.5f, 0.2f, 0.3f);
        public Color NonPlaceableZoneColor = new Color(0.5f, 0.2f, 0.2f, 0.3f);
    }
}
