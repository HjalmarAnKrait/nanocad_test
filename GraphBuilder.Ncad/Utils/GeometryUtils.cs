namespace GraphBuilder.Ncad.Utils;

using System;

using Multicad.Geometry;

/// <summary>
/// Утилиты для работы с геометрией.
/// </summary>
public static class GeometryUtils
{
    /// <summary>
    /// Возвращает точки для построения треугольника, вписанного в окружность с заданным радиусом.
    /// </summary>
    /// <param name="center"> Координаты центра окружности. </param>
    /// <param name="radius"> Радиус окружности. </param>
    /// <returns> Кортеж из координат трёх точек для построения треугольника. </returns>
    public static (Point3d A, Point3d B, Point3d C) GetTrianglePointsByRadius(Point3d center, double radius)
    {
        const double angleA = Math.PI / 2;
        const double angleB = 7 * Math.PI / 6;
        const double angleC = 11 * Math.PI / 6;
        var a = new Point3d(
            center.X + radius * Math.Cos(angleA),
            center.Y + radius * Math.Sin(angleA),
            center.Z
        );

        var b = new Point3d(
            center.X + radius * Math.Cos(angleB),
            center.Y + radius * Math.Sin(angleB),
            center.Z
        );

        var c = new Point3d(
            center.X + radius * Math.Cos(angleC),
            center.Y + radius * Math.Sin(angleC),
            center.Z
        );

        return (a, b, c);
    }
}