using System;

public class PictureBoxRowCols
{
    public PictureBoxRowCols() { }
    public PictureBoxRowCols(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
    }

    public int Rows { get; set; }
    public int Cols { get; set; }
}
