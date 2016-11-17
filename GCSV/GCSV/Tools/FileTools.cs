
using System;
using System.Collections.Generic;
using System.IO;
//using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using SV.ConversionTools;

namespace SV.FileTools
{

#region FileMappingWorking
	/// <summary>
	/// игтерфейс работы с файлами
	/// </summary>
	public interface IFileWorking
	{
		/// <summary>
		/// имя
		/// </summary>
		string FileName { get; }
		/// <summary>
		/// размер
		/// </summary>
		long Size { get; }
		/// <summary>
		/// открытый
		/// </summary>
		bool IsOpen { get; }
		/// <summary>
		/// скрытый
		/// </summary>
		bool IsHidden { get; }
		/// <summary>
		/// закрытие
		/// </summary>
		/// <param name="patchFile"></param>
		/// <param name="isHidden"></param>
		/// <returns></returns>
		bool Open(string patchFile, bool isHidden);
		/// <summary>
		/// закрытие
		/// </summary>
		void Close();
		/// <summary>
		/// чтение
		/// </summary>
		/// <param name="position"></param>
		/// <param name="readBytes"></param>
		/// <returns></returns>
		object Read(long position, long readBytes);
		/// <summary>
		/// запись
		/// </summary>
		/// <param name="position"></param>
		/// <param name="ob"></param>
		/// <returns></returns>
		long Write (long position, object ob);
	}
	/// <summary>
	/// работа с файлом
	/// </summary>
	public class FileWorking : IDisposable, IFileWorking
	{
		#region Constructor
		/// <summary>
		/// контсруктор
		/// </summary>
		public FileWorking()
		{
			m_filePatch = "";
			isHidden = false;
		}
		/// <summary>
		/// контсруктор
		/// </summary>
		/// <param name="patchFile"></param>
		/// <param name="isHidden"></param>
		public FileWorking(string patchFile, bool isHidden)
		{
			Open(patchFile, isHidden);
		}
		#endregion
		#region Destructor
		private bool disposed = false;
		/// <summary>
		/// освобождение
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
		~FileWorking()
        {
           Dispose(false);
        }
		#endregion

		#region Property
		private string m_filePatch;
		/// <summary>
		/// имя файла
		/// </summary>
		public string FileName
		{
			get { return m_filePatch; }
		}

		private bool isOpen;
		/// <summary>
		/// открыт файл или нет
		/// </summary>
		public bool IsOpen
		{
			get { return isOpen; }
		}

		private bool isHidden;
		/// <summary>
		/// скрытый
		/// </summary>
		public bool IsHidden
		{
			get { return isHidden; }
		}
		/// <summary>
		/// текущий размер файла
		/// </summary>
		public long Size
		{
			get { if (m_streamFile != null) return m_streamFile.Length;
				return 0;
			}	
		}

		private FileStream m_streamFile;
		#endregion

