namespace GraphBuilder.Ncad.CadObjects
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using GraphBuilder.Ncad.Interface;
    using GraphBuilder.Ncad.Utils;

    using Multicad;
    using Multicad.Constants;
    using Multicad.CustomObjectBase;
    using Multicad.DatabaseServices;
    using Multicad.Geometry;
    using Multicad.Runtime;

    /// <summary>
    /// Cad-объект вершины графа
    /// </summary>
    [CustomEntity("B31FD339-831A-4D4E-951F-A62EC5E23917", "GB_GraphVertex", "Graph vertex")]
    public class GraphVertex : McCustomBase, IVertexObservable
    {
        private const int RADIUS = 200;
        private GraphVertexForm _graphVertexForm = GraphVertexForm.Triangle;
        private readonly List<IVertexObserver> _observers = new();
        
        public Point3d CenterPoint = new(0, 0, 0);
        
        /// <inheritdoc/>
        public void AddObserver(IVertexObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        /// <inheritdoc/>
        public void RemoveObserver(IVertexObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <inheritdoc/>
        public void NotifyMoved()
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnVertexMoved(this);
            }
        }

        /// <inheritdoc/>
        public void NotifyErased()
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnVertexErased(this);
            }
        }

        /// <inheritdoc/>
        public override bool GetGripPoints(GripPointsInfo info)
        {
            info.AppendGrip(new McSmartGrip<GraphVertex>(CenterPoint, (obj, _, offset) =>
            {
                obj.TryModify();
                obj.CenterPoint += offset;
                obj.NotifyMoved(); 
            }));

            return true;
        }

        /// <inheritdoc/>
        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.LineType = LineTypes.ByObject;
            dc.LineWidth = LineWeights.ByObject;

            if (_graphVertexForm == GraphVertexForm.Circle)
            {
                dc.Color = Color.Blue;
                dc.DrawCircle(CenterPoint, RADIUS);
                return;
            }

            dc.Color = Color.Red;
            var trianglePoints = GeometryUtils.GetTrianglePointsByRadius(CenterPoint, RADIUS);
            dc.DrawPolyline(new[]
            {
                trianglePoints.A, trianglePoints.B, trianglePoints.C, trianglePoints.A
            });
        }

        /// <inheritdoc/>
        public override void OnErase()
        {
            NotifyErased();
            base.OnErase();
        }

        /// <inheritdoc/>
        public override hresult OnMcDeserialization(McSerializationInfo info)
        {
            if (!info.GetValue(nameof(CenterPoint), out CenterPoint))
                return hresult.e_Fail;
            if (!info.GetValue(nameof(_graphVertexForm), out int vertexFormType))
                return hresult.e_Fail;

            _graphVertexForm = GetGraphVertexForm(vertexFormType);
            return hresult.s_Ok;
        }

        /// <inheritdoc/>
        public override hresult OnMcSerialization(McSerializationInfo info)
        {
            info.Add(nameof(CenterPoint), CenterPoint);
            info.Add(nameof(_graphVertexForm), (int)_graphVertexForm);

            return hresult.s_Ok;
        }

        /// <inheritdoc/>
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify())
                return;

            CenterPoint = CenterPoint.TransformBy(tfm);
            NotifyMoved(); // Уведомляем наблюдателей о трансформации
        }

        /// <inheritdoc/>
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            var jig = new InputJig();

            // Выбор типа вершины
            var res = jig.GetIntNumber("Выберите тип объекта(1 круг, остальное или enter - треугольник):", out var graphVertexFormType);
            if (!res)
                return hresult.e_Fail;

            // Выбор позиции
            var pointInputResult = jig.GetPoint("Выберите точку вставки:");
            if (pointInputResult.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;

            CenterPoint = pointInputResult.Point;
            _graphVertexForm = GetGraphVertexForm(graphVertexFormType);

            DbEntity.AddToCurrentDocument();
            jig.ExcludeObject(ID);

            // Интерактивное перемещение
            jig.MouseMove = (s, a) =>
            {
                TryModify();
                CenterPoint = a.Point;
                DbEntity.Update();
                InputJig.PropertyInpector.UpdateProperties();
            };

            return hresult.s_Ok;
        }

        /// <summary>
        /// Возвращает enum-представление формы вершины по её номеру.
        /// </summary>
        private GraphVertexForm GetGraphVertexForm(int type)
        {
            return type == 1 ? GraphVertexForm.Circle : GraphVertexForm.Triangle;
        }
    }

    /// <summary>
    /// Форма вершины.
    /// </summary>
    public enum GraphVertexForm
    {
        Triangle,
        Circle
    }
}