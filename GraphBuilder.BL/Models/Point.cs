namespace GraphBuilder.BL.Models;

/// <summary>
/// DTO для точки.
/// </summary>
public class Point2d
{
    public Point2d(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }
}