		#region Function
		/// <summary>
		/// открытие
		/// </summary>
		/// <param name="patchFile"></param>
		/// <param name="isHidden"></param>
		/// <returns></returns>
		public bool Open(string patchFile, bool isHidden)
		{
			if (isOpen)
				return false;
			m_filePatch = patchFile;
			this.isHidden = isHidden;
			Close();
			m_streamFile = File.Create(m_filePatch);//, 1024, FileOptions.DeleteOnClose);
			File.SetAttributes(m_filePatch, ( isHidden ? System.IO.FileAttributes.Hidden : System.IO.FileAttributes.Normal));
			//File.SetAttributes(m_filePatch, System.IO.FileAttributes.System);
			//byte [] b = new byte[1];
			//b[0] = 0x10;
			//st.Write(b, 0, 1);
			//st.Close();
			//m_memoryMappedFile = MemoryMappedFile.CreateFromFile(m_filePatch,  FileMode.Open, this.GetType().GUID.ToString());
			//m_memoryMappedViewStream = m_memoryMappedFile.CreateViewStream();
			isOpen = true;
			return isOpen;
		}
		/// <summary>
		/// закрытие
		/// </summary>
		public void Close()
		{
			if (m_streamFile != null)
			{
				m_streamFile.Close();
				m_streamFile.Dispose();
				m_streamFile = null;
			}
			isOpen = false;
			if (!String.IsNullOrEmpty(m_filePatch) || File.Exists(m_filePatch))
				File.Delete(m_filePatch);

		}
		/// <summary>
		/// чтение
		/// </summary>
		/// <param name="position"></param>
		/// <param name="readBytes"></param>
		/// <returns></returns>
		public object Read(long position, long readBytes)
		{
			//m_memoryMappedViewStream = m_memoryMappedFile.CreateViewStream();
			//m_memoryMappedViewStream.Seek(position, SeekOrigin.Begin);
			byte [] buffer = new byte[readBytes];
			long lg = buffer.LongLength;
			m_streamFile.Seek(position, SeekOrigin.Begin);
			m_streamFile.Read(buffer, 0, (int)lg);
			//BinaryReader streamWriter = new BinaryReader(m_memoryMappedViewStream);
			//streamWriter.Read(buffer, 0, (int)readBytes);
			//streamWriter.Close();
			//m_memoryMappedViewStream.Read(buffer, 0, (int)readBytes);
			//m_memoryMappedViewStream.Close();
			//m_memoryMappedViewStream = null;
			return  DataCoversion.ByteToObject(buffer);
		}
		/// <summary>
		/// запись
		/// </summary>
		/// <param name="position"></param>
		/// <param name="ob"></param>
		/// <returns></returns>
		public long Write (long position, object ob)
		{
			if (!isOpen)
				return 0;
			byte[] buffer = DataCoversion.ObjectToByte(ob);
			long lg = buffer.Length;

			//long Filesize = m_streamFile.Length;

			m_streamFile.Seek(position, SeekOrigin.Begin);
			m_streamFile.Write(buffer, 0, buffer.Length);
			//MemoryMappedViewAccessor acessor = m_memoryMappedFile.CreateViewAccessor(position, position + lg);
			//acessor.Write(position, ref buffer);
			//m_memoryMappedViewStream.Seek(position, SeekOrigin.Begin);
			//m_memoryMappedViewStream.Write(buffer, 0, buffer.Length);
			//BinaryWriter streamWriter = new BinaryWriter(m_memoryMappedViewStream);
			//streamWriter.Write(buffer);
			//streamWriter.Close();
			//m_memoryMappedViewStream.Flush();
			//m_memoryMappedViewStream.Close();
			//m_memoryMappedViewStream = null;
			return lg;
		}
		
		#endregion
	}
#endregion

#region FileReadBlock
	/// <summary>
	/// проблочное чтение из файла
	/// </summary>
	public class FileReadBlock : IDisposable, IFileWorking
	{
#region Constructor
		/// <summary>
		/// контсруктор
		/// </summary>
		public FileReadBlock()
		{
		 	
		}
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="BlockSize">размер блока в байтах</param>
		public  FileReadBlock(int BlockSize)
		{
			m_iSizeBlock = BlockSize;
		}
		/// <summary>
		/// контсруктор
		/// </summary>
		/// <param name="fullFileName"></param>
		public  FileReadBlock(string fullFileName)
		{
			m_strFullName = fullFileName;
		}
		/// <summary>
		/// контсруктор
		/// </summary>
		/// <param name="fullFileName"></param>
		/// <param name="BlockSize"></param>
		public  FileReadBlock(string fullFileName, int BlockSize)
			:this(fullFileName)
		{
			m_iSizeBlock = BlockSize;
		}
#endregion

#region Destructor
		private bool disposed = false;
		/// <summary>
		/// освобождение
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
		~FileReadBlock()
        {
           Dispose(false);
        }
#endregion

