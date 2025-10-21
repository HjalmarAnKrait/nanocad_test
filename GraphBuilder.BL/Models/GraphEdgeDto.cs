namespace GraphBuilder.BL.Dto
{
    using System.Collections.Generic;

    using GraphBuilder.BL.Models;

    /// <summary>
    /// DTO для ребра графа
    /// </summary>
    public class GraphEdgeDto
    {
        public GraphEdgeDto(long id, long startVertexId, long endVertexId, double length)
        {
            Id = id;
            StartVertexId = startVertexId;
            EndVertexId = endVertexId;
            Length = length;
        }

        public long EndVertexId { get; set; }
        public long Id { get; set; }

        public List<Point2d> IntermediatePoints { get; set; } = new();
        public double Length { get; set; }
        public long StartVertexId { get; set; }
    }
}