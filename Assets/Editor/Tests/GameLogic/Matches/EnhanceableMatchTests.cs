using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unit tests for <see cref="Grid"/>.
/// </summary>
[TestFixture]
public class EnhanceableMatchTests {
  CellTileDataDatabase _cellTileDb;
  CellTileFactory _cellTileFactory;
  MatchFinderManager _matchFinderManager;

  [OneTimeSetUp]
  public void OneTimeSetUp() {
    _cellTileDb = ScriptableObject.CreateInstance<CellTileDataDatabase>();
    _cellTileDb.Entries = new List<CellTileData>() {
      GridTestUtils.CreateTileData("1"),
      GridTestUtils.CreateTileData("2"),
      GridTestUtils.CreateTileData("3")
    }; 
    _cellTileFactory = new CellTileFactory(_cellTileDb);

    _matchFinderManager = new MatchFinderManager(_cellTileFactory);
  }

  /// <summary>
  /// Makes sure that when a match includes a tile to be enhanced, that tile remains and gets enhanced while the others 
  /// are resolved normally.
  /// </summary>
  [Test]
  public void TestResolve() {
    Grid grid = CreateGrid(new string[,] {
      { "1n", "1n"},
      { "1n", "1n"},
    });
    var cells = grid.Cells;
    Match match = new EnhanceableMatch(
      new List<GridCell> { cells[0, 0], cells[0, 1], cells[1, 0], cells[1, 1] }, _cellTileFactory, cells[0, 0], 
      CellTile.EnhancementType.Super);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[0, 0], cells[0, 1], cells[1, 0], cells[1, 1] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "1s", "__" },
      { "__", "__" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  /// <summary>
  /// Makes sure that when a match should enhance a tile that's already enhanced, that the tile gets resolved but is
  /// replaced by another enhanced tile.
  /// </summary>
  [Test]
  public void TestResolveEnhancedTileGetsReplaced() {
    Grid grid = CreateGrid(new string[,] {
      { "1e", "1n", "2n"},
      { "1n", "1n", "2n"},
    });
    var cells = grid.Cells;
    Match match = new EnhanceableMatch(
      new List<GridCell> { cells[0, 0], cells[0, 1], cells[1, 0], cells[1, 1] }, _cellTileFactory, cells[0, 0], 
      CellTile.EnhancementType.Super);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[0, 0], cells[0, 1], cells[0, 2], cells[1, 0], cells[1, 1] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "1s", "__", "__" },
      { "__", "__", "2n" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  #region Utility methods

  private Grid CreateGrid(string[,] cells) {
    return GridTestUtils.CreateGrid(cells, _matchFinderManager, _cellTileFactory, _cellTileDb);
  }

  #endregion
}
