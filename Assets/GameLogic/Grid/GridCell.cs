using System;

/// <summary>
/// Represents a cell in a grid. Holds information about its position as well as the tile it
/// contains.
/// </summary>
public class GridCell {
  public int Column, Row;
  public CellTile Tile;

  public GridCell(int column, int row) {
    Column = column;
    Row = row;
  }

  public bool IsNextToCell(GridCell other) =>
    (Row    == other.Row    && Math.Abs(Column - other.Column) == 1) ||
    (Column == other.Column && Math.Abs(Row - other.Row)       == 1);

  public override string ToString() =>
    $"([{Column},{Row}]:" + (IsEmpty ? "null" : Tile.Name) + ")";

  public bool IsEmpty => Tile == null;
}
