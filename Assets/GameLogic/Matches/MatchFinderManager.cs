using System;
using System.Collections.Generic;

/// <summary>
/// Defines and manages all MatchFinders as a group.
/// </summary>
public class MatchFinderManager {
  private readonly List<MatchFinder> _finders;

  public MatchFinderManager(CellTileFactory cellTileFactory) {
    _finders = new List<MatchFinder> {
      new EnhanceableMatchFinder(new Matrix(new int[,] {{1,1,1},
                                                        {0,1,0},
                                                        {0,1,0}}),     4, "T",     cellTileFactory, CellTile.EnhancementType.Legendary),
      new EnhanceableMatchFinder(new Matrix(new int[,] {{1,0,0},
                                                        {1,0,0},
                                                        {1,1,1}}),     4, "L",     cellTileFactory, CellTile.EnhancementType.Legendary),
      new EnhanceableMatchFinder(new Matrix(new int[,] {{1,1},
                                                        {1,1}}),       1, "2x2",   cellTileFactory, CellTile.EnhancementType.Epic),
      new EnhanceableMatchFinder(new Matrix(new int[,] {{1,1,1,1,1}}), 2, "5line", cellTileFactory, CellTile.EnhancementType.Legendary),
      new EnhanceableMatchFinder(new Matrix(new int[,] {{1,1,1,1}}),   2, "4line", cellTileFactory, CellTile.EnhancementType.Super),
      new MatchFinder(           new Matrix(new int[,] {{1,1,1}}),     2, "3line", cellTileFactory)
    };
  }

  /// <summary>
  /// Gets all the matches in a grid surrounding changed cells.
  /// </summary>
  /// <param name="grid">The grid to search for matches</param>
  /// <param name="changedCells">the cells that were changed. Used to determine enhanced tiles as
  /// well as to speed up the search.</param>
  public List<Match> GetMatches(Grid grid, List<GridCell> changedCells) {
    EvaluationGrid evaluationGrid = new EvaluationGrid(grid, changedCells);
    return GetMatches(evaluationGrid);
  }

  /// <summary>
  /// Gets all the matches in a grid, assuming all cells should be checked.
  /// </summary>
  /// <param name="grid">The grid to search for matches</param>
  public List<Match> GetMatches(Grid grid) {
    EvaluationGrid evaluationGrid = new EvaluationGrid(grid);
    return GetMatches(evaluationGrid);
  }

  /// <summary>
  /// Determines whether a grid has any potential matches the user could make by swapping two
  /// adjacent tiles.
  /// </summary>
  /// <param name="grid">The grid to search for potential matches</param>
  public bool HasPotentialMatches(Grid grid) {
    Console.WriteLine("Checking for potential matches");
    EvaluationGrid evalGrid = new EvaluationGrid(grid);

    for (int row = 0; row < evalGrid.RowCount; row++) {
      for (int col = 0; col < evalGrid.ColumnCount; col++) {
        // Try swapping this cell with the one to the right
        if (col < evalGrid.ColumnCount - 1 && SwapCausesMatch(evalGrid.Cells[row, col], evalGrid.Cells[row, col + 1], evalGrid)) {
          Console.WriteLine("There's a potential match if you swap " + evalGrid.Cells[row, col] + " and " + evalGrid.Cells[row, col + 1]);
          return true;
        }
        // Try swapping this cell with the one below
        if (row < evalGrid.RowCount - 1 && SwapCausesMatch(evalGrid.Cells[row, col], evalGrid.Cells[row + 1, col], evalGrid)) {
          Console.WriteLine("There's a potential match if you swap " + evalGrid.Cells[row, col] + " and " + evalGrid.Cells[row + 1, col]);
          return true;
        }
      }
    }

    return false;
  }

  /// <summary>
  /// Gets all the matches in an evaluation grid.
  /// </summary>
  /// <param name="grid">The evaluation grid to search for matches</param>
  private List<Match> GetMatches(EvaluationGrid grid) {
    List<Match> matches = new List<Match>();
    foreach (MatchFinder finder in _finders) {
      matches.AddRange(finder.GetMatches(grid));
    }
    return matches;
  }

  /// <summary>
  /// Determines whether swapping two specific tiles will result in any matches.
  /// </summary>
  /// <param name="cell1">The first cell to swap</param>
  /// <param name="cell2">The second cell to swap</param>
  /// <param name="grid">The grid in which the cells are swapped</param>
  private bool SwapCausesMatch(EvaluationGridCell cell1, EvaluationGridCell cell2, EvaluationGrid grid) {
    grid.SwapTiles(cell1, cell2, true);

    foreach (MatchFinder finder in _finders) {
      List<Match> matches = finder.GetMatches(grid);
      if (matches.Count > 0) {
        grid.SwapTiles(cell1, cell2, false);
        return true;
      }
    }

    grid.SwapTiles(cell1, cell2, false);
    return false;
  }
}
