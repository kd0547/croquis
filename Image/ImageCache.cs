using System;

public static class RotationAngle
{
    public const double Angle0 = 0.0;
    public const double Angle90 = 90.0;
    public const double Angle180 = 180.0;
    public const double Angle270 = 270.0;
}


public class ImageCache
{
    public string ImageName { get; set; }
    public string ImagePath { get; set; }
    public bool IsFlip { get; set; } = false; // 좌우반전, 기본값 : false
    public bool IsFlipXY { get; set; } = false; // 상하반전, 기본값 : false

    public bool IsGrayscale { get; set; } = false; //흑백전환, 기본값 : false


    public double Angle { get; set; } = RotationAngle.Angle0; // 회전 각도, 기본값 : 0



    public ImageCache()
	{
	}

    /// <summary>
    /// 시계방향으로 회전 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void RotateClockwise()
    {
        if (Angle == RotationAngle.Angle270)
            Angle = RotationAngle.Angle0;
        else
            Angle += 90;
    }

    /// <summary>
    /// 시계 반대 방향으로 회전 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void RotateCounterClockwise()
    {
        if (Angle == RotationAngle.Angle0)
            Angle = RotationAngle.Angle270;
        else
            Angle -= 90;
    }
}
