namespace GraphBuilder.BL;

using System;
using System.Collections.Generic;
using System.Linq;

using GraphBuilder.BL.Dto;
using GraphBuilder.BL.Models;

/// <summary>
/// Простой менеджер для поиска кратчайшего пути в графе
/// </summary>
public class SimpleGraphPathFinder
{
    private Dictionary<long, List<(long neighborId, double weight)>> _connections;
    private Dictionary<(long, long), GraphEdgeDto> _edgesDictionary;
    private Dictionary<long, GraphVertexDto> _vertices;

    /// <summary>
    /// Поиск кратчайшего пути.
    /// </summary>
    /// <param name="startId">ID начальной вершины</param>
    /// <param name="endId">ID конечной вершины</param>
    /// <returns>Список ID вершин, через которые должен проходить маршрут</returns>
    public List<long> FindShortestPath(long startId, long endId)
    {
        ValidateVerticesExist(startId, endId);
        
        var distances = new Dictionary<long, double>();
        var previous = new Dictionary<long, long>();
        var unvisited = new HashSet<long>();
        foreach (var vertexId in _vertices.Keys)
        {
            distances[vertexId] = double.MaxValue;
            previous[vertexId] = -1;
            unvisited.Add(vertexId);
        }

        distances[startId] = 0;

        while (unvisited.Count > 0)
        {
            var currentId = GetClosestUnvisitedVertex(unvisited, distances);
            if (currentId == -1)
                break;

            unvisited.Remove(currentId);
            if (currentId == endId)
                break;
            
            UpdateNeighborDistances(currentId, distances, previous, unvisited);
        }

        return BuildPath(previous, endId);
    }

    /// <summary>
    /// Получает рёбра маршрута по списку вершин
    /// </summary>
    /// <param name="routeVertices">Список ID вершин маршрута</param>
    /// <returns>Список рёбер маршрута</returns>
    public List<GraphEdgeDto> GetRouteEdges(List<long> routeVertices)
    {
        if (routeVertices == null || routeVertices.Count < 2)
            return new List<GraphEdgeDto>();

        var routeEdges = new List<GraphEdgeDto>();

        for (int i = 0; i < routeVertices.Count - 1; i++)
        {
            var startVertexId = routeVertices[i];
            var endVertexId = routeVertices[i + 1];

            if (_edgesDictionary.TryGetValue((startVertexId, endVertexId), out var edge))
            {
                routeEdges.Add(edge);
            }
            else
            {
                if (_edgesDictionary.TryGetValue((endVertexId, startVertexId), out edge))
                {
                    routeEdges.Add(edge);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Ребро между вершинами {startVertexId} и {endVertexId} не найдено");
                }
            }
        }

        return routeEdges;
    }

    /// <summary>
    /// Получает ID рёбер маршрута по списку вершин
    /// </summary>
    /// <param name="routeVertices">Список ID вершин маршрута</param>
    /// <returns>Список ID рёбер маршрута</returns>
    public List<long> GetRouteEdgeIds(List<long> routeVertices)
    {
        return GetRouteEdges(routeVertices).Select(e => e.Id).ToList();
    }

    /// <summary>
    /// Вычисляет общую длину маршрута
    /// </summary>
    /// <param name="routeVertices">Список ID вершин маршрута</param>
    /// <returns>Общая длина маршрута</returns>
    public double CalculateRouteLength(List<long> routeVertices)
    {
        return GetRouteEdges(routeVertices).Sum(e => e.Length);
    }

    /// <summary>
    /// Получает информацию о графе
    /// </summary>
    public string GetInfo()
    {
        return $"Вершин: {_vertices.Count}, Рёбер: {_edgesDictionary.Count / 2}";
    }

    /// <summary>
    /// Инициализация графа данными
    /// </summary>
    public void Initialize(List<GraphVertexDto> vertices, List<GraphEdgeDto> edges)
    {
        _vertices = vertices?.ToDictionary(v => v.Id) ?? throw new ArgumentNullException(nameof(vertices));
        _connections = new Dictionary<long, List<(long, double)>>();
        _edgesDictionary = new Dictionary<(long, long), GraphEdgeDto>();
        foreach (var edge in edges ?? throw new ArgumentNullException(nameof(edges)))
        {
            if (!_vertices.ContainsKey(edge.StartVertexId) || !_vertices.ContainsKey(edge.EndVertexId))
                continue;
            
            AddConnection(edge.StartVertexId, edge.EndVertexId, edge.Length);
            AddConnection(edge.EndVertexId, edge.StartVertexId, edge.Length);
            _edgesDictionary[(edge.StartVertexId, edge.EndVertexId)] = edge;
            _edgesDictionary[(edge.EndVertexId, edge.StartVertexId)] = edge;
        }
    }

    /// <summary>
    /// Проверяет существование вершин в графе
    /// </summary>
    public bool VertexExists(long vertexId)
    {
        return _vertices.ContainsKey(vertexId);
    }

    private void AddConnection(long fromId, long toId, double weight)
    {
        if (!_connections.ContainsKey(fromId))
            _connections[fromId] = new List<(long, double)>();

        _connections[fromId].Add((toId, weight));
    }

    private void ValidateVerticesExist(long startId, long endId)
    {
        if (!VertexExists(startId))
            throw new ArgumentException($"Вершина {startId} не найдена");
        if (!VertexExists(endId))
            throw new ArgumentException($"Вершина {endId} не найдена");
        if (startId == endId)
            throw new ArgumentException("Начальная и конечная вершины совпадают");
    }

    private long GetClosestUnvisitedVertex(HashSet<long> unvisited, Dictionary<long, double> distances)
    {
        long closestId = -1;
        var minDistance = double.MaxValue;

        foreach (var vertexId in unvisited)
        {
            if (distances[vertexId] < minDistance)
            {
                minDistance = distances[vertexId];
                closestId = vertexId;
            }
        }

        return closestId;
    }

    private void UpdateNeighborDistances(long currentId, Dictionary<long, double> distances,
        Dictionary<long, long> previous, HashSet<long> unvisited)
    {
        if (!_connections.ContainsKey(currentId))
            return;

        foreach (var (neighborId, weight) in _connections[currentId])
        {
            if (!unvisited.Contains(neighborId))
                continue;

            var newDistance = distances[currentId] + weight;
            if (newDistance < distances[neighborId])
            {
                distances[neighborId] = newDistance;
                previous[neighborId] = currentId;
            }
        }
    }

    private List<long> BuildPath(Dictionary<long, long> previous, long endId)
    {
        var path = new List<long>();
        var current = endId;

        while (current != -1 && previous.ContainsKey(current))
        {
            path.Insert(0, current);
            current = previous[current];
        }
        
        return path.Count > 0 && path[0] != -1 ? path : new List<long>();
    }
}