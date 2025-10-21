namespace GraphBuilder.Ncad.Commands;

using System.Linq;

using GraphBuilder.BL;
using GraphBuilder.BL.Dto;
using GraphBuilder.BL.Models;
using GraphBuilder.Ncad.CadObjects;
using GraphBuilder.Ncad.Utils;

using HostMgd.ApplicationServices;

using Multicad.DatabaseServices;
using Multicad.Runtime;

/// <summary>
/// Команда для поиска ближайшего пути по графу..
/// </summary>
public class FindShortestWayCommand
{
    [CommandMethod("GB_FIND_SHORTEST_WAY", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
    public static void FindShortestWayCmd()
    {
        var verticies = GetVerticies();
        if(!verticies.success)
            return;
        
        var document = Application.DocumentManager.MdiActiveDocument;
        var transaction = document.Database.TransactionManager.StartTransaction();
        
        var allObjectsIds = CadUtils.GetAllDocumentObjectIds(document, transaction);
        var allObjects = allObjectsIds.Select(McObjectManager.GetObject).ToList();
        var graphVertexes = allObjects.OfType<GraphVertex>().ToList();
        var graphEdges = allObjects.OfType<GraphEdge>().ToList();

        var vertexDtoList =
            graphVertexes.Select(x => new GraphVertexDto(x.ID.Handle, x.CenterPoint.X, x.CenterPoint.Y)).ToList();
        var edgeDtoList =
            graphEdges.Select(x => new GraphEdgeDto(x.ID.Handle, x.StartVertex.ID.Handle, x.EndVertex.ID.Handle, x.Length)).ToList();
        
        var pathFinder = new SimpleGraphPathFinder();
        pathFinder.Initialize(vertexDtoList, edgeDtoList);
        
        var routeVertices = pathFinder.FindShortestPath(verticies.starVertexId, verticies.endVertexId);
        if (routeVertices.Count <= 0)
            return;
        
        var routeEdgeIds = pathFinder.GetRouteEdgeIds(routeVertices);
        foreach (var vertexId in routeVertices)
        {
            var vertex = graphVertexes.FirstOrDefault(v => v.ID.Handle == vertexId);
            vertex?.DbEntity.Highlight(true);
        }
        
        foreach (var edgeId in routeEdgeIds)
        {
            var edge = graphEdges.FirstOrDefault(e => e.ID.Handle == edgeId);
            edge?.DbEntity.Highlight(true);
        }
    }
    
    private static (long starVertexId, long endVertexId, bool success) GetVerticies()
    {
        var jig = new InputJig();
        var inputResultStart = jig.SelectObject("Выберите вершину старта.");
        if(inputResultStart.Result != InputResult.ResultCode.Normal || inputResultStart.ObjectId.GetObject() is not GraphVertex)
            return (-1, -1, false);
        
        var inputResultEnd = jig.SelectObject("Выберите вершину финиша.");
        if(inputResultEnd.Result != InputResult.ResultCode.Normal || inputResultEnd.ObjectId.GetObject() is not GraphVertex)
            return (-1, -1, false);

        return (inputResultStart.ObjectId.Handle, inputResultStart.ObjectId.Handle, true);
    } 
}