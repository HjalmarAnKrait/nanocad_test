namespace GraphBuilder.Bl.Tests;

using System;
using System.Collections.Generic;

using GraphBuilder.BL;
using GraphBuilder.BL.Dto;
using GraphBuilder.BL.Models;

using NUnit.Framework;

[TestFixture]
public class SimpleGraphPathFinderTests
{
    private SimpleGraphPathFinder _pathFinder;
    private List<GraphVertexDto> _vertices;
    private List<GraphEdgeDto> _edges;

    [SetUp]
    public void Setup()
    {
        _pathFinder = new SimpleGraphPathFinder();
        
        // Базовый тестовый граф
        _vertices = new List<GraphVertexDto>
        {
            new(1, 0, 0),
            new(2, 10, 0),
            new(3, 20, 0),
            new(4, 10, 10),
            new(5, 30, 0)  // Изолированная вершина
        };

        _edges = new List<GraphEdgeDto>
        {
            new(1, 1, 2, 10),  // 1-2
            new(2, 2, 3, 10),  // 2-3  
            new(3, 1, 4, 14),  // 1-4 (более длинное прямое соединение)
            new(4, 4, 3, 10)   // 4-3
        };

        _pathFinder.Initialize(_vertices, _edges);
    }

    [Test]
    public void Initialize_WithValidData_BuildsCorrectConnections()
    {
        // Assert
        Assert.That(_pathFinder.GetInfo(), Contains.Substring("Вершин: 5"));
        Assert.That(_pathFinder.VertexExists(1), Is.True);
        Assert.That(_pathFinder.VertexExists(2), Is.True);
        Assert.That(_pathFinder.VertexExists(5), Is.True); // Изолированная вершина
    }

