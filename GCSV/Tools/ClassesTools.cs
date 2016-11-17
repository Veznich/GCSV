using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
//using System.Windows.Forms;
using System.Threading;

namespace SV.Tools
{
	public delegate void AddProp(string name, string val);
	/// <summary>
	/// структура для дублирующих параметров
	/// </summary>
	public struct MetodInfoDouble
	{
		public MethodInfo getMetod;
		public MethodInfo setMetod;
	}
	/// <summary>
	/// класс со статик функциями для работы с классами :( бля ну и описалово получилось
	/// </summary>
	public class ClassesTools
	{
		/// <summary>
		/// получения данных всех порперти класса включая вложения
		/// </summary>
		/// <param name="ob"></param>
		/// <param name="delegateProp"></param>
		/// <param name="parentName"></param>
		public static void GetAllPropertyData(object ob, AddProp delegateProp, string parentName = "")
		{
			if (ob == null || delegateProp == null)
				return;
			PropertyInfo[] propInfo = ob.GetType().GetProperties();
			for (int b = 0; b < propInfo.Length; b++)
			{
				ParameterInfo[] param = propInfo[b].GetIndexParameters();
				if (param.Length == 0 && propInfo[b].CanRead && propInfo[b].Name != "Root" && propInfo[b].Name != "Parent")
				{
					object data = propInfo[b].GetValue(ob, null);
					if (propInfo[b].PropertyType.IsInterface && data != null)
					{
							GetAllPropertyData(data, delegateProp, (parentName == "" ? "" : parentName + ".") + propInfo[b].Name);

					}
					else
					{
						delegateProp((parentName == "" ? "" : parentName + ".") + propInfo[b].Name,
							( data == null ? "NULL" : SV.ConversionTools.DataCoversion.ConvertByType(data)));
					}
				}
			}
		}

		static AppDomain domain = Thread.GetDomain();
		static Assembly[] asm = domain.GetAssemblies();
		/// <summary>
		/// поиск интерфеса по имени
		/// </summary>
		/// <param name="name">имя интерфейса</param>
		/// <returns></returns>
		public static Type FindInterfece(string Namespace, string Name, bool isIgnorCase = true)
		{
			if (Namespace == "" || Name == "")
				return null;
			
			int count = asm.Length;
			for (int a = 0; a < count; a++)
			{
				Type t = asm[a].GetType(Namespace+"."+Name);
				if (t != null)
					return t;
			}
			return null;
		}
		public static List<Type> GetAsseblyIntrface(string AssembleName, string Namespace)
		{
			if (Namespace == "" )
				return null;
			int count = asm.Length;
			for (int a = 0; a < count; a++)
			{
				if (asm[a].GetName().Name == AssembleName)
				{
					Type[] t = asm[a].GetTypes();
					List<Type> lst = new List<Type>();
					count = t.Length;
					for(int b =0; b < count; b++)
					{
						if (t[b].Namespace == Namespace)
							lst.Add(t[b]);
					}
					return lst;
				}				
			}
			return null;
		}
		/// <summary>
		/// находит все нтерфейсы, включая вложенные, удаляет дубликаты
		/// </summary>
		/// <param name="tp"></param>
		/// <param name="listInterfece"></param>
		public static void GetAllInterfece(Type[] tp, ref List<Type> listInterfece)
		{
			if (tp == null)
				return;
			int count = tp.Length;
			for (int a = 0; a < count; a++)
			{
				Type rezult = listInterfece.Find(
					delegate(Type typ)
					{
						return tp[a] == typ;
					}
					);
				if (rezult == null)
					listInterfece.Add(tp[a]);
				Type[] t = tp[a].GetInterfaces();
				GetAllInterfece(t, ref listInterfece);
			}
		}
		/// <summary>
		/// находит все нтерфейсы, включая вложенные
		/// </summary>
		/// <param name="parentClass"></param>
		/// <returns></returns>
		public static List<Type> GetAllInterfece(Type parentClass)
		{
			List<Type> listClasses = new List<Type>();
			GetAllInterfece(new Type[] { parentClass }, ref listClasses);
			return listClasses;
		}
		/// <summary>
		/// находит все нтерфейсы, включая вложенные, удаляет дубликаты
		/// </summary>
		/// <param name="parentClass"></param>
		/// <param name="listInterfece"></param>
		/// <returns></returns>
		public static List<Type> GetAllInterfece(Type parentClass, List<Type> listInterfece)
		{
			List<Type> listClasses = new List<Type>();
			GetAllInterfece(new Type[] { parentClass }, ref listClasses);
			GetAllInterfece(listInterfece.ToArray(), ref listInterfece);
			return RemoveDouble(listClasses, listInterfece);
		}
		/// <summary>
		/// удаляет дубликаты в списках
		/// </summary>
		/// <param name="sources">источник</param>
		/// <param name="editableList">список в котором уберуться встречающиеся значения в sources</param>
		/// <returns>возращаемое значение</returns>
		public static List<Type> RemoveDouble(List<Type> sources, List<Type> editableList)
		{
			for (int a = editableList.Count - 1; a >= 0; a--)
			{
				if (sources.Find((Type t) => editableList[a] == t) != null)
					editableList.RemoveAt(a);
			}
			return editableList;
		}
		/// <summary>
		/// поиск параметра во всех интерфейсах типа
		/// </summary>
		/// <param name="_class">тип класса</param>
		/// <param name="propertyName">имя параметра</param>
		/// <returns>найденный параметр</returns>
		/// <remarks>
		/// тестовая функция
		/// </remarks>
		public static PropertyInfo FindProperty(Type _class, string propertyName)
		{
			List<PropertyInfo> allProperty = GetAllProperty(_class);
			int count = allProperty.Count;
			PropertyInfo info = null;
			for (int a = 0; a < count; a++)
			{
				if (allProperty[a].Name == propertyName)
				{
					info = allProperty[a];
					break;
				}
			}

			return info;
		}
		public static List<Type> RemoveDouble(List<Type> property)
		{
			List<Type> retryList = new List<Type>();
			int count = property.Count;
			for (int a = 0; a < count; a++)
			{
				if (retryList.Find((Type inf) => property[a] == inf) == null)
					retryList.Add(property[a]);

			}
			return retryList;
		}

