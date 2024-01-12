using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Finds tile matches in a grid.
/// </summary>
public class MatchFinder {
  private readonly string _name;
  private readonly List<Matrix> _patternOrientations;
  protected readonly CellTileFactory _cellTileFactory;

  /// <summary>
  /// Constructs a MatchFinder that searches for a pattern in various orientations.
  /// </summary>
  /// <param name="basePattern">The pattern which is rotated while searching for matches</param>
  /// <param name="orientationCount">The number of orientations of the base pattern to search for</param>
  /// <param name="name">The name of the pattern</param>
  public MatchFinder(Matrix basePattern, int orientationCount, string name,
    CellTileFactory cellTileFactory) {
    _cellTileFactory = cellTileFactory;

    _patternOrientations = new List<Matrix> {basePattern};

    orientationCount = Math.Max(orientationCount, 1);
    Matrix orientation = basePattern;
    for (int i = 1; i < orientationCount; i++) {
      orientation = CreateRotatedPattern(orientation);
      _patternOrientations.Add(orientation);
    }
    _name = name;
  }

  /// <summary>
  /// Gets all the matches in a grid defined by the MatchFinder's pattern in its various orientations.
  /// </summary>
  /// <param name="grid">The grid to search</param>
  public List<Match> GetMatches(EvaluationGrid grid) {
    List<Match> matches = new List<Match>();
    int angle = 0;
    foreach (Matrix orientation in _patternOrientations) {
      Console.WriteLine($"Finding matches for pattern: {_name} @ {angle} degrees");
      matches.AddRange(GetPatternMatches(orientation, grid));
      angle += 360 / _patternOrientations.Count;
    }
    return matches;
  }

  /// <summary>
  /// Gets the cells involved in a match.
  /// </summary>
  /// <param name="pattern">The pattern that defines the match</param>
  /// <param name="grid">The grid the match is in</param>
  /// <param name="col">The starting (leftmost) column of the pattern in the grid</param>
  /// <param name="row">The starting (topmost) row of the pattern in the grid</param>
  protected List<GridCell> GetMatchCells(Matrix pattern, EvaluationGrid grid, int col, int row) {
    List<GridCell> matchCells = new List<GridCell>();
    for (int i = 0; i < pattern.RowCount; i++) {
      for (int j = 0; j < pattern.ColumnCount; j++) {
        if (pattern.Elements[i, j] == 0) continue;
        EvaluationGridCell evaluationCell = grid.Cells[row + i, col + j];
        grid.Cells[row + i, col + j].IsInMatch = true;
        matchCells.Add(evaluationCell.Cell);
      }
    }
    return matchCells;
  }

  /// <summary>
  /// Creates a match from a pattern of cells in a grid.
  /// </summary>
  /// <param name="pattern">The pattern that defines the match</param>
  /// <param name="grid">The grid the match is in</param>
  /// <param name="col">The starting (leftmost) column of the pattern in the grid</param>
  /// <param name="row">The starting (topmost) row of the pattern in the grid</param>
  protected virtual Match CreateMatch(Matrix pattern, EvaluationGrid grid, int col, int row) {
    List<GridCell> matchCells = GetMatchCells(pattern, grid, col, row);
    return new Match(matchCells, _cellTileFactory);
  }

  /// <summary>
  /// Gets the cell responsible for making a match. This would be any changed cell in the match that
  /// neighbors an unchanged cell also in the match. If one cannot be found, a random match cell is
  /// chosen. 
  /// </summary>
  /// <param name="matchCells">The cells in the match</param>
  /// <param name="grid">The grid the match is in</param>
  protected GridCell GetCellThatMadeMatch(List<GridCell> matchCells, EvaluationGrid grid) {
    foreach (GridCell cell in matchCells) {
      int col = cell.Column;
      int row = cell.Row;
      if (!grid.Cells[row, col].WasChanged) continue;
      if (col > 0                    && !grid.Cells[row, col - 1].WasChanged ||
          col < grid.ColumnCount - 1 && !grid.Cells[row, col + 1].WasChanged ||
          row > 0                    && !grid.Cells[row - 1, col].WasChanged ||
          row < grid.RowCount - 1    && !grid.Cells[row + 1, col].WasChanged) {
        return cell;
      }
    }

    // If all the cells are static or were just moved, just pick a random cell in the match
    int index = Random.Range(0, matchCells.Count);
    return matchCells[index];
  }

  /// <summary>
  /// Creates a version of a pattern that's rotated 90 degrees counterclockwise.
  /// </summary>
  /// <param name="pattern">The pattern to rotate</param>
  private Matrix CreateRotatedPattern(Matrix pattern) {
    Matrix rotated = new Matrix(pattern.RowCount, pattern.ColumnCount);
    for (int row = 0; row < rotated.RowCount; row++) {
      for (int col = 0; col < rotated.ColumnCount; col++) {
        rotated.Elements[row, col] = pattern.Elements[col, pattern.ColumnCount - 1 - row];
      }
    }
    return rotated;
  }

  /// <summary>
  /// Gets all the matches in a grid defined by a single pattern.
  /// </summary>
  /// <param name="pattern">The pattern to search for</param>
  /// <param name="grid">The grid to search</param>
  private List<Match> GetPatternMatches(Matrix pattern, EvaluationGrid grid) {
    List<Match> matches = new List<Match>();
    for (int row = 0; row <= grid.RowCount - pattern.RowCount; row++) {
      for (int col = 0; col <= grid.ColumnCount - pattern.ColumnCount; col++) {
        if (!DoesPatternFit(pattern, grid, col, row)) continue;
        Match match = CreateMatch(pattern, grid, col, row);
        Console.WriteLine("Found match: " + match.ToString());
        matches.Add(match);
      }
    }
    return matches;
  }

  /// <summary>
  /// Checks whether a pattern identifies a match in a certain part of a grid.
  /// </summary>
  /// <param name="pattern">The pattern to check for</param>
  /// <param name="grid">The grid to search</param>
  /// <param name="col">The starting (leftmost) column of the pattern in the grid</param>
  /// <param name="row">The starting (topmost) row of the pattern in the grid</param>
  private bool DoesPatternFit(Matrix pattern, EvaluationGrid grid, int col, int row) {
    string targetTileName = null;
    for (int i = 0; i < pattern.RowCount; i++) {
      for (int j = 0; j < pattern.ColumnCount; j++) {
        if (pattern.Elements[i, j] == 0) continue;
        EvaluationGridCell cell = grid.Cells[row + i, col + j];
        if (cell.IsEmpty || cell.IsInMatch) return false;
        if (targetTileName == null) {
          targetTileName = cell.TileName;
        } else if (cell.TileName != targetTileName) {
          return false;
        }
      }
    }
    return true;
  }
}
