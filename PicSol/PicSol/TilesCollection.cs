namespace PicSol
{
    /// <summary>
    /// A 2D Collection of tiles, to avoid having to work with nested arrays.
    /// </summary>
    public class TilesCollection : ReadOnlyTilesCollection
    {
        public TilesCollection(int rowCount, int colCount) : base(rowCount, colCount)
        {
            Clear();
        }

        public void SetTile(int row, int col, bool value)
        {
            Tiles[row][col] = value;
        }

        public ReadOnlyTilesCollection AsReadOnly() => new ReadOnlyTilesCollection(this);
        public void Clear() => ClearInternal();
    }

    /// <summary>
    /// A read-only 2D Collection of tiles, to avoid having to work with nested arrays.
    /// </summary>
    public class ReadOnlyTilesCollection
    {
        protected bool[][] Tiles;
        public int RowCount { get; }
        public int ColumnCount { get; }

        protected ReadOnlyTilesCollection(int rowCount, int colCount)
        {
            RowCount = rowCount;
            ColumnCount = colCount;
            ClearInternal();
        }

        public ReadOnlyTilesCollection(TilesCollection parent) : this(parent.RowCount, parent.ColumnCount)
        {
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    Tiles[row][col] = parent[row, col];
                }
            }
        }

        public bool this[int row, int col]
        {
            get { return Tiles[row][col]; }
        }

        protected void ClearInternal()
        {
            Tiles = new bool[RowCount][];
            for (int i = 0; i < RowCount; i++)
            {
                Tiles[i] = new bool[ColumnCount];
            }
        }
    }
}
