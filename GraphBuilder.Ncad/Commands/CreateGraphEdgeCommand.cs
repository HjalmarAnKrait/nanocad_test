namespace GraphBuilder.Ncad.Commands;

using GraphBuilder.Ncad.CadObjects;

using Multicad.DatabaseServices;
using Multicad.Runtime;

/// <summary>
/// Команда создания дополнительных ребёр графа для соединения вершин.
/// </summary>
public class CreateGraphEdgeCommand
{
    [CommandMethod("GB_GRAPH_EDGE", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
    public static void GraphEdgeCmd()
    {
        var edge = new GraphEdge();
        edge.PlaceObject();
        McObjectManager.UpdateAll();
    }
}