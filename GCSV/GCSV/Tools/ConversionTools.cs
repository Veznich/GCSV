using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SV.ConversionTools
{
	/// <summary>
	/// ����� ��������� ����������
	/// </summary>
	/// <remarks>����������� ����� ��� ��������� �����������</remarks>
	public static class DataCoversion
	{
		/// <summary>
		/// ������� ���������������� ������ � ������ ������, �������������� ���������� � ������� BinaryFormatter.Serialize
		/// </summary>
		/// <param name="ob">������ ��� ��������������</param>
		/// <returns>��������� ����� ���� � ������� ����� ������������� ���������� ������ ����� Serialize</returns>
		/// <remarks>��� ����������� �������������� ������, ������������ � �������, ������ ����� ����������� ������������</remarks>
		/// <example>
		/// <code>
		/// string st = "test";
		/// byte [] bt = ObjectToByte(st);
		/// </code>
		/// </example>
		public static byte [] ObjectToByte(object ob)
		{
			if (ob == null)
				return null;
			if (!ob.GetType().IsSerializable)
				return null;
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(ms, ob);
			byte[] buffer = ms.GetBuffer();
			ms.Close();
			ms.Dispose();
			return buffer;
		}
		/// <summary>
		/// �������������� ������� ���� � ������ 
		/// </summary>
		/// <param name="bt">����� ����� ������� ����� �������������� � ������ ����� Deserialize</param>
		/// <returns>��������������� ������</returns>
		/// <remarks>��� ����������� �������������� ������, ������������ � ������ ����, ������ ��� ����� ����������� ������������</remarks>
		public static object ByteToObject(byte[] bt)
		{
			if (bt == null)
				return null;
			MemoryStream ms = new MemoryStream(bt);
			BinaryFormatter bf = new BinaryFormatter();
			object ob = null;
			try
			{
				ob = bf.Deserialize(ms);
			}
			catch (Exception ex)
			{

				ob = null;
			}
			
			ms.Close();
			ms.Dispose();
			return ob;
		}
		/// <summary>
		/// ��������� �� ���������� �������� ��� ������������ ���� � ������ � ������������
		/// </summary>
		/// <param name="patch">�������� ������</param>
		/// <returns></returns>
		public static string RemoveFilePathNoCorrectSimvols(string patch)
		{
			//+ { ; " \ = ? ~ ( ) < > & * | $
			string retryVal = patch.Replace("+", "");
			retryVal = retryVal.Replace("{", "");
			retryVal = retryVal.Replace(";", "");
			retryVal = retryVal.Replace("\"", "");
			retryVal = retryVal.Replace("\\", "");
			retryVal = retryVal.Replace("=", "");
			retryVal = retryVal.Replace("?", "");
			retryVal = retryVal.Replace("~", "");
			retryVal = retryVal.Replace("(", "");
			retryVal = retryVal.Replace(")", "");
			retryVal = retryVal.Replace("<", "");
			retryVal = retryVal.Replace(">", "");
			retryVal = retryVal.Replace("&", "");
			retryVal = retryVal.Replace("*", "");
			retryVal = retryVal.Replace("|", "");
			retryVal = retryVal.Replace("$", "");
			return retryVal;
		}
		public static T ConvertByType<T>(string data)
		{
			object ob = ConvertByType(data, typeof(T));
			if(ob is T)
				return (T)ob;
			return default(T);
		}
		/// <summary>
		/// ������� ��������������� ������ �� ����
		/// </summary>
		/// <param name="data">������</param>
		/// <param name="tp">���</param>
		/// <returns>���������</returns>
		public static object ConvertByType(string data, Type tp)
		{
			if (data == null)
				return null;			
			object ob = null;
			if(tp.IsEnum)
			{
				ob = Enum.Parse(tp, data);
			}
			else
			if (tp == typeof(int))
			{
				ob = int.Parse(data);
			}
			else
			if (tp == typeof(float))
			{
				ob = float.Parse(data);
			}
			else
			if (tp == typeof(double))
			{
				ob = double.Parse(data);
			}
			else
			if (tp == typeof(bool))
			{
				ob = bool.Parse(data);
			}
			else
			if (tp == typeof(String))
			{
				ob = data;
			}
			else
			if (tp == typeof(Guid))
			{
				//ob = Guid.Parse(data);
			}
			else
			if (tp == typeof(DateTime))
			{
				ob = DateTime.Parse(data);
			}
			else
			if (tp == typeof(System.Byte))
			{
				ob = System.Byte.Parse(data);
			}
			else
            if (tp == typeof(object))
            {
                ob = data;
            }			
			else
			{
				//return Convert.ChangeType(data, tp);
				Type [] t = tp.GetGenericArguments();
				if(t.Length == 1)
					ob = ConvertByType(data, t[0]);
			}
		    return ob;
		}

        public static object ConvertByType(object data, Type tp)
        {
            if (data == null | data.ToString().Equals(string.Empty))
                return null;
            return ConvertByType(data.ToString(), tp);
        }

		public static string ConvertByType(object data)
		{
			if (data == null)
				return null;
			Type tp = data.GetType();
			if (tp.IsEnum || tp.IsPrimitive
				|| tp == typeof(String) || tp == typeof(Guid) || tp == typeof(DateTime) || tp == typeof(System.Byte))
				return data.ToString();
			return null;
		}
	}

}