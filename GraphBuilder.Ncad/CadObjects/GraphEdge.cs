namespace GraphBuilder.Ncad.CadObjects
{
    using System.Collections.Generic;

    using GraphBuilder.Ncad.Interface;

    using Multicad;
    using Multicad.Constants;
    using Multicad.CustomObjectBase;
    using Multicad.DatabaseServices;
    using Multicad.Geometry;
    using Multicad.Runtime;

    /// <summary>
    /// Ребро графа.
    /// </summary>
    [CustomEntity("2F76680E-5FEA-4DC2-B250-39044FB21E58", "GB_GraphEdge", "Graph edge")]
    public class GraphEdge : McCustomBase, IVertexObserver
    {
        private double _cachedLength = -1;
        private McObjectId _endVertexId;
        private List<Point3d> _intermediatePoints = new();
        private McObjectId _startVertexId;
        
        public GraphEdge()
        {
        }

        public GraphEdge(McObjectId startVertexId, McObjectId endVertexId)
        {
            _startVertexId = startVertexId;
            _endVertexId = endVertexId;
            RegisterWithVertices();
        }

        /// <summary>
        /// Первая вершина :)
        /// </summary>
        public GraphVertex EndVertex => GetEndVertex();
        
        /// <summary>
        /// Вторая вершина :)
        /// </summary>
        public GraphVertex StartVertex => GetStartVertex();
        
        /// <summary>
        /// Точки перелома.
        /// </summary>
        public IReadOnlyList<Point3d> IntermediatePoints => _intermediatePoints.AsReadOnly();
        
        /// <summary>
        /// Длинна ребра.
        /// </summary>
        public double Length
        {
            get
            {
                if (_cachedLength < 0)
                    _cachedLength = CalculateLength();
                return _cachedLength;
            }
        }
        
        /// <inheritdoc/>
        public void OnVertexMoved(GraphVertex vertex)
        {
            UpdateGeometry();
        }

        /// <inheritdoc/>
        public void OnVertexErased(GraphVertex vertex)
        {
            if (StartVertex?.ID == vertex.ID)
                _startVertexId = McObjectId.Null;
            
            if (EndVertex?.ID == vertex.ID)
                _endVertexId = McObjectId.Null;
            
            if (IsOrphanedEdge())
            {
                UnregisterFromVertices();
                DbEntity?.Erase();
            }
        }

        /// <inheritdoc/>
        public override bool GetGripPoints(GripPointsInfo info)
        {
            for (var i = 0; i < _intermediatePoints.Count; i++)
            {
                var index = i;
                info.AppendGrip(new McSmartGrip<GraphEdge>(_intermediatePoints[index],
                    (obj, grip, offset) =>
                    {
                        obj.TryModify();
                        obj._intermediatePoints[index] += offset;
                        obj.InvalidateLength();
                    }));
            }

            return true;
        }
        
        /// <summary>
        /// Сброс кэша.
        /// </summary>
        public void InvalidateLength()
        {
            _cachedLength = -1;
        }

        /// <inheritdoc/>
        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = Colors.ByObject;
            dc.LineType = LineTypes.ByObject;
            dc.LineWidth = LineWeights.ByObject;

            var points = GetEdgePoints();
            if (points.Count > 1)
                dc.DrawPolyline(points.ToArray());
        }

        /// <inheritdoc/>
        public override void OnErase()
        {
            UnregisterFromVertices();
            base.OnErase();
        }

        /// <inheritdoc/>
        public override hresult OnMcDeserialization(McSerializationInfo info)
        {
            if (!info.GetValue(nameof(_startVertexId), out _startVertexId))
                return hresult.e_Fail;
            if (!info.GetValue(nameof(_endVertexId), out _endVertexId))
                return hresult.e_Fail;
            if (!info.GetValue(nameof(_intermediatePoints), out Point3d[] points))
                return hresult.e_Fail;

            _intermediatePoints = new List<Point3d>(points);
            InvalidateLength();
            
            return hresult.s_Ok;
        }

        /// <inheritdoc/>
        public override hresult OnMcSerialization(McSerializationInfo info)
        {
            info.Add(nameof(_startVertexId), _startVertexId);
            info.Add(nameof(_endVertexId), _endVertexId);
            info.Add(nameof(_intermediatePoints), _intermediatePoints.ToArray());

            return hresult.s_Ok;
        }

        /// <inheritdoc/>
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify())
                return;

            for (var i = 0; i < _intermediatePoints.Count; i++)
                _intermediatePoints[i] = _intermediatePoints[i].TransformBy(tfm);

            InvalidateLength();
        }

        /// <inheritdoc/>
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            var firstVertex = McObjectManager.SelectObject("Выберите первую вершину").GetObject() as GraphVertex;
            var secondVertex = McObjectManager.SelectObject("Выберите вторую вершину").GetObject() as GraphVertex;

            if (firstVertex == null || secondVertex == null)
                return hresult.e_Fail;

            _startVertexId = firstVertex.ID;
            _endVertexId = secondVertex.ID;

            DbEntity.AddToCurrentDocument();
            RegisterWithVertices();

            return hresult.s_Ok;
        }
        
        /// <summary>
        /// Отвязывает ребро от вершин.
        /// </summary>
        public void UnregisterFromVertices()
        {
            var startVertex = GetStartVertex();
            var endVertex = GetEndVertex();
            
            startVertex?.RemoveObserver(this);
            endVertex?.RemoveObserver(this);
        }

        /// <summary>
        /// Обновляет геометрию вершины.
        /// </summary>
        public void UpdateGeometry()
        {
            if (!TryModify())
                return;
            
            if (IsOrphanedEdge())
            {
                UnregisterFromVertices();
                DbEntity?.Erase();
                return;
            }

            InvalidateLength();
            DbEntity?.Update();
        }
        
        /// <summary>
        /// Возвращает длинну ребра.
        /// </summary>
        /// <returns> Длинна ребра. </returns>
        private double CalculateLength()
        {
            var points = GetEdgePoints();
            if (points.Count < 2)
                return 0;

            double totalLength = 0;
            for (var i = 0; i < points.Count - 1; i++)
                totalLength += (points[i + 1] - points[i]).Length;
            return totalLength;
        }

        /// <summary>
        /// Возвращает список точек.
        /// </summary>
        /// <returns> Список всех точек ребра, включая вершины. </returns>
        private List<Point3d> GetEdgePoints()
        {
            var points = new List<Point3d>();
            var startVertex = GetStartVertex();
            var endVertex = GetEndVertex();

            if (startVertex != null && endVertex != null)
            {
                points.Add(startVertex.CenterPoint);
                points.AddRange(_intermediatePoints);
                points.Add(endVertex.CenterPoint);
            }

            return points;
        }

        /// <summary>
        /// Возвращает вершину конца.
        /// </summary>
        private GraphVertex GetEndVertex()
        {
            return McObjectManager.GetObject(_endVertexId) as GraphVertex;
        }

        /// <summary>
        /// Возвращает вершину начала.
        /// </summary>
        private GraphVertex GetStartVertex()
        {
            return McObjectManager.GetObject(_startVertexId) as GraphVertex;
        }
        
        /// <summary>
        /// Проверяет, является ли ребро "Висячим"
        /// </summary>
        private bool IsOrphanedEdge()
        {
            return GetStartVertex() == null || GetEndVertex() == null;
        }

        /// <summary>
        /// Регестрирует вершины.
        /// </summary>
        private void RegisterWithVertices()
        {
            var startVertex = GetStartVertex();
            var endVertex = GetEndVertex();
            
            startVertex?.AddObserver(this);
            endVertex?.AddObserver(this);
            
            UpdateGeometry();
        }
    }
}