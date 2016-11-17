using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SV.ConversionTools
{
	/// <summary>
	/// класс различных коверторов
	/// </summary>
	/// <remarks>статический класс для различных конвертаций</remarks>
	public static class DataCoversion
	{
		/// <summary>
		/// функция преообразовывает обьект в массив байтов, преобразование происходит с помощью BinaryFormatter.Serialize
		/// </summary>
		/// <param name="ob">обьект для преобразования</param>
		/// <returns>возращает набор байт в который будет преобразовани переданный обьект через Serialize</returns>
		/// <remarks>для нормального преобразования обьект, передаваемый в функцию, должен иметь возможность сереализации</remarks>
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
		/// преобразование массива байт в обьект 
		/// </summary>
		/// <param name="bt">набор байть которые будут преобразованны в обьект через Deserialize</param>
		/// <returns>преобразованный обьект</returns>
		/// <remarks>для нормального преобразования обьект, содержащийся в наборе байт, должен был иметь возможность сереализации</remarks>
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
		/// удаленние не корректных символов для формирования пути к папкам и диррикотриям
		/// </summary>
		/// <param name="patch">входящая строка</param>
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
		/// функция конвертирования данные по типу
		/// </summary>
		/// <param name="data">данные</param>
		/// <param name="tp">тип</param>
		/// <returns>результат</returns>
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