    [Test]
    public void Initialize_WithNullVertices_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _pathFinder.Initialize(null, _edges));
    }

    [Test]
    public void Initialize_WithNullEdges_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _pathFinder.Initialize(_vertices, null));
    }

    [Test]
    public void FindShortestPath_ValidPath_ReturnsCorrectPath()
    {
        // Act
        var path = _pathFinder.FindShortestPath(1, 3);

        // Assert - кратчайший путь 1-4-3 (длина 24) вместо 1-2-3 (длина 20)
        Assert.That(path, Is.EqualTo(new List<long> { 1, 2, 3 }));
    }

    [Test]
    public void FindShortestPath_SameStartAndEnd_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            _pathFinder.FindShortestPath(1, 1));
        Assert.That(ex.Message, Contains.Substring("совпадают"));
    }

    [Test]
    public void FindShortestPath_StartVertexNotFound_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            _pathFinder.FindShortestPath(999, 1));
        Assert.That(ex.Message, Contains.Substring("не найдена"));
    }

    [Test]
    public void FindShortestPath_EndVertexNotFound_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            _pathFinder.FindShortestPath(1, 999));
        Assert.That(ex.Message, Contains.Substring("не найдена"));
    }

    [Test]
    public void FindShortestPath_DirectConnection_ReturnsTwoVertices()
    {
        // Act
        var path = _pathFinder.FindShortestPath(1, 2);

        // Assert
        Assert.That(path, Is.EqualTo(new List<long> { 1, 2 }));
    }

    [Test]
    public void GetRouteEdges_ValidPath_ReturnsCorrectEdges()
    {
        // Arrange
        var path = new List<long> { 1, 4, 3 };

        // Act
        var edges = _pathFinder.GetRouteEdges(path);

        // Assert
        Assert.That(edges, Has.Count.EqualTo(2));
        Assert.That(edges[0].Id, Is.EqualTo(3)); // Ребро 1-4
        Assert.That(edges[1].Id, Is.EqualTo(4)); // Ребро 4-3
    }

    [Test]
    public void GetRouteEdges_EmptyPath_ReturnsEmptyList()
    {
        // Act
        var edges = _pathFinder.GetRouteEdges(new List<long>());

        // Assert
        Assert.That(edges, Is.Empty);
    }

    [Test]
    public void GetRouteEdges_SingleVertex_ReturnsEmptyList()
    {
        // Act
        var edges = _pathFinder.GetRouteEdges(new List<long> { 1 });

        // Assert
        Assert.That(edges, Is.Empty);
    }

    [Test]
    public void GetRouteEdges_MissingEdge_ThrowsInvalidOperationException()
    {
        // Arrange - путь с несуществующим ребром
        var invalidPath = new List<long> { 1, 5 }; // Нет ребра 1-5

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _pathFinder.GetRouteEdges(invalidPath));
    }

    [Test]
    public void GetRouteEdgeIds_ValidPath_ReturnsCorrectIds()
    {
        // Arrange
        var path = new List<long> { 1, 4, 3 };

        // Act
        var edgeIds = _pathFinder.GetRouteEdgeIds(path);

        // Assert
        Assert.That(edgeIds, Is.EqualTo(new List<long> { 3, 4 }));
    }

    [Test]
    public void CalculateRouteLength_ValidPath_ReturnsCorrectLength()
    {
        // Arrange
        var path = new List<long> { 1, 4, 3 };

        // Act
        var length = _pathFinder.CalculateRouteLength(path);

        // Assert - 14 (1-4) + 10 (4-3) = 24
        Assert.That(length, Is.EqualTo(24));
    }

    [Test]
    public void CalculateRouteLength_EmptyPath_ReturnsZero()
    {
        // Act
        var length = _pathFinder.CalculateRouteLength(new List<long>());

        // Assert
        Assert.That(length, Is.EqualTo(0));
    }

    [Test]
    public void VertexExists_ExistingVertex_ReturnsTrue()
    {
        // Act & Assert
        Assert.That(_pathFinder.VertexExists(1), Is.True);
        Assert.That(_pathFinder.VertexExists(3), Is.True);
    }

    [Test]
    public void VertexExists_NonExistingVertex_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(_pathFinder.VertexExists(999), Is.False);
    }

    [Test]
    public void GetInfo_AfterInitialization_ReturnsCorrectInfo()
    {
        // Act
        var info = _pathFinder.GetInfo();

        // Assert
        Assert.That(info, Contains.Substring("Вершин: 5"));
        Assert.That(info, Contains.Substring("Рёбер: 4"));
    }

    [Test]
    public void ComplexGraph_MultiplePaths_FindsShortest()
    {
        // Arrange - более сложный граф
        var complexVertices = new List<GraphVertexDto>
        {
            new(1, 0, 0),
            new(2, 10, 0),
            new(3, 20, 0),
            new(4, 10, 10),
            new(5, 0, 10)
        };

        var complexEdges = new List<GraphEdgeDto>
        {
            new(1, 1, 2, 10),  // 1-2
            new(2, 2, 3, 10),  // 2-3
            new(3, 1, 5, 5),   // 1-5 (короткое)
            new(4, 5, 4, 5),   // 5-4 (короткое)  
            new(5, 4, 3, 5)    // 4-3 (короткое)
        };

        var complexFinder = new SimpleGraphPathFinder();
        complexFinder.Initialize(complexVertices, complexEdges);

        // Act - кратчайший путь 1-5-4-3 (длина 15)
        var path = complexFinder.FindShortestPath(1, 3);

        // Assert
        Assert.That(path, Is.EqualTo(new List<long> { 1, 5, 4, 3 }));
        Assert.That(complexFinder.CalculateRouteLength(path), Is.EqualTo(15));
    }
    
    [Test] 
    public void EdgesWithMissingVertices_AreIgnored()
    {
        // Arrange - ребро ссылается на несуществующую вершину
        var vertices = new List<GraphVertexDto>
        {
            new(1, 0, 0)
        };

        var edges = new List<GraphEdgeDto>
        {
            new(1, 1, 999, 10) // Вершина 999 не существует
        };

        var finder = new SimpleGraphPathFinder();
        
        // Act
        Assert.DoesNotThrow(() => finder.Initialize(vertices, edges));
        
        // Assert - ребро должно быть проигнорировано
        Assert.That(finder.GetInfo(), Contains.Substring("Рёбер: 0"));
    }
}