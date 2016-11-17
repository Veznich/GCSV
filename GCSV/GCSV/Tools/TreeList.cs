using System;
using System.Collections;
using System.ComponentModel;

namespace SV.TreeTools
{
	public enum OperationTree
	{
		Insert,
		Add,
		Remove
	}
	/// <summary>
	/// интерфейс древовоидного листа
	/// </summary>
	public interface ITreeMemItem : IBindingList
	{
		/// <summary>
		/// освобождение ресурсов
		/// </summary>
		void Dispose();
		/// <summary>
		/// родительский узел
		/// </summary>
		ITreeMemItem Parent { get; set; }
		/// <summary>
		/// главный узел дерева
		/// </summary>
		ITreeMemItem Root { get; }
		/// <summary>
		/// создает новый обьект узла
		/// </summary>
		/// <param name="interfeceList">родительский узел</param>
		/// <returns>созданный узел</returns>
		ITreeMemItem New(ITreeMemItem parent = null);
		/// <summary>
		/// добавление узла в список дочерних с автоматическим добавлением родителя
		/// </summary>
		/// <param name="item">добавляемый дочерний узел</param>
		void Add(ITreeMemItem item);
		/// <summary>
		/// возращает полное количество дочерних узлов включая вложенные
		/// </summary>
		int CountAllChild { get;}
		/// <summary>
		/// клонирование
		/// </summary>
		/// <returns></returns>
		//ITreeMemItem Clone();

	}
	/// <summary>
	/// реализация интерфейса древовидного листа
	/// </summary>
	[Serializable()]
	public class TreeMemItem : BindingList<ITreeMemItem>, IDisposable, ITreeMemItem
	{
#region CONSTRUCTOR
		/// <summary>
		/// конструктор
		/// </summary>
		public TreeMemItem()
		{
		}
		/// <summary>
		/// конструктор 
		/// </summary>
		/// <param name="interfeceList">родительский узел</param>
		public TreeMemItem(ITreeMemItem parent)
		{
			m_parentItem = parent;
		}
#endregion

#region DESTRUCTOR
		bool isDispose = false;
		/// <summary>
		/// диструктор
		/// </summary>
		~TreeMemItem()
		{
			Dispose(false);
		}

		/// <summary>
		/// освобождение ресурсов
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.isDispose)
			{
				Clear();
				//TrimExcess();
				m_parentItem = null;
				isDispose = true;
			}
		}

#endregion

#region PARENT
		/// <summary>
		/// верхний узел(родитель)
		/// </summary>
		[NonSerialized]
		ITreeMemItem m_parentItem = null;
		/// <summary>
		/// верхний узел(родитель)
		/// </summary>
		public ITreeMemItem Parent
		{
			get { return m_parentItem; }
			set { m_parentItem = value; }
		}
		/// <summary>
		/// гавный узел дерева
		/// </summary>
		public ITreeMemItem Root
		{
			get
			{
				if (m_parentItem == null)
					return this;
				return m_parentItem.Root;
			}
		}

#endregion

#region CHILD
		public virtual void MessageInsertAddRemove(ITreeMemItem item, OperationTree opertion)
		{

		}
		/// <summary>
		/// создает новый узел
		/// </summary>
		/// <param name="interfeceList">родитель</param>
		/// <returns>созданный узел</returns>
		public ITreeMemItem New(ITreeMemItem parent = null)
		{
			return new TreeMemItem(parent);
		}
		/// <summary>
		/// добавление узла в список дочерних с автоматическим добавлением родителя
		/// </summary>
		/// <param name="item">добавляемый дочерний узел</param>
		public new void Add(ITreeMemItem item)
		{
			if (item != null)
				item.Parent = this;
			base.Add(item);
			MessageInsertAddRemove(item, OperationTree.Add);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public new void Insert(int index, ITreeMemItem item)
		{
			if (item != null)
				item.Parent = this;
			base.Insert(index, item);
			MessageInsertAddRemove(item, OperationTree.Insert);
		}

		public void Remove(object value)
		{
			base.Remove(value as ITreeMemItem);
			MessageInsertAddRemove(this, OperationTree.Remove);
		}
		
		public new void RemoveAt(int index)
		{
			base.RemoveAt(index);
			MessageInsertAddRemove(this, OperationTree.Remove);
		}

		void IList.Insert(int index, object item)
		{
			Insert(index, item as ITreeMemItem);
		}

		/// <summary>
		/// возращает полное количество дочерних узлов включая вложенные
		/// </summary>
		public int CountAllChild {
			get
			{
				int cnt = Count;
				for (int a = 0; a < Count; a++ )
				{
					cnt += this[a].CountAllChild;
				}
				return cnt;
			}
		}
#endregion

#region CLONE
		/*
		public virtual ITreeMemItem Clone()
				{
					return this.MemberwiseClone() as ITreeMemItem;
				}*/
		

#endregion
	}

}
