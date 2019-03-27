//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 17:15:58
//============================================================

using System.Diagnostics;

namespace UndoMethods
{
    public delegate void UndoRedoOperation<in T>(T undoData);

    public class UndoRedoRecord<T> : IUndoRedoRecord
    {
        private UndoRedoOperation<T> _Operation;
        private T _UndoData;

        public UndoRedoRecord()
        {
        }


        public UndoRedoRecord(UndoRedoOperation<T> operation, T undoData, string description = "")
        {
            SetInfo(operation, undoData, description);
        }


        public void Execute()
        {
            Trace.TraceInformation("Undo/Redo operation {0} with data {1} - {2}", _Operation, _UndoData, Name);
            _Operation(_UndoData);
        }

        public string Name { get; private set; }

        public void SetInfo(UndoRedoOperation<T> operation, T undoData, string description = "")
        {
            _Operation = operation;
            _UndoData = undoData;
            Name = description;
        }
    }
}