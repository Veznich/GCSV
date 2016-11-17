

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SV.FileTools;


namespace SV.UndoRedoTools
{


	/// <summary>
	/// ��� �����
	/// </summary>
	public enum TypeFileRead
		{
		/// <summary>
		/// ������ ����
		/// </summary>
			File,
		/// <summary>
		/// ������������ �� ������
		/// </summary>
			MappedFile,
		/// <summary>
		/// �� ������ ��������
		/// </summary>
			BlockFile
		}
	/// <summary>
	/// ����� ������ ��������� ��������
	/// </summary>
	public class UndoRedoClass : IDisposable
	{

#region Constructors

		/// <summary>
		/// �����������
		/// </summary>
		public UndoRedoClass()
		{
			m_typeFileRead = TypeFileRead.File;
			//m_fileMapping = new FileMappingWorking(this.GetType().GUID+".tmp");
		}
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="type">��� ���� ������ � �������</param>
		public  UndoRedoClass(TypeFileRead type)
		{
			m_typeFileRead = type;
			//m_fileMapping = new FileMappingWorking(this.GetType().GUID+".tmp");
		}
#endregion

#region Destructor
		private bool disposed = false;
		/// <summary>
		/// ������������ ��������
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
		/// ����������
		/// </summary>
		~UndoRedoClass()
        {
           Dispose(false);
        }
		#endregion

#region PROPERTY

		private TypeFileRead m_typeFileRead;
		/// <summary>
		/// ��������� ������� ��� ���� ������ � �������
		/// </summary>
		public TypeFileRead TypeFileStream
		{
			get { return m_typeFileRead; }
		}

		private int m_currPosition = -1;
		/// <summary>
		/// ��������� ������� � ������ ��������
		/// </summary>
		public int LastRedactPosition
		{
			get	{
				return m_currPosition;
			}
		}
		/// <summary>
		/// ���������� ������������������ �������� (�������)
		/// </summary>
		public int CountListPosition
		{
			get { if (m_PositionObjectList == null) return 0;
				return m_PositionObjectList.Count;
			}
		}

		bool m_isOpen = false;
		/// <summary>
		/// ������ �� ����
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
		/// ��������� ������ ������ ��� �������� � ���� �������
		/// </summary>
		/// <param name="oldObject">������ ������ ����������� ���������</param>
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
		/// ��������� ������ �� ��� �������
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
		/// ���������� ������ ��������� �� ������� �������� 
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
		/// ��������� ������ ���������� ������� �������
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
		/// ��������� ��� ���������� � ������� �����
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
		/// ��������� ��� ���������� � ����������� �� ���������, ���� ����� ��� ���������������, �� ����� ������ � ������ ������
		/// </summary>
		/// <returns></returns>
		public bool Open()
		{
			return Open(System.IO.Path.GetTempFileName(), TypeFileRead.File);
		}
		/// <summary>
		/// ��������� ��� ����������, ���� ����� ��� ���������������, �� ����� ������ � ������ ������
		/// </summary>
		/// <param name="patchFile">���� � ��� ���������� �����</param>
		/// <param name="type">��� ���� ������ � ������</param>
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