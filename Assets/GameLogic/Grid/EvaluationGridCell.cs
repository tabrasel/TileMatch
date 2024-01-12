/// <summary>
/// A GridCell used exclusively for finding matches. Holds information about whether it was
/// previously moved or has been included in a match so far this search session.
/// </summary>
public class EvaluationGridCell {
  public bool IsInMatch;
  public bool WasChanged;
  public readonly GridCell Cell;

  public int Row { get { return Cell.Row; } }
  public int Column { get { return Cell.Column; } }
  public string TileName { get { return Cell.Tile?.Name; } }
  public bool IsEmpty { get { return Cell.IsEmpty; } }

  public EvaluationGridCell(GridCell cell) {
    Cell = cell;
    IsInMatch = false;
    WasChanged = false;
  }

  public override string ToString() {
    return $"([{Cell.Column},{Cell.Row}] " + (Cell.IsEmpty ? "Empty" : Cell.Tile.Name) + ")";
  }
}
