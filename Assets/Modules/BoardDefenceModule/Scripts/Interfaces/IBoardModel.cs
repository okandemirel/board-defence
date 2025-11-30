using Strada.Core.Patterns.Interfaces;
using Strada.Core.Sync;

namespace BoardDefence.Models
{
    public interface IBoardModel : IModel
    {
        int Columns { get; }
        int Rows { get; }
        int PlaceableRowCount { get; }
        float CellSize { get; }

        bool CanPlace(int column, int row);
        void Place(int column, int row, EntityHandle handle);
        void Remove(int column, int row);
        bool HasDefence(int column, int row);
        EntityHandle GetDefenceAt(int column, int row);
        void Reset();
    }
}
