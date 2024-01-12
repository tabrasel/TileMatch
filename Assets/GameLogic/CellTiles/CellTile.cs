using UnityEngine;

public class CellTile {
  public GridCell Cell;
  public EnhancementType Enhancement;

  public string Name { get; private set; }
  public Sprite Sprite { get; private set; }
  public Color Color { get; private set; }

  public CellTile(CellTileData data, EnhancementType enhancement, GridCell cell) {
    Name = data.Name;
    Sprite = data.Sprite;
    Color = data.Color;
    Cell = cell;
    Enhancement = enhancement;
  }

  public enum EnhancementType {
    Normal,
    Super,
    Epic,
    Legendary
  }
}
