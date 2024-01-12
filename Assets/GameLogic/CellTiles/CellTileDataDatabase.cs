using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A database for cell tile data.
/// </summary>
[CreateAssetMenu(fileName = "CellTileDataDatabase", menuName = "Config/CellTileDataDatabase")]
public class CellTileDataDatabase : ScriptableObject {
  public List<CellTileData> Entries;
}
