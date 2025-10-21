namespace GraphBuilder.Ncad.Commands;

using GraphBuilder.Ncad.CadObjects;

using Multicad;
using Multicad.DatabaseServices;
using Multicad.Runtime;

/// <summary>
/// Команда для построения графа.
/// </summary>
public class CreateGraphVertexCommand
{
    [CommandMethod("GB_SIMPLE_GRAPH", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
    public static void BuildSimpleGraphCmd()
    {
        GraphVertex lastVertex = null;
        while (true)
        {
            var vertex = new GraphVertex();
            var result = vertex.PlaceObject();

            if (result != hresult.s_Ok)
                break;

            if (lastVertex != null)
            {
                var edge = new GraphEdge(lastVertex.ID, vertex.ID);
                edge.DbEntity.AddToCurrentDocument();
            }

            lastVertex = vertex;
            McObjectManager.UpdateAll();
        }
    }
}