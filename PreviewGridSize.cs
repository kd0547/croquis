using System;

public class PreviewGridSize
{
    public PreviewGridSize() { }
    public PreviewGridSize(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
    }

    public int Rows { get; set; }
    public int Cols { get; set; }
}
