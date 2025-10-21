namespace GraphBuilder.BL.Models;

/// <summary>
/// DTO для вершины графа.
/// </summary>
public class GraphVertexDto
{
    public GraphVertexDto(long id, double x, double y)
    {
        Id = id;
        X = x;
        Y = y;
    }

    public long Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public override bool Equals(object obj)
    {
        return obj is GraphVertexDto dto && Id == dto.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}