		public static List<PropertyInfo> RemoveDouble(List<PropertyInfo> property)
		{
			List<PropertyInfo> retryList = new List<PropertyInfo>();
			int count = property.Count;
			for (int a = 0; a < count; a++)
			{
				if(retryList.Find( (PropertyInfo inf) =>property[a] == inf	) == null)
					retryList.Add(property[a]);

			}
			return retryList;
		}
		/// <summary>
		/// получает все параметры по типу, с удалением дублей
		/// </summary>
		/// <param name="interfeceList">родидельский тип</param>
		/// <returns>список параметров</returns>
		/// <remarks>
		/// тестовая функция
		/// </remarks>
		public static List<PropertyInfo> GetAllProperty(Type parent)
		{
			List<Type> allTypes = GetAllInterfece(parent);
			List<PropertyInfo> allProperty = new List<PropertyInfo>();
			if (allTypes != null)
			{
				int count = allTypes.Count;
				for(int a =0; a < count; a++)
				{
					allProperty.AddRange(allTypes[a].GetProperties(/*BindingFlags.Default*/));
				}
			}
			return RemoveDouble(allProperty);
		}
		/// <summary>
		/// поиск параметра по имени
		/// </summary>
		/// <param name="name">имя параметра</param>
		/// <returns></returns>
		public static PropertyInfo GetPropertyByName(object curr, string name, ref object objectIsCall)
		{
			if (curr == null)
				return null;
			PropertyInfo pI = curr.GetType().GetProperty(name);
			objectIsCall = curr;
			if (pI == null)
			{

				int t = name.IndexOf('.');
				if (t > 0)
				{
					string curName = name.Substring(0, t);
					pI = curr.GetType().GetProperty(curName);
					if (pI != null)
					{
						name = name.Remove(0, t + 1);
						if (name.Length > 0)
						{
							object v = pI.GetValue(curr, null);
							if (v == null)
								return null;
							return GetPropertyByName(v, name, ref objectIsCall);
						}
					}

				}
			}
			return pI;
		}
		/// <summary>
		/// формирует списки метаданных на основании переданных типов
		/// </summary>
		/// <param name="interfeceList">список типов интерфейса\класса</param>
		/// <param name="propertyInfo">список для заполнения</param>
		/// <param name="memberInfo">список для заполнения</param>
		/// <param name="fieldInfo">список для заполнения</param>
		/// <param name="metodInfo">список для заполнения</param>
		/// <param name="eventInfo">список для заполнения</param>
		public static void GetInterfaceMetadata(List<Type> interfeceList, ref List<PropertyInfo> propertyInfo,
			ref List<MemberInfo> memberInfo, ref List<FieldInfo> fieldInfo, ref List<MethodInfo> metodInfo, ref List<EventInfo> eventInfo)
		{
			int count = interfeceList.Count;
			for (int a = 0; a < count; a++)
			{
				//////////////////////////////////////////////////////////////////////////
				//базовые евенты и проперти
				PropertyInfo[] propertyIE = interfeceList[a].GetProperties();
				propertyInfo.AddRange(propertyIE);
				EventInfo[] events = interfeceList[a].GetEvents();
				eventInfo.AddRange(events);
				MemberInfo[] membersIE = interfeceList[a].GetMembers();
				memberInfo.AddRange(membersIE);
				FieldInfo[] fieldIE = interfeceList[a].GetFields();
				fieldInfo.AddRange(fieldIE);
				MethodInfo[] metodIE = interfeceList[a].GetMethods();
				metodInfo.AddRange(metodIE);
			}

		}
		/// <summary>
		/// функция нахождения пвоторяющихся  пропертей
		/// </summary>
		/// <param name="propertyInfoInterface"></param>
		/// <returns></returns>
		public static Dictionary<string, MetodInfoDouble> RemoveDoubleProperty(List<PropertyInfo> propertyInfoInterface)
		{
			if (propertyInfoInterface == null)
				return null;
			Dictionary<string, MetodInfoDouble> m_doubleList = new Dictionary<string, MetodInfoDouble>();
			int count = propertyInfoInterface.Count - 1;
			for (int a = count; a >= 0; a--)
			{
				List<PropertyInfo> fnd = propertyInfoInterface.FindAll(
					(PropertyInfo inf) => inf.Name == propertyInfoInterface[a].Name);

				PropertyInfo fullMetod = null;
				MetodInfoDouble mDouble = new MetodInfoDouble();
				mDouble.getMetod = null;
				mDouble.setMetod = null;

				if (fnd != null && fnd.Count > 1)
				{
					string tmp = "";
					for (int b = 0; b < fnd.Count; b++)
					{
						tmp += fnd[b].ReflectedType.FullName + "\r\n";
						propertyInfoInterface.Remove(fnd[b]);
						if (fnd[b].CanRead && fnd[b].CanWrite)
							fullMetod = fnd[b];
						else if (fnd[b].CanRead)
							mDouble.getMetod = fnd[b].GetGetMethod();
						else if (fnd[b].CanWrite)
							mDouble.setMetod = fnd[b].GetSetMethod();
					}
#if DEBUG
					//MessageBox.Show("DEBUG:\r\nПовторяющийся параметр с именем: " + fnd[0].Name + "\r\nВ интерфейсах:\r\n" + tmp);
#endif

					if (fullMetod != null)
						propertyInfoInterface.Add(fullMetod);
					else
					{
						m_doubleList.Add(fnd[0].Name, mDouble);
						propertyInfoInterface.Add(fnd[0]);
					}
				}

			}

			return m_doubleList;
		}
		public static bool IsPrimitive(Type t)
		{
			if (t == null)
				return true;
			if (!t.IsClass && !t.IsInterface || t.IsPrimitive || t.IsEnum || t == typeof(String) || t == typeof(Guid) || t == typeof(DateTime))
				return true;

			return false;
		}
		/// <summary>
		/// функция получения данных и параметра по имени параметра
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>данные, могут быть null если не найденн параметр или он нулевой</returns>
		static public object GetData(object baseCalss,string propertyName, object[] param = null)
		{
			if (baseCalss == null || propertyName == null)
				return null;
			object RecalcOb = null;
			PropertyInfo _PropertyDescriptor = SV.Tools.ClassesTools.GetPropertyByName(baseCalss, propertyName.ToString(), ref RecalcOb);
			object v = null;
			if (_PropertyDescriptor != null)
				v = _PropertyDescriptor.GetValue((RecalcOb == null ? baseCalss : RecalcOb), param);
			return v;
		}
		/// <summary>
		/// установка данных в параметр по имени
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		/// <param name="newPropertyData">новое значение</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>false - если параметр небыл найден</returns>
		static public bool SetData(object baseCalss, string propertyName, object newPropertyData, object[] param = null)
		{
			if (baseCalss == null || propertyName == null)
				return false;
			object RecalcOb = null;
			PropertyInfo _PropertyDescriptor = SV.Tools.ClassesTools.GetPropertyByName(baseCalss, propertyName, ref RecalcOb);
			if (_PropertyDescriptor == null)
				return false;

			object data = newPropertyData;
			if (newPropertyData != null && newPropertyData.GetType() != _PropertyDescriptor.PropertyType)
				data = SV.ConversionTools.DataCoversion.ConvertByType(data.ToString(), _PropertyDescriptor.PropertyType);
			_PropertyDescriptor.SetValue((RecalcOb == null ? baseCalss : RecalcOb), data, param);
			return true;
		}


	}
}
