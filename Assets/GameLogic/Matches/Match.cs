using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A collection of tiles of the same type arranged in a special pattern relative to each other in a grid.
/// </summary>
public class Match {
  /// <summary>
  /// Cells that made the match.
  /// </summary>
  public List<GridCell> Cells;
  /// <summary>
  /// Cells involved in the resolution of the match.
  /// </summary>
  public List<GridCell> ResolvedCells;

  protected readonly CellTileFactory _cellTileFactory;

  public Match(List<GridCell> cells, CellTileFactory cellTileFactory) {
    Cells = cells;
    ResolvedCells = new List<GridCell>();
    _cellTileFactory = cellTileFactory;
  }

  /// <summary>
  /// Gets the cells in the match that should be resolved.
  /// </summary>
  public virtual List<GridCell> GetCellsToResolve() {
    return Cells;
  }

  /// <summary>
  /// Behavior to perform when the match is made. Returns a list of all cells resolved.
  /// </summary>
  public virtual List<GridCell> Resolve(Grid grid) {
    return ResolveCells(grid);
  }

  public override string ToString() {
    string str = $"{Cells.Count} {Cells[0].Tile.Name}:";
    foreach (GridCell cell in Cells) {
      str += $" ({cell.Column},{cell.Row})";
    }
    return str;
  }

  /// <summary>
  /// Resolves the individual cells in the match.
  /// </summary>
  /// <param name="grid"></param>
  /// <returns>The cells resolved by the match.</returns>
  protected List<GridCell> ResolveCells(Grid grid) {
    var resolvedCells = new HashSet<GridCell>();
    foreach (GridCell cell in Cells) {
      ResolveCell(cell, grid, resolvedCells);
    }
    foreach (GridCell resolvedCell in resolvedCells) {
      resolvedCell.Tile = null;
    }
    return resolvedCells.ToList();
  }

  /// <summary>
  /// Resolves a cell and any others that should be resolved along with it.
  /// </summary>
  /// <param name="cell">The cell to resolve</param>
  /// <param name="grid">The grid to resolve the cell in</param>
  /// <param name="resolvedCells">A set of cells that have been resolved so far</param>
  /// <returns>The set of resolved cells</returns>
  private HashSet<GridCell> ResolveCell(GridCell cell, Grid grid, HashSet<GridCell> resolvedCells) {
    // Don't resolve this cell if it's empty or has already been resolved
    if (cell.IsEmpty || resolvedCells.Contains(cell)) return resolvedCells;

    // Otherwise, mark this cell as resolved
    resolvedCells.Add(cell);

    // A super tile will resolve its neighboring cells
    if (cell.Tile.Enhancement == CellTile.EnhancementType.Super) {
      if (cell.Column > 0)                    ResolveCell(grid.Cells[cell.Row, cell.Column - 1], grid, resolvedCells);           
      if (cell.Column < grid.ColumnCount - 1) ResolveCell(grid.Cells[cell.Row, cell.Column + 1], grid, resolvedCells);
      if (cell.Row > 0)                       ResolveCell(grid.Cells[cell.Row - 1, cell.Column], grid, resolvedCells);          
      if (cell.Row < grid.RowCount - 1)       ResolveCell(grid.Cells[cell.Row + 1, cell.Column], grid, resolvedCells);
    }

    // An epic tile will resolve all cells in its row
    else if (cell.Tile.Enhancement == CellTile.EnhancementType.Epic) {
      for (int col = 0; col < grid.ColumnCount; col++) {
        ResolveCell(grid.Cells[cell.Row, col], grid, resolvedCells);
      }
    }

    // A legendary tile will resolve all cells in the grid with the same tile type
    else if (cell.Tile.Enhancement == CellTile.EnhancementType.Legendary) {
      for (int row = 0; row < grid.RowCount; row++) {
        for (int col = 0; col < grid.ColumnCount; col++) {
          GridCell potentialCell = grid.Cells[row, col];
          if (!potentialCell.IsEmpty && potentialCell.Tile.Name == cell.Tile.Name) {
            ResolveCell(grid.Cells[row, col], grid, resolvedCells);
          } 
        }
      }
    }

    return resolvedCells;
  }
}
