namespace GraphBuilder.Ncad.Utils;

using HostMgd.ApplicationServices;

using Multicad;

using Teigha.DatabaseServices;

/// <summary>
/// Утилиты для CAD-объектов.
/// </summary>
public static class CadUtils
{
    /// <summary>
    /// Возвращает все McObjectId объектов из документа.
    /// </summary>
    /// <param name="document"> Документ. </param>
    /// <param name="transaction"> Транзакция. </param>
    /// <returns> Список id всех найденных объектов. </returns>
    public static List<McObjectId> GetAllDocumentObjectIds(Document document, Transaction transaction)
    {
        var allObjectsIds = GetModelSpaceBtr(document.Database,
            transaction,
            OpenMode.ForRead).Cast<ObjectId>().Select(x => McObjectId.FromHandle(x.Handle.Value));

        return allObjectsIds.ToList();
    }

    /// <summary>
    /// Получить запись таблицы блоков пространства чертежа, в котором хранится вся геометрия.
    /// </summary>
    /// <param name="database"> База чертежа. </param>
    /// <param name="transaction"> Транзакция чертежа. </param>
    /// <param name="openMode"> Режим доступа. </param>
    /// <returns> Возвращает запись таблицы блоков пространства чертежа. </returns>
    public static BlockTableRecord GetModelSpaceBtr(Database database, Transaction transaction,
        OpenMode openMode)
    {
        var bt = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
        return (BlockTableRecord)transaction.GetObject(bt[BlockTableRecord.ModelSpace], openMode);
    }
}