		private string m_strFullName = "";
		private long m_lgCurrPosition = 0;
		private int m_iSizeBlock = 1024;
		private FileStream m_fileStream = null;
		private bool m_bIsOpen;
		private string m_strLastError = "";
		private System.Text.Encoding m_encoding = System.Text.Encoding.Default;
#region PROPERTY
		/// <summary>
		/// размер блока чтения в байтах
		/// </summary>
		public int SizeBlock
		{
			get { return m_iSizeBlock; }
			set { if(value > 0) m_iSizeBlock = value; }
		}
		/// <summary>
		/// полное имя файла 
		/// </summary>
		public string FileName
		{
			get { return m_strFullName; }
			set { m_strFullName = value; }
		}
		/// <summary>
		/// открыт файл или нет
		/// </summary>
		public bool IsOpen
		{ get { return m_bIsOpen; } }
		/// <summary>
		/// описание последней ошибки
		/// </summary>
		public string GetLastErorr
		{
			get { return m_strLastError; }
		}

		private bool isHidden;
		/// <summary>
		/// являеться ли файл скрытым
		/// </summary>
		public bool IsHidden
		{
			get { return isHidden; }
		}
		/// <summary>
		/// кодировка
		/// </summary>
		public System.Text.Encoding EncodingDataFile
		{
			get { return m_encoding; }
			set { if(value != null) m_encoding = value; }
		}
		/// <summary>
		/// возвращает текущий размер
		/// </summary>
		public long Size
		{
			get
			{
				if (m_fileStream != null) return m_fileStream.Length;
				return 0;
			}
		}

#endregion
		/// <summary>
		/// закрытие файла
		/// </summary>
		public void Close()
		{
			m_bIsOpen = false;
			if (m_fileStream != null)
			{
				m_fileStream.Close();
				m_fileStream.Dispose();
				m_fileStream = null;
			}
		}
		/// <summary>
		/// открытие файла
		/// </summary>
		/// <param name="patchFile">путь</param>
		/// <param name="isHidden">скрытый</param>
		/// <returns></returns>
		public bool Open(string patchFile, bool isHidden)
		{
			m_strFullName = patchFile;
			this.isHidden = isHidden;
			return Open();
		}
		/// <summary>
		/// открытие
		/// </summary>
		/// <returns></returns>
		public bool Open()
		{
			Close();
			if (m_strFullName.Length == 0)
				return IsOpen;
			if(!File.Exists(m_strFullName))
			{
				m_strLastError = "File " + m_strFullName + " Not Faund!!!";
				return IsOpen;
			}

			m_fileStream = new FileStream(m_strFullName, FileMode.Open);
			if(!m_fileStream.CanRead)
			{
				Close();
				return IsOpen;
			}
			File.SetAttributes(m_strFullName, (isHidden ? System.IO.FileAttributes.Hidden : System.IO.FileAttributes.Normal));
			m_bIsOpen = true;
			return IsOpen;
		}
		/// <summary>
		/// возрат в начало файла
		/// </summary>
		public void GoBeginFile()
		{
			m_lgCurrPosition = 0;
		}
		/// <summary>
		/// чтение данных
		/// </summary>
		/// <param name="position">позиция </param>
		/// <param name="readBytes">размер</param>
		/// <returns></returns>
		public object Read(long position, long readBytes)
		{
			if (!IsOpen)
				return false;
			if (position + readBytes >= m_fileStream.Length)
				return false;
			string readBlock;
			byte[] buffer = new byte[readBytes];
			m_fileStream.Seek(position, SeekOrigin.Begin);
			m_fileStream.Read(buffer, 0, (int)readBytes);
			readBlock = m_encoding.GetString(buffer);
			return readBlock;
		}
		/// <summary>
		/// чтение данных
		/// </summary>
		/// <param name="readBlock"></param>
		/// <returns></returns>
		public bool Read(ref string readBlock)
		{
			if (!IsOpen)
				return false;
			if (m_lgCurrPosition >= m_fileStream.Length)
				return false;

			byte[] buffer = new byte[m_iSizeBlock];
			m_fileStream.Seek(m_lgCurrPosition, SeekOrigin.Begin);	
			m_fileStream.Read(buffer, 0, m_iSizeBlock);
			m_lgCurrPosition += m_iSizeBlock;
			readBlock = m_encoding.GetString(buffer);
			return true;
		}
		/// <summary>
		/// запись данных
		/// </summary>
		/// <param name="position">начальная позиция</param>
		/// <param name="ob">обьект для записи</param>
		/// <returns></returns>
		public long Write(long position, object ob)
		{
			if (!IsOpen)
				return 0;
			m_fileStream.Seek(position, SeekOrigin.Begin);
			byte[] buffer = SV.ConversionTools.DataCoversion.ObjectToByte(ob);
			m_fileStream.Write(buffer, 0, buffer.Length);
			return buffer.Length;
		}
	}
	#endregion

#region FileSelectDialog
     public class NodeFoldersFile
    {
        NodeFoldersFile parent = null;
        public NodeFoldersFile Parent {
            get { return parent; }
            set { parent = value; }
        }
        public string Name { get; set; } = "";
        public bool isFolder { get; set; } = true;
        public List<NodeFoldersFile> Child { get; set; } = new List<NodeFoldersFile>();
       
