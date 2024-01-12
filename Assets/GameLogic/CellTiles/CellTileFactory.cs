using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// A factory for creating cell tiles.
/// </summary>
public class CellTileFactory {
  private readonly Dictionary<string, CellTileData> _database;
  private readonly List<string> _keys;

  public CellTileFactory(CellTileDataDatabase database) {
    _database = new Dictionary<string, CellTileData>();
    _keys = new List<string>();
    foreach (CellTileData data in database.Entries) {
      _database.Add(data.Name, data);
      _keys.Add(data.Name);
    }
  }

  public CellTile CreateCellTile(string name, CellTile.EnhancementType enhancement, GridCell cell) {
    CellTileData data = _database[name];
    CellTile tile = new CellTile(data, enhancement, cell);
    return tile;
  }

  public CellTile CreateRandomCellTile(GridCell cell) {
    int index = Random.Range(0, _database.Count);
    string name = _keys[index];
    return CreateCellTile(name, CellTile.EnhancementType.Normal, cell);
  }
}

