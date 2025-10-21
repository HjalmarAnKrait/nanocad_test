namespace GraphBuilder.Ncad.Interface
{
    using GraphBuilder.Ncad.CadObjects;

    /// <summary>
    /// Интерфейс для объектов, которые могут слушать изменения вершин
    /// </summary>
    public interface IVertexObserver
    {
        /// <summary>
        /// Событие перемещения вершины.
        /// </summary>
        /// <param name="vertex"> Перемещаемая вершина. </param>
        void OnVertexMoved(GraphVertex vertex);
        
        /// <summary>
        /// Событие удаления вершины.
        /// </summary>
        /// <param name="vertex"> Удаляемая вершина. </param>
        void OnVertexErased(GraphVertex vertex);
    }

    /// <summary>
    /// Интерфейс для наблюдаемых вершин
    /// </summary>
    public interface IVertexObservable
    {
        /// <summary>
        /// Добавляет наблюдателя.
        /// </summary>
        /// <param name="observer"> Наблюдатель </param>
        void AddObserver(IVertexObserver observer);
        
        /// <summary>
        /// Удаляет наблюдателя.
        /// </summary>
        /// <param name="observer"> Наблюдатель </param>
        void RemoveObserver(IVertexObserver observer);
        
        /// <summary>
        /// Уведомляет о перемещении.
        /// </summary>
        void NotifyMoved();
        
        /// <summary>
        /// Уведомляет о удалении.
        /// </summary>
        void NotifyErased();
    }
}