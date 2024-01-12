using System.Collections.Generic;

/// <summary>
/// A MatchFinder that replaces the tile that made the match with an enhanced version of that tile.
/// </summary>
public class EnhanceableMatchFinder : MatchFinder {
  private readonly CellTile.EnhancementType _enhancement;

  public EnhanceableMatchFinder(Matrix basePattern, int orientationCount, string name,
    CellTileFactory cellTileFactory, CellTile.EnhancementType enhancement)
      : base(basePattern, orientationCount, name, cellTileFactory) {
    _enhancement = enhancement;
  }

  protected override Match CreateMatch(Matrix pattern, EvaluationGrid grid, int col, int row) {
    List<GridCell> matchCells = GetMatchCells(pattern, grid, col, row);
    GridCell cellToEnhance = GetCellThatMadeMatch(matchCells, grid);
    return new EnhanceableMatch(matchCells, _cellTileFactory, cellToEnhance, _enhancement);
  }
}
