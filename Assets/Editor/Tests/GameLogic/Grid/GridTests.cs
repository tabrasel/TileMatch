using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unit tests for <see cref="Grid"/>.
/// </summary>
public class GridTests {
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
  /// Makes sure that the grid can fill its empty cells with tiles either with or without matches
  /// afterwards.
  /// </summary>
  [Test]
  [TestCase(true)]
  [TestCase(false)]
  public void TestFillEmptyCells(bool allowMatches) {
    for (int i = 0; i < 50; i++) {
      Random.InitState(i);

      Grid grid = CreateGrid(new int[,] {
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0 }
      });
      var cellsToFill = grid.GetEmptyCells();

      grid.FillEmptyCells(false);

      if (!allowMatches) {
        var matches = grid.GetMatches(cellsToFill);
        Assert.AreEqual(0, matches.Count);
      }

      for (int row = 0; row < grid.RowCount; row++) {
        for (int col = 0; col < grid.ColumnCount; col++) {
          Assert.IsFalse(grid.Cells[row, col].IsEmpty);
        }
      }
    }
  }

  /// <summary>
  /// Makes sure that the correct matches are found when the grid has some.
  /// </summary>
  [Test]
  public void TestGetMatchesMultipleMatches() {
    Grid grid = CreateGrid(new int[,] {
      { 1, 1, 1, 0 },
      { 3, 0, 0, 2 },
      { 3, 2, 2, 2 },
      { 3, 3, 3, 2 }
    });

    var matches = grid.GetMatches();

    Assert.AreEqual(3, matches.Count);
  }

  /// <summary>
  /// Makes sure that no matches are found when the grid has none.
  /// </summary>
  [Test]
  public void TestGetMatchesNoMatches() {
    Grid grid = CreateGrid(new int[,] {
      { 1, 0, 1, 0 },
      { 3, 0, 0, 2 },
      { 0, 0, 2, 1 },
      { 3, 3, 0, 2 }
    });

    var matches = grid.GetMatches();

    Assert.AreEqual(0, matches.Count);
  }

  /// <summary>
  /// Makes sure that swapping two tiles in a grid causes them to be moved into the correct cells.
  /// </summary>
  [Test]
  public void TestSwapTiles() {
    Grid grid = CreateGrid(new int[,] {
      { 1, 0 },
      { 0, 2 }
    });

    grid.SwapTiles(grid.Cells[0, 0], grid.Cells[1, 1]);

    Grid expectedGrid = CreateGrid(new int[,] {
      { 2, 0 },
      { 0, 1 }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
    Assert.IsTrue(GridTestUtils.DoTilesPointToCorrectCells(grid));
  }

  /// <summary>
  /// Makes sure that collapsing the grid causes tiles to fall into the proper cells and marks the
  /// correct cells as "changed".
  /// </summary>
  [Test]
  public void TestCollapse() {
    Grid grid = CreateGrid(new int[,] {
      { 1, 0, 1, 1, 1, 0 },
      { 2, 1, 2, 0, 0, 0 },
      { 3, 2, 0, 2, 0, 0 }
    });
    GridCell[,] cells = grid.Cells;

    List<GridCell> changedCells = new List<GridCell>();
    grid.Collapse(changedCells);

    Grid expectedGrid = CreateGrid(new int[,] {
      { 1, 0, 0, 0, 0, 0 },
      { 2, 1, 1, 1, 0, 0 },
      { 3, 2, 2, 2, 1, 0 }
    });
    Assert.IsTrue(GridTestUtils.AreGridsEqual(expectedGrid, grid));
    Assert.IsTrue(GridTestUtils.DoTilesPointToCorrectCells(grid));

    List<GridCell> expectedChangedCells = new List<GridCell> {
      cells[2, 2], cells[1, 2], cells[0, 2],
      cells[1, 3], cells[0, 3],
      cells[2, 4], cells[0, 4]
    };
    Assert.AreEqual(expectedChangedCells, changedCells);
  }

  /// <summary>
  /// Makes sure that requesting an empty grid's empty cells returns all cells.
  /// </summary>
  [Test]
  public void TestGetEmptyCellsEmptyGrid() {
    Grid grid = CreateGrid(new int[,] {
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 },
      { 0, 0, 0, 0 }
    });

    var emptyCells = grid.GetEmptyCells();

    // Are the returned cells actually empty?
    foreach (GridCell cell in emptyCells) {
      Assert.IsTrue(cell.IsEmpty);
    }

    // Were all the grid's cells returned?
    Assert.AreEqual(grid.RowCount * grid.ColumnCount, emptyCells.Count);
  }

  /// <summary>
  /// Makes sure that requesting an empty grid's empty cells returns all cells.
  /// </summary>
  [Test]
  public void TestGetEmptyCellsFullGrid() {
    Grid grid = CreateGrid(new int[,] {
      { 1, 1, 1, 1 },
      { 1, 1, 1, 1 },
      { 1, 1, 1, 1 },
      { 1, 1, 1, 1 }
    });

    var emptyCells = grid.GetEmptyCells();

    // Are the returned cells actually empty?
    foreach (GridCell cell in emptyCells) {
      Assert.IsTrue(cell.IsEmpty);
    }

    // Were no cells returned?
    Assert.AreEqual(0, emptyCells.Count);
  }

  /// <summary>
  /// Makes sure that requesting a partially full grid's empty cells returns the correct cells.
  /// </summary>
  [Test]
  public void TestGetEmptyCellsPartiallyFullGrid() {
    Grid grid = CreateGrid(new int[,] {
      { 0, 1, 0, 0 },
      { 1, 0, 0, 1 },
      { 0, 3, 2, 0 },
      { 1, 3, 1, 2 }
    });
    GridCell[,] cells = grid.Cells;

    var emptyCells = grid.GetEmptyCells();

    // Are the returned cells actually empty?
    foreach (GridCell cell in emptyCells) {
      Assert.IsTrue(cell.IsEmpty);
    }

    // Do the returned cells match those expected?
    List<GridCell> expectedEmptyCells = new List<GridCell> {
      cells[0, 0], cells[0, 2], cells[0, 3],
      cells[1, 1], cells[1, 2],
      cells[2, 0], cells[2, 3]
    };
    Assert.AreEqual(expectedEmptyCells, emptyCells);
  }

  /// <summary>
  /// Makes sure that requesting a partially full grid's empty cells by column returns the correct
  /// cells.
  /// </summary>
  [Test]
  public void TestGetEmptyCellsByColumnPartiallyFullGrid() {
    Grid grid = CreateGrid(new int[,] {
      { 0, 0, 0, 1, 0 },
      { 1, 0, 0, 2, 0 },
      { 1, 0, 2, 1, 0 },
      { 1, 0, 1, 2, 2 }
    });
    GridCell[,] cells = grid.Cells;

    var emptyCells = grid.GetEmptyCellsByColumn();

    // Are the returned cells actually empty?
    foreach (int col in emptyCells.Keys) {
      foreach (GridCell cell in emptyCells[col]) {
        Assert.IsTrue(cell.IsEmpty);
      }
    }

    // Do the returned cells match those expected?
    var expectedEmptyCells = new Dictionary<int, List<GridCell>> {
      { 0, new List<GridCell> { cells[0, 0] } },
      { 1, new List<GridCell> { cells[0, 1], cells[1, 1], cells[2, 1], cells[3, 1] } },
      { 2, new List<GridCell> { cells[0, 2], cells[1, 2] } },
      { 4, new List<GridCell> { cells[0, 4], cells[1, 4], cells[2, 4] } }
    };
    Assert.AreEqual(expectedEmptyCells, emptyCells);
  }

  #region Utility methods

  private Grid CreateGrid(int[,] tiles) {
    return GridTestUtils.CreateGrid(tiles, _matchFinderManager, _cellTileFactory, _cellTileDb);
  }

  #endregion
}
