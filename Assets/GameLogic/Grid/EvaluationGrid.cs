using System.Collections.Generic;

/// <summary>
/// A grid used exclusively for finding matches.
/// </summary>
public class EvaluationGrid {
  public readonly EvaluationGridCell[,] Cells;
  public int ColumnCount, RowCount;

  public EvaluationGrid(Grid grid, List<GridCell> changedCells) {
    ColumnCount = grid.ColumnCount;
    RowCount = grid.RowCount;

    Cells = new EvaluationGridCell[RowCount, ColumnCount];
    for (int row = 0; row < RowCount; row++) {
      for (int col = 0; col < ColumnCount; col++) {
        Cells[row, col] = new EvaluationGridCell(grid.Cells[row, col]);
      }
    }

    // Mark cells that were changed
    foreach (GridCell cell in changedCells) {
      Cells[cell.Row, cell.Column].WasChanged = true;
    }
  }

  /// <summary>
  /// Constructs a grid that assumes every cell was changed.
  /// </summary>
  /// <param name="grid"></param>
  public EvaluationGrid(Grid grid) {
    ColumnCount = grid.ColumnCount;
    RowCount = grid.RowCount;

    Cells = new EvaluationGridCell[RowCount, ColumnCount];
    for (int row = 0; row < RowCount; row++) {
      for (int col = 0; col < ColumnCount; col++) {
        Cells[row, col] = new EvaluationGridCell(grid.Cells[row, col]);
        Cells[row, col].WasChanged = true;
      }
    }
  }

  /// <summary>
  /// Swaps the tiles of two cells.
  /// </summary>
  /// <param name="cell1">The first cell to swap</param>
  /// <param name="cell2">The second cell to swap</param>
  /// <param name="markAsChanged">Whether to mark the swapped cells as "changed"</param>
  public void SwapTiles(EvaluationGridCell cell1, EvaluationGridCell cell2, bool markAsChanged) {
    CellTile tile1 = Cells[cell1.Row, cell1.Column].Cell.Tile;
    CellTile tile2 = Cells[cell2.Row, cell2.Column].Cell.Tile;

    cell1.Cell.Tile = tile2;
    cell1.WasChanged = markAsChanged;

    cell2.Cell.Tile = tile1;
    cell2.WasChanged = markAsChanged;
  }

  public override string ToString() {
    string str = "EvalGrid:";
    for (int row = 0; row < RowCount; row++) {
      str += "\n  ";
      for (int col = 0; col < ColumnCount; col++) {
        str += Cells[row, col] + " ";
      }
    }
    return str;
  }
}
