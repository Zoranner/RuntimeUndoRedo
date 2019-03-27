//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 17:23:35
//============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace UndoMethods
{
    /// <summary>
    /// 存储和执行撤销/重做记录的单例
    /// </summary>
    public class UndoRedoManager
    {
        public delegate void OnStackStatusChanged(bool hasItems);

        /// <summary>
        /// 存储此单例对象的实例
        /// </summary>
        private static volatile UndoRedoManager _ThisObject = new UndoRedoManager();

        /// <summary>
        /// 存储当前的撤销/重做事务
        /// </summary>
        private UndoTransaction _CurTran;

        /// <summary>
        /// 撤销/重做堆栈的最大值
        /// </summary>
        protected int _MaxItems = 10;

        /// <summary>
        /// 标记是否正在进行重做操作
        /// </summary>
        private bool _RedoGoingOn;

        /// <summary>
        /// 存储重做记录
        /// </summary>
        private readonly List<IUndoRedoRecord> _RedoStack = new List<IUndoRedoRecord>();

        /// <summary>
        /// 标记是否正在进行撤销操作
        /// </summary>
        private bool _UndoGoingOn;

        /// <summary>
        /// 存储撤销记录
        /// </summary>
        private readonly List<IUndoRedoRecord> _UndoStack = new List<IUndoRedoRecord>();

        /// <summary>
        /// 撤销/重做堆栈中的最大项
        /// </summary>
        public int MaxItems
        {
            get { return _MaxItems; }
            set
            {
                if (_MaxItems <= 0)
                {
                    throw new ArgumentOutOfRangeException("Max items can't be <= 0");
                }

                _MaxItems = value;
            }
        }

        [UsedImplicitly]
        public int UndoOperationCount
        {
            get { return _UndoStack.Count; }
        }

        [UsedImplicitly]
        public int RedoOperationCount
        {
            get { return _RedoStack.Count; }
        }

        public bool HasUndoOperations
        {
            get { return _UndoStack.Count != 0; }
        }

        public bool HasRedoOperations
        {
            get { return _RedoStack.Count != 0; }
        }

        /// <summary>
        /// 在更改撤消堆栈状态时触发
        /// </summary>
        public event OnStackStatusChanged UndoStackStatusChanged;

        /// <summary>
        /// 在更改重做堆栈状态时触发
        /// </summary>
        public event OnStackStatusChanged RedoStackStatusChanged;

        /// <summary>
        /// 返回此单例对象的实例
        /// </summary>
        public static UndoRedoManager Instance()
        {
            return _ThisObject;
        }

        /// <summary>
        /// 启动一个事务在该事务下执行所有撤消重做操作
        /// </summary>
        /// <param name="tran"></param>
        public void StartTransaction(UndoTransaction tran)
        {
            if (_CurTran != null)
            {
                return;
            }

            _CurTran = tran;
            // 推入一个空的撤消操作
            _UndoStack.Push(new UndoTransaction(tran.Name));
            _RedoStack.Push(new UndoTransaction(tran.Name));
        }

        /// <summary>
        /// 结束在其下进行所有撤消重做操作的事务
        /// </summary>
        /// <param name="tran"></param>
        public void EndTransaction(UndoTransaction tran)
        {
            if (_CurTran != tran)
            {
                return;
            }

            _CurTran = null;
            // 检查顶部的空事务并将其删除
            if (_UndoStack.Count > 0)
            {
                var t = _UndoStack[0] as UndoTransaction;
                if (t != null && t.OperationsCount == 0)
                {
                    _UndoStack.Pop();
                }
            }

            if (_RedoStack.Count > 0)
            {
                var t = _RedoStack[0] as UndoTransaction;
                if (t != null && t.OperationsCount == 0)
                {
                    _RedoStack.Pop();
                }
            }
        }

        /// <summary>
        /// 向撤销/重做堆栈推入操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="undoOperation"></param>
        /// <param name="undoData"></param>
        /// <param name="description"></param>
        public void Push<T>(UndoRedoOperation<T> undoOperation, T undoData, string description = "")
        {
            List<IUndoRedoRecord> stack;
            Action eventToFire;

            // 通过撤销/重做标识决定如何堆栈
            if (_UndoGoingOn)
            {
                stack = _RedoStack;
                eventToFire = FireRedoStackStatusChanged;
            }
            else
            {
                stack = _UndoStack;
                eventToFire = FireUndoStackStatusChanged;
            }

            if (!_UndoGoingOn && !_RedoGoingOn)
            {
                _RedoStack.Clear();
                FireRedoStackStatusChanged();
            }
            
            // 如果正在进行事务，则将该操作作为条目添加到事务操作中
            if (_CurTran == null)
            {
                stack.Push(new UndoRedoRecord<T>(undoOperation, undoData, description));
            }
            else
            {
                ((UndoTransaction) stack[0]).AddUndoRedoOperation(new UndoRedoRecord<T>(undoOperation, undoData,
                    description));
            }

            // 堆栈计数超过允许的最大项
            if (stack.Count > MaxItems)
            {
                stack.RemoveRange(MaxItems - 1, stack.Count - MaxItems);
            }

            // 通知消费者堆栈大小已更改
            eventToFire();
        }

        private void FireUndoStackStatusChanged()
        {
            if (null != UndoStackStatusChanged)
            {
                UndoStackStatusChanged(HasUndoOperations);
            }
        }

        private void FireRedoStackStatusChanged()
        {
            if (null != RedoStackStatusChanged)
            {
                RedoStackStatusChanged(HasRedoOperations);
            }
        }

        /// <summary>
        /// 执行撤销操作
        /// </summary>
        public void Undo()
        {
            try
            {
                _UndoGoingOn = true;

                if (_UndoStack.Count == 0)
                {
                    throw new InvalidOperationException("Nothing in the undo stack");
                }

                object oUndoData = _UndoStack.Pop();

                var undoDataType = oUndoData.GetType();

                // 如果存储的操作是事务，则也将撤消作为事务执行
                if (typeof(UndoTransaction) == undoDataType)
                {
                    StartTransaction(oUndoData as UndoTransaction);
                }

                undoDataType.InvokeMember("Execute", BindingFlags.InvokeMethod, null, oUndoData, null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            finally
            {
                _UndoGoingOn = false;

                EndTransaction(_CurTran);

                FireUndoStackStatusChanged();
            }
        }

        /// <summary>
        /// 执行重做操作
        /// </summary>
        public void Redo()
        {
            try
            {
                _RedoGoingOn = true;
                if (_RedoStack.Count == 0)
                {
                    throw new InvalidOperationException("Nothing in the redo stack");
                }

                object oUndoData = _RedoStack.Pop();

                var undoDataType = oUndoData.GetType();

                // 如果存储的操作是事务，则也将重做作为事务执行
                if (typeof(UndoTransaction) == undoDataType)
                {
                    StartTransaction(oUndoData as UndoTransaction);
                }

                undoDataType.InvokeMember("Execute", BindingFlags.InvokeMethod, null, oUndoData, null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            finally
            {
                _RedoGoingOn = false;
                EndTransaction(_CurTran);

                FireRedoStackStatusChanged();
            }
        }

        /// <summary>
        /// 清空所有撤销/重做操作
        /// </summary>
        [UsedImplicitly]
        public void Clear()
        {
            _UndoStack.Clear();
            _RedoStack.Clear();
            FireUndoStackStatusChanged();
            FireRedoStackStatusChanged();
        }

        /// <summary>
        /// 返回包含所有撤消堆栈记录的说明列表
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public IList<string> GetUndoStackInformation()
        {
            return _UndoStack.ConvertAll(input => input.Name ?? "");
        }

        /// <summary>
        /// 返回包含所有重做堆栈记录的说明列表
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public IList<string> GetRedoStackInformation()
        {
            return _RedoStack.ConvertAll(input => input.Name ?? "");
        }
    }
}