        public NodeFoldersFile(string name, NodeFoldersFile prn=null, bool isFld=true)
        {
            Name = name;
            isFolder = isFld;
            parent = prn;
        }
    }
	/// <summary>
	/// статик класс вывода диалогов файлов и папок
	/// </summary>
	public static class SelectDialogs
	{
		/// <summary>
		/// возвращает фильтр для всех файлов All files (*.*)|*.*
		/// </summary>
		public static string DefaultFilers
		{
			get { return "All files (*.*)|*.*"; }
		}
		/// <summary>
		/// диалог выбора папки
		/// </summary>
		/// <returns>полный путь к папке</returns>
		public static string SelectFoldersDialog ()
		{
			System.Windows.Forms.FolderBrowserDialog directoryDialog = new System.Windows.Forms.FolderBrowserDialog();
			if (directoryDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return directoryDialog.SelectedPath;
			return "";
		}
		/// <summary>
		/// Диалог открытия\сохранения файла
		/// </summary>
		/// <param name="filter">строка фильтра в формате "XML files (*.xml)|*.xml"</param>
		/// <param name="filename">имя файла, может быть null</param>
		/// <param name="isOpen">если true то открываеться диалог открытия, иначе диалог сохранения</param>
		/// <returns>возвращает полный путь к файлу, иначе null </returns>
		public static  string OpenFile(string filter, string filename = null, bool isOpen = true)
		{
			string pt = null;
			if (filter == null)
				filter = DefaultFilers;
			System.Windows.Forms.FileDialog FileDialog = null;

			if (isOpen)
			{
				FileDialog = new System.Windows.Forms.OpenFileDialog();
			
			}
			else
			{
				FileDialog = new System.Windows.Forms.SaveFileDialog();
				//openFileDialog1.InitialDirectory = System.;
				if (filename != null)
					FileDialog.FileName = filename;
				
			}

			FileDialog.Filter = filter;
			FileDialog.FilterIndex = 1;
			FileDialog.RestoreDirectory = true;
			if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
					pt = FileDialog.FileName;
			}
			FileDialog.Dispose();
			return pt;
		}
		public static  string OpenFile()
		{
			return OpenFile(SelectDialogs.DefaultFilers);
		}
        public static NodeFoldersFile LoadTreeFiles()
        {
              System.Windows.Forms.FolderBrowserDialog directoryDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (directoryDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return null;
            return LoadTreeFiles(directoryDialog.SelectedPath);
        }
        public static NodeFoldersFile LoadTreeFiles(string path)
        {
            NodeFoldersFile root = new NodeFoldersFile(path);
            AddFilesToNode(root);
            return root;
        }
        static void AddFilesToNode(NodeFoldersFile root)
        {
            if (!root.isFolder)
                return;
            string[] filleList = System.IO.Directory.GetFiles(root.Name);
            foreach (string file in filleList)
            {
                root.Child.Add(new NodeFoldersFile(file, root, false));
            }
            string[] dir = System.IO.Directory.GetDirectories(root.Name);
            foreach (string p in dir)
            {
                NodeFoldersFile dirNode = new NodeFoldersFile(p, root);
                AddFilesToNode(dirNode);
            }
        }
	}
#endregion
}