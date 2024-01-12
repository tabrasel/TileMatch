/// <summary>
/// A rectangular 2D array of integers.
/// </summary>
public class Matrix {
  public int[,] Elements;
  public int ColumnCount, RowCount;

  public Matrix(int[,] elements) {
    Elements = elements;
    ColumnCount = elements.GetLength(1);
    RowCount = elements.GetLength(0);
  }

  public Matrix(int columnCount, int rowCount) {
    Elements = new int[rowCount, columnCount];
    ColumnCount = columnCount;
    RowCount = rowCount;
  }

  public override string ToString() {
    string str = "[";
    for (int i = 0; i < RowCount; i++) {
      str += "[";
      for (int j = 0; j < ColumnCount; j++) {
        str += Elements[i, j];
      }
      str += "]";
    }
    str += "]";
    return str;
  }
}
