using UnityEngine;

/// <summary>
/// Stores general information about a kind of tile.
/// </summary>
[CreateAssetMenu(fileName = "CellTileData", menuName = "Config/CellTileData")]
public class CellTileData : ScriptableObject {
  public string Name;
  public Sprite Sprite;
  public Color Color;

  public CellTileData(string name, Sprite sprite, Color color) {
    Name = name;
    Sprite = sprite;
    Color = color;
  }
}
