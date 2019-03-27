//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 16:53:06
//============================================================

namespace UndoMethods
{
    /// <summary>
    /// UndoRedo¼ÇÂ¼½Ó¿Ú
    /// </summary>
    public interface IUndoRedoRecord
    {
        string Name { get; }
        void Execute();
    }
}