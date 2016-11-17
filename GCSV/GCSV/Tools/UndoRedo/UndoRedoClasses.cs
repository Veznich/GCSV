

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SV.FileTools;


namespace SV.UndoRedoTools
{


	/// <summary>
	/// тип файла
	/// </summary>
	public enum TypeFileRead
		{
		/// <summary>
		/// просто файл
		/// </summary>
			File,
		/// <summary>
		/// порецируемый на память
		/// </summary>
			MappedFile,
		/// <summary>
		/// по блочно читаемый
		/// </summary>
			BlockFile
		}
	/// <summary>
	/// класс отмены последних действий
	/// </summary>
	public class UndoRedoClass : IDisposable
	{

#region Constructors

		/// <summary>
		/// конструктор
		/// </summary>
		public UndoRedoClass()
		{
			m_typeFileRead = TypeFileRead.File;
			//m_fileMapping = new FileMappingWorking(this.GetType().GUID+".tmp");
		}
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="type">тип вида работы с файлами</param>
		public  UndoRedoClass(TypeFileRead type)
		{
			m_typeFileRead = type;
			//m_fileMapping = new FileMappingWorking(this.GetType().GUID+".tmp");
		}
#endregion

#region Destructor
		private bool disposed = false;
		/// <summary>
		/// освобождение ресурсов
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				Close();
				disposed = true;
			}
		}
		/// <summary>
		/// деструктор
		/// </summary>
		~UndoRedoClass()
        {
           Dispose(false);
        }
		#endregion

#region PROPERTY

		private TypeFileRead m_typeFileRead;
		/// <summary>
		/// возращает текущий тип вида работы с файлами
		/// </summary>
		public TypeFileRead TypeFileStream
		{
			get { return m_typeFileRead; }
		}

		private int m_currPosition = -1;
		/// <summary>
		/// последняя позиция в списке действий
		/// </summary>
		public int LastRedactPosition
		{
			get	{
				return m_currPosition;
			}
		}
		/// <summary>
		/// количество зарегестрированных действий (откатов)
		/// </summary>
		public int CountListPosition
		{
			get { if (m_PositionObjectList == null) return 0;
				return m_PositionObjectList.Count;
			}
		}

		bool m_isOpen = false;
		/// <summary>
		/// открыт ли файл
		/// </summary>
		public bool isOpen
		{
			get { return m_isOpen; }
		}
#endregion
#region FIELDS

		private List<long> m_PositionObjectList = null;
		private SV.FileTools.IFileWorking m_fileMapping = null;
#endregion

#region FUNCTION
		/// <summary>
		/// добавляет старый обьект для внесения в лист откатов
		/// </summary>
		/// <param name="oldObject">старый обьект подвергутый изменению</param>
		/// <returns></returns>
		public  bool Add(object oldObject)
		{
			if(m_fileMapping == null)
				return false;
			bool rtVal = false;
			if (m_PositionObjectList == null)
				m_PositionObjectList = new List<long>();
			long position = 0;
			if (m_currPosition >= 0)
				position = m_PositionObjectList[m_currPosition];
			//byte [] buffer = SV.ConversionTools.DataCoversion.ObjectToByte(oldObject);
			if (oldObject != null)
			{
				long size = m_fileMapping.Write(position, oldObject);
				if (size > 0)
				{
					long lastSize = 0;
					if (m_currPosition < m_PositionObjectList.Count-1)
						m_PositionObjectList.RemoveRange(m_currPosition+1, m_PositionObjectList.Count - (m_currPosition+1));
					if (m_currPosition >= 0)
						lastSize = m_PositionObjectList[m_currPosition];
					m_PositionObjectList.Add(size + lastSize);
					m_currPosition = m_PositionObjectList.Count-1;
					rtVal = true;
				}
			}
			return rtVal;
		}
		/// <summary>
		/// возращает обьект по его позиции
		/// </summary>
		/// <param name="position"></param>
		/// <param name="stepOb"></param>
		/// <returns></returns>
		private bool GetData(int position, ref object stepOb)
		{
			if (m_fileMapping == null || m_PositionObjectList == null)
				return false;
			bool rtVal = false;
			if (position < m_PositionObjectList.Count)
			{
				long ps = 0;
				long size = m_PositionObjectList[position];
				if (position > 0)
				{
					ps = m_PositionObjectList[position-1];
					size = m_PositionObjectList[position] - ps;
				}
				stepOb = m_fileMapping.Read(ps, size);
				//m_currPosition = position;
				rtVal = true;
			}

			return rtVal;
		}
		/// <summary>
		/// возвращает обьект следующий за текущей позицеей 
		/// </summary>
		/// <param name="stepOb"></param>
		/// <returns></returns>
		public bool Redo(ref object stepOb)
		{
			if ( m_PositionObjectList == null || m_currPosition == m_PositionObjectList.Count -1)
				return false;
			m_currPosition++;
			bool rt = GetData(m_currPosition, ref stepOb);
			
			return rt;
		}
		/// <summary>
		/// возращает обьект предедущий текущей позиции
		/// </summary>
		/// <param name="stepOb"></param>
		/// <returns></returns>
		public bool Undo(ref object stepOb)
		{
			if (m_PositionObjectList == null || m_currPosition < 0)
				return false;
			bool rt = GetData(m_currPosition, ref stepOb);
			m_currPosition--;
			return rt;
		}
		/// <summary>
		/// закрывает все соеденения и очищает класс
		/// </summary>
		public void Close()
		{
			if(m_fileMapping != null)
			{
				m_fileMapping.Close();
				m_fileMapping = null;
			}
			if(m_PositionObjectList != null)
			{
				m_PositionObjectList.Clear();
				m_PositionObjectList.TrimExcess();
				m_PositionObjectList = null;
			}
		}
		/// <summary>
		/// открывает все соеденения с настройками по умолчанию, если класс был инициализтрован, он будет очищен и открыт заново
		/// </summary>
		/// <returns></returns>
		public bool Open()
		{
			return Open(System.IO.Path.GetTempFileName(), TypeFileRead.File);
		}
		/// <summary>
		/// открывает все соеденения, если класс был инициализтрован, он будет очищен и открыт заново
		/// </summary>
		/// <param name="patchFile">путь и имя временного файла</param>
		/// <param name="type">тип вида работы с файлом</param>
		/// <returns></returns>
		public bool Open(string patchFile, TypeFileRead type = TypeFileRead.File)
		{
			Close();
			m_typeFileRead = type;
			switch (m_typeFileRead)
			{
					default:
					case TypeFileRead.File:
						m_fileMapping = new FileWorking(patchFile, true);
					break;
			}

			m_isOpen = m_fileMapping.IsOpen;
			return m_isOpen;
		}
#endregion

	}

}