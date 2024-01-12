using System;
using UnityEngine;

/// <summary>
/// <see cref="Grid"/>-related utility methods for unit tests.
/// </summary>
public class GridTestUtils {
  public static CellTileData CreateTileData(string name) {
    CellTileData tileData = ScriptableObject.CreateInstance<CellTileData>();
    tileData.Name = name;
    tileData.Sprite = null;

    return tileData;
  }

  public static Grid CreateGrid(int[,] tiles, MatchFinderManager matchFinderManager,
    CellTileFactory cellTileFactory, CellTileDataDatabase cellTileDb) {
    int rowCount = tiles.GetLength(0);
    int colCount = tiles.GetLength(1);
    Grid grid = new Grid(colCount, rowCount, matchFinderManager, cellTileFactory);

    for (int row = 0; row < rowCount; row++) {
      for (int col = 0; col < colCount; col++) {
        GridCell cell = grid.Cells[row, col];
        if (tiles[row, col] == 0) {
          cell.Tile = null;
        } else {
          int index = tiles[row, col] - 1;
          CellTileData tileData = cellTileDb.Entries[index];
          cell.Tile = new CellTile(tileData, CellTile.EnhancementType.Normal, cell);
        }
      }
    }

    return grid;
  }

  /// <summary>
  /// Creates a grid from a 2D array of cell codes in the format "<tile><enhancement>".
  /// For example:
  ///  - "__": empty
  ///  - "n1": normal tile1
  ///  - "s2": super tile2
  ///  - "e3": epic tile3
  ///  - "l4": legendary tile4
  /// </summary>
  /// <param name="cells"></param>
  /// <param name="matchFinderManager"></param>
  /// <param name="cellTileFactory"></param>
  /// <param name="cellTileDb"></param>
  public static Grid CreateGrid(string[,] cells, MatchFinderManager matchFinderManager,
    CellTileFactory cellTileFactory, CellTileDataDatabase cellTileDb) {
    int rowCount = cells.GetLength(0);
    int colCount = cells.GetLength(1);
    Grid grid = new Grid(colCount, rowCount, matchFinderManager, cellTileFactory);

    for (int row = 0; row < rowCount; row++) {
      for (int col = 0; col < colCount; col++) {
        GridCell cell = grid.Cells[row, col];
        string cellCode = cells[row, col];
        if (cellCode.StartsWith("_")) {
          cell.Tile = null;
        } else {
          int index = int.Parse(cellCode.Substring(0, 1)) - 1;
          CellTile.EnhancementType enhancement = cellCode[1] switch {
            'l' => CellTile.EnhancementType.Legendary,
            'e' => CellTile.EnhancementType.Epic,
            's' => CellTile.EnhancementType.Super,
            _ => CellTile.EnhancementType.Normal
          };
          
          CellTileData tileData = cellTileDb.Entries[index];
          cell.Tile = new CellTile(tileData, enhancement, cell);
        }
      }
    }

    return grid;
  }

  public static bool AreGridsEqual(Grid grid1, Grid grid2) {
    // Check null
    if (grid1 == null || grid2 == null) return grid1 == grid2;

    // Check dimensions
    if (grid1.RowCount != grid2.RowCount || grid1.ColumnCount != grid2.ColumnCount) return false;

    // Check contents
    for (int row = 0; row < grid1.RowCount; row++) {
      for (int col = 0; col < grid1.ColumnCount; col++) {
        if (grid1.Cells[row, col].IsEmpty != grid1.Cells[row, col].IsEmpty) return false;
        if (grid1.Cells[row, col].IsEmpty) continue;
        if (grid1.Cells[row, col].Tile.Name != grid2.Cells[row, col].Tile.Name) return false;
      }
    }

    return true;
  }

  public static bool DoTilesPointToCorrectCells(Grid grid) {
    for (int row = 0; row < grid.RowCount; row++) {
      for (int col = 0; col < grid.ColumnCount; col++) {
        GridCell cell = grid.Cells[row, col];
        if (cell.IsEmpty) continue;
        if (cell.Tile.Cell != cell) return false;
      }
    }
    return true;
  }
}
