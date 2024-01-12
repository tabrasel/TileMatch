using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unit tests for <see cref="Grid"/>.
/// </summary>
[TestFixture]
public class MatchTests {
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
  /// Makes sure that resolving a match of normal tiles only clears those tiles and no others.
  /// </summary>
  [Test]
  public void TestResolveNormal() {
    Grid grid = CreateGrid(new string[,] {
      { "1n", "1n", "1n" },
      { "2n", "3n", "2n" },
      { "3n", "2n", "3n" }
    });
    var cells = grid.Cells;
    Match match = new Match(new List<GridCell> { cells[0, 0], cells[0, 1], cells[0, 2] }, _cellTileFactory);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[0, 0], cells[0, 1], cells[0, 2] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "__", "__", "__" },
      { "2n", "3n", "2n" },
      { "3n", "2n", "3n" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  /// <summary>
  /// Makes sure that resolving a match with a super tile clears out that cell as well as its neighbors.
  /// </summary>
  [Test]
  public void TestResolveSuper() {
    Grid grid = CreateGrid(new string[,] {
      { "3n", "1n", "3n" },
      { "1n", "1s", "1n" },
      { "3n", "2n", "3n" }
    });
    var cells = grid.Cells;
    Match match = new Match(new List<GridCell> { cells[1, 0], cells[1, 1], cells[1, 2] }, _cellTileFactory);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[1, 0], cells[1, 1], cells[1, 2], cells[0, 1], cells[2, 1] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "3n", "__", "3n" },
      { "__", "__", "__" },
      { "3n", "__", "3n" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  /// <summary>
  /// Makes sure that resolving a match with an epic tile clears out all the cells in the same row.
  /// </summary>
  [Test]
  public void TestResolveEpic() {
    Grid grid = CreateGrid(new string[,] {
      { "3n", "1n", "3n", "2n" },
      { "1n", "1e", "1n", "1n" },
      { "3n", "2n", "3n", "2n" }
    });
    var cells = grid.Cells;
    Match match = new Match(new List<GridCell> { cells[1, 0], cells[1, 1], cells[1, 2] }, _cellTileFactory);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[1, 0], cells[1, 1], cells[1, 2], cells[1, 3] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "3n", "1n", "3n", "2n" },
      { "__", "__", "__", "__" },
      { "3n", "2n", "3n", "2n" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  /// <summary>
  /// Makes sure that resolving a match with a legendary tile clears out all the cells of the same type.
  /// </summary>
  [Test]
  public void TestResolveLegendary() {
    Grid grid = CreateGrid(new string[,] {
      { "3n", "1n", "2n" },
      { "1n", "1l", "1n" },
      { "1n", "2n", "1n" }
    });
    var cells = grid.Cells;
    Match match = new Match(new List<GridCell> { cells[1, 0], cells[1, 1], cells[1, 2] }, _cellTileFactory);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[1, 0], cells[1, 1], cells[0, 1], cells[1, 2], cells[2, 0], cells[2, 2] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "3n", "__", "2n" },
      { "__", "__", "__" },
      { "__", "2n", "__" }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  /// <summary>
  /// Makes sure that resolving a match can cause other cells outside it to be resolved as well (by enhanced tiles).
  /// </summary>
  [Test]
  public void TestResolveCanChain() {
    Grid grid = CreateGrid(new string[,] {
      { "1n", "1n", "1s"},
      { "3n", "3n", "2e"}
    });
    var cells = grid.Cells;
    Match match = new Match(new List<GridCell> { cells[0, 0], cells[0, 1], cells[0, 2] }, _cellTileFactory);

    var resolvedCells = match.Resolve(grid);

    var expectedResolvedCells = new List<GridCell> { cells[0, 0], cells[0, 1], cells[0, 2], cells[1, 2], cells[1, 0], cells[1, 1] };
    Assert.AreEqual(expectedResolvedCells, resolvedCells);

    Grid expectedGrid = CreateGrid(new string[,] {
      { "__", "__", "__" },
      { "__", "__", "__" },
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
  }

  #region Utility methods

  private Grid CreateGrid(string[,] cells) {
    return GridTestUtils.CreateGrid(cells, _matchFinderManager, _cellTileFactory, _cellTileDb);
  }

  #endregion
}
