using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a grid of cells.
/// </summary>
public class Grid {
  public int RowCount, ColumnCount;
  public readonly GridCell[,] Cells;

  private readonly MatchFinderManager _matchFinder;
  private readonly CellTileFactory _cellTileFactory;

  private const int MaxFillAttemptCount = 10;

  public Grid(int columnCount, int rowCount, MatchFinderManager matchFinder,
    CellTileFactory cellTileFactory) {
    _matchFinder = matchFinder;
    _cellTileFactory = cellTileFactory;

    RowCount = rowCount;
    ColumnCount = columnCount;

    Cells = new GridCell[RowCount, ColumnCount];
    _matchFinder = matchFinder;
    for (int row = 0; row < RowCount; row++) {
      for (int col = 0; col < ColumnCount; col++) {
        Cells[row, col] = new GridCell(col, row);
      }
    }
  }

  /// <summary>
  /// Fills all empty cells with new tiles, ensuring there's at least one potential match to make
  /// afterward.
  /// </summary>
  /// <param name="allowMatches">whether or not the grid should be allowed to have matches after
  /// getting filled</param>
  /// <returns>The cells that got filled</returns>
  public void FillEmptyCells(bool allowMatches) {
    Debug.Log("Filling empty cells");

    List<GridCell> emptyCells = GetEmptyCells();
    if (emptyCells.Count == 0) return;

    for (int i = 0; i < MaxFillAttemptCount; i++) {
      List<GridCell> changedCells = new List<GridCell>();
      NaivelyFillEmptyCells(changedCells);
      if (!allowMatches) {
        PreventMatches(changedCells);
      }
      if (_matchFinder.HasPotentialMatches(this)) {
        Debug.Log($"Filled grid in {i + 1} tries");
        return;
      }
      ClearCells(emptyCells);
    }

    Debug.LogError($"Could not fill grid in {MaxFillAttemptCount} tries");
  }

  /// <summary>
  /// Gets all matches in the grid.
  /// </summary>
  /// <param name="changedCells">The cells that were changed. Used to determine enhanced tiles as
  /// well as to speed up the search</param>
  public List<Match> GetMatches(List<GridCell> changedCells) {
    return _matchFinder.GetMatches(this, changedCells);
  }

  /// <summary>
  /// Gets all matches in the grid.
  /// </summary>
  public List<Match> GetMatches() {
    return _matchFinder.GetMatches(this);
  }

  /// <summary>
  /// Swaps the tiles of two cells.
  /// </summary>
  /// <param name="cell1">First cell to swap</param>
  /// <param name="cell2">Second cell to swap</param>
  public void SwapTiles(GridCell cell1, GridCell cell2) {
    CellTile tile1 = Cells[cell1.Row, cell1.Column].Tile;
    CellTile tile2 = Cells[cell2.Row, cell2.Column].Tile;

    cell1.Tile = tile2;
    if (tile2 != null) {
      tile2.Cell = cell1;
    }

    cell2.Tile = tile1;
    if (tile1 != null) {
      tile1.Cell = cell2;
    }
  }

  /// <summary>
  /// Drops all tiles down so there are no empty cells beneath them. Cells whose contents change
  /// because of this are added to a running list of changed cells.
  /// </summary>
  /// <param name="changedCells">a list of cells that have changed</param>
  public void Collapse(List<GridCell> changedCells) {
    Debug.Log("Collapsing grid");
    for (int col = 0; col < ColumnCount; col++) {
      CollapseColumn(col, changedCells);
    }
  }

  /// <summary>
  /// Gets a list of all the empty cells in the grid.
  /// </summary>
  public List<GridCell> GetEmptyCells() {
    List<GridCell> emptyCells = new List<GridCell>();
    for (int row = 0; row < RowCount; row++) {
      for (int col = 0; col < ColumnCount; col++) {
        if (Cells[row, col].IsEmpty) {
          emptyCells.Add(Cells[row, col]);
        }
      }
    }
    return emptyCells;
  }

