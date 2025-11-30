using Strada.Core.Patterns;
using Strada.Core.DI.Attributes;
using Strada.Core.Sync;
using BoardDefence.Data;

namespace BoardDefence.Models
{
    public class BoardModel : Model, IBoardModel
    {
        [Inject] private BoardData _boardData;

        private EntityHandle[,] _grid;

        public int Columns => _boardData?.Columns ?? 4;
        public int Rows => _boardData?.Rows ?? 8;
        public int PlaceableRowCount => _boardData?.PlaceableRowsFromBottom ?? 4;
        public float CellSize => _boardData?.CellSize ?? 1f;

        protected override void OnInitialize()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            _grid = new EntityHandle[Columns, Rows];
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    _grid[x, y] = EntityHandle.Invalid;
                }
            }
        }

        public bool CanPlace(int column, int row)
        {
            if (column < 0 || column >= Columns) return false;
            if (row < 0 || row >= Rows) return false;
            if (row >= PlaceableRowCount) return false;
            if (_grid[column, row].IsValid) return false;
            return true;
        }

        public void Place(int column, int row, EntityHandle handle)
        {
            if (column < 0 || column >= Columns) return;
            if (row < 0 || row >= Rows) return;
            _grid[column, row] = handle;
        }

        public void Remove(int column, int row)
        {
            if (column < 0 || column >= Columns) return;
            if (row < 0 || row >= Rows) return;
            _grid[column, row] = EntityHandle.Invalid;
        }

        public bool HasDefence(int column, int row)
        {
            if (column < 0 || column >= Columns) return false;
            if (row < 0 || row >= Rows) return false;
            return _grid[column, row].IsValid;
        }

        public EntityHandle GetDefenceAt(int column, int row)
        {
            if (column < 0 || column >= Columns) return EntityHandle.Invalid;
            if (row < 0 || row >= Rows) return EntityHandle.Invalid;
            return _grid[column, row];
        }

        public void Reset()
        {
            InitializeGrid();
        }
    }
}
