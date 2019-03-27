//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 17:55:37
//============================================================

using System.Collections.Generic;

namespace UndoMethods
{
    /// <summary>
    /// 将列表用作堆栈的扩展方法
    /// </summary>
    public static class ListStackExtensions
    {
        public static void Push(this List<IUndoRedoRecord> list, IUndoRedoRecord item)
        {
            list.Insert(0, item);
        }

        public static IUndoRedoRecord Pop(this List<IUndoRedoRecord> list)
        {
            var ret = list[0];
            list.RemoveAt(0);
            return ret;
        }
    }
}