  /// <summary>
  /// Gets a dictionary of all the empty cells in the grid, mapping each column with empty cells to
  /// a list of those empty cells.
  /// </summary>
  public Dictionary<int, List<GridCell>> GetEmptyCellsByColumn() {
    var emptyCells = new Dictionary<int, List<GridCell>>();
    for (int col = 0; col < ColumnCount; col++) {
      for (int row = 0; row < RowCount; row++) {
        if (Cells[row, col].IsEmpty) {
          if (!emptyCells.ContainsKey(col)) {
            emptyCells.Add(col, new List<GridCell>());
          }
          emptyCells[col].Add(Cells[row, col]);
        }
      }
    }
    return emptyCells;
  }

  public override string ToString() {
    string str = "Grid:";
    for (int row = 0; row < RowCount; row++) {
      str += "\n  ";
      for (int col = 0; col < ColumnCount; col++) {
        str += Cells[row, col] + " ";
      }
    }
    return str;
  }

  /// <summary>
  /// Fills the empty cells of a grid with new random tiles. No strategy involved in choosing them. 
  /// </summary>
  /// <param name="changedCells">A list of cells that have changed</param>
  private void NaivelyFillEmptyCells(List<GridCell> changedCells) {
    Debug.Log("Naively filling empty cells");

    // Fill by column to help determine tile drop position
    for (int col = 0; col < ColumnCount; col++) {
      int emptyCount = 0;
      for (int row = RowCount - 1; row >= 0; row--) {
        GridCell cell = Cells[row, col];
        if (!cell.IsEmpty) continue;

        CellTile tile = _cellTileFactory.CreateRandomCellTile(cell);
        cell.Tile = tile;
        changedCells.Add(cell);

        emptyCount++;
      }
    }
  }

  /// <summary>
  /// Prevents matches in the grid by repeatedly removing matched cells, collapsing the grid, and
  /// refilling empty cells until no matches are present.
  /// </summary>
  /// <param name="changedCells">A list of cells that have changed</param>
  private void PreventMatches(List<GridCell> changedCells) {
    Debug.Log("Preventing matches");
    List<Match> matches = _matchFinder.GetMatches(this, changedCells);
    while (matches.Count > 0) {
      List<GridCell> allMatchedCells = new List<GridCell>();
      foreach (Match match in matches) {
        allMatchedCells.AddRange(match.Cells);
      }
      ClearCells(allMatchedCells);
      Collapse(changedCells);
      NaivelyFillEmptyCells(changedCells);
      matches = GetMatches(changedCells);
    }
  }

  /// <summary>
  /// Drops all cells in a column down so there are no empty cells beneath them. Cells whose
  /// contents change because of this are added to a running list of changed cells.
  /// </summary>
  /// <param name="col">The index of the column to collapse</param>
  /// <param name="changedCells">A list of cells that have changed</param>
  private void CollapseColumn(int col, List<GridCell> changedCells) {
    HashSet<int> changedRows = new HashSet<int>();
    GridCell lowestEmptyCell = null;
    for (int row = RowCount - 1; row >= 0; row--) {
      GridCell cell = Cells[row, col];

      if (cell.IsEmpty) {
        if (lowestEmptyCell == null) {
          lowestEmptyCell = cell;
        }
        continue;
      } else {
        if (lowestEmptyCell == null) continue;

        cell.Tile.Cell = lowestEmptyCell;
        lowestEmptyCell.Tile = cell.Tile;
        cell.Tile = null;

        changedRows.Add(lowestEmptyCell.Row);
        changedRows.Add(row);

        lowestEmptyCell = Cells[lowestEmptyCell.Row - 1, col];
      }
    }

    foreach (int row in changedRows) {
      changedCells.Add(Cells[row, col]);
    }
  }

  /// <summary>
  /// Clears out the tiles from the given cells.
  /// </summary>
  /// <param name="cells">The cells to clear out</param>
  private void ClearCells(List<GridCell> cells) {
    Debug.Log("Clearing cells");
    string str = "";
    foreach (GridCell cell in cells) {
      str += cell.ToString();
      cell.Tile = null;
    }
    Debug.Log("Cleared cells: " + str);
  }
}
