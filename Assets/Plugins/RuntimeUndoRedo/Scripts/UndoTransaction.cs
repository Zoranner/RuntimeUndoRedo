//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 17:32:27
//============================================================

using System;
using System.Collections.Generic;

namespace UndoMethods
{
    public class UndoTransaction : IDisposable, IUndoRedoRecord
    {
        private readonly string _Name;
        private readonly List<IUndoRedoRecord> _UndoRedoOperations = new List<IUndoRedoRecord>();

        public UndoTransaction(string name = "")
        {
            _Name = name;
            UndoRedoManager.Instance().StartTransaction(this);
        }

        public int OperationsCount
        {
            get { return _UndoRedoOperations.Count; }
        }

        public void Dispose()
        {
            UndoRedoManager.Instance().EndTransaction(this);
        }


        public string Name
        {
            get { return _Name; }
        }

#region Implementation of IUndoRedoRecord

        public void Execute()
        {
            _UndoRedoOperations.ForEach(a => a.Execute());
        }

#endregion

        public void AddUndoRedoOperation(IUndoRedoRecord operation)
        {
            _UndoRedoOperations.Insert(0, operation);
        }
    }
}