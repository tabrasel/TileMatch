using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A match that will "enhance" the tile that was just moved in order to complete the match. For example, if you move a
/// tile to complete an L-shape match, that tile will remain and get enhanced when the match is resolved.
/// </summary>
public class EnhanceableMatch : Match {
  private readonly GridCell _cellToEnhance;
  private readonly CellTile.EnhancementType _enhancement;

  public EnhanceableMatch(List<GridCell> cells, CellTileFactory cellTileFactory, GridCell cellToEnhance, 
    CellTile.EnhancementType enhancement) : base(cells, cellTileFactory) {
    _cellToEnhance = cellToEnhance;
    _enhancement = enhancement;
  }

  public override List<GridCell> GetCellsToResolve() {
    return Cells
      .Where(cell => !(cell.Column == _cellToEnhance.Column && cell.Row == _cellToEnhance.Row))
      .ToList();
  }

  public override List<GridCell> Resolve(Grid grid) {
    string tileName = _cellToEnhance.Tile.Name;
    CellTile enhancedTile = _cellTileFactory.CreateCellTile(tileName, _enhancement, _cellToEnhance);
    var resolvedCells = ResolveCells(grid);
    _cellToEnhance.Tile = enhancedTile;
    return resolvedCells;
  }
}
