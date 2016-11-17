using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
//using System.Xml.Serialization;
using System.Xml;


namespace SV.Tools
{
#region NewBuilder
	/// <summary>
	/// строковые константы
	/// </summary>
	public static class StaticNamesAndType
	{		/// <summary>
		/// начальный заголовок вновь генерируемого типа
		/// </summary>
		public static string ClassNameTitle = "SVTypeCreator_";
		public static string ModuleExt = ".lib";
		public static string ModuleName = "DataSome";//имя модуля и имя сборки должно совпадать а также нужно расширение dl для нормальной десериализации
		public static string AssemblyName = "SVDynamicLibrary";//"SVAssembleCreator";
	}

	#region TypeGenerate
	public sealed class TypeGenerator : System.Runtime.Serialization.SerializationBinder
	{
		#region Fields
		DomainCreator m_domain;
		public DomainCreator DomainCreator
		{
			get { return m_domain; }
		}
		AssembleCreator m_assebly;
		public AssembleCreator AssembleCreator
		{
			get { return m_assebly; }
		}
		ModuleCreator m_module;
		public ModuleCreator ModuleCreator
		{
			get { return m_module; }
		}
		List<TypeCreator> m_listType = new List<TypeCreator>();
		#endregion

		public TypeGenerator(string moduleName = "", string ext = "", string FoldferLib = "", string nameAssemble = "", AssemblyBuilderAccess acess = AssemblyBuilderAccess.RunAndSave)
		{
			m_domain = new DomainCreator(FoldferLib);
			m_assebly = new AssembleCreator(m_domain);
			m_module = new ModuleCreator(m_assebly, moduleName, ext);
		}

		public Type GetType(Type baseClass, params Type[] listIntrface)
		{
			TypeCreator fCr = FindTypeCreator(baseClass, listIntrface);
			if (fCr != null)
				return fCr.TypeClass;
			fCr = new TypeCreator(this, baseClass, listIntrface);
			Type t = fCr.CreateType();
			if (t != null)
				m_listType.Add(fCr);
			return t;
		}
		public Type GetType(string className)
		{
			TypeCreator cr = m_listType.Find((TypeCreator tC) => tC.ClassName == className);
			if (cr != null)
				return cr.TypeClass;
			return null;
		}
		public object GetInstance(Type baseClass, params Type[] listIntrface)
		{
			TypeCreator fCr = FindTypeCreator(baseClass, listIntrface);
			if (fCr != null)
				return fCr.CreateInstance();
			else if (CreateType(baseClass, listIntrface))
			{
				return GetInstance(baseClass, listIntrface);
			}
			return null;
		}
		public object GetInstance(string className)
		{
			TypeCreator cr = m_listType.Find((TypeCreator tC) => tC.ClassName == className);
			if (cr != null)
				return cr.CreateInstance();
			return null;
		}
		public object GetInstance(Type inst)
		{
			TypeCreator cr = m_listType.Find((TypeCreator tC) => tC.TypeClass == inst);
			if (cr != null)
				return cr.CreateInstance();
			return null;
		}
		public bool CreateType(Type baseClass, params Type[] listIntrface)
		{
			return (GetType(baseClass, listIntrface) != null);
		}
		TypeCreator FindTypeCreator(Type baseClass, params Type[] listIntrface)
		{
			TypeCreator cr = new TypeCreator(this, baseClass, listIntrface);
			return m_listType.Find((TypeCreator tC) => tC.ClassName == cr.ClassName);
		}		
		public bool SaveLib()
		{
			if (m_module == null || !m_module.IsInit)
				return false;
			return m_module.Save();
		}
		public bool SaveLibAs(string nameLib)
		{
			if (m_module == null || !m_module.IsInit)
				return false;
			return m_module.SaveAs(nameLib);
		}
		/// <summary>
		/// функция позволяющая использовать сериализацию с текущими типами
		/// </summary>
		/// <param name="assemblyName">имя сборки, пофиг</param>
		/// <param name="typeName">имя класса</param>
		/// <example>
		/// Stream TestFileStream = File.OpenRead(m_fileName);
		///	BinaryFormatter deserializer = new BinaryFormatter();
		///	deserializer.Binder = this;
		///	estimate = deserializer.Deserialize(TestFileStream);
		/// </example>
		/// <returns></returns>
		public override Type BindToType(string assemblyName, string typeName)
		{
			return GetType(typeName);
		}

	}
	#endregion
	#region DomainCreator
	public sealed class DomainCreator
	{
		#region FIELDS
		protected AppDomain m_Domain;
		public AppDomain Domain
		{
			get { return m_Domain; }
		}
		string m_localDir;
		public string DynamicDirectory
		{
			get { if (m_localDir != "") return m_localDir; return (m_Domain != null ? m_Domain.DynamicDirectory : ""); }
		}
		#endregion
		public DomainCreator(string FolderLib = "")
		{
			m_Domain = Thread.GetDomain();
			m_localDir = FolderLib;
			if (m_localDir != "")
			{
				if (!System.IO.Directory.Exists(m_localDir))
				{
					System.IO.Directory.CreateDirectory(m_localDir);
				}
			}
		}

	}
	#endregion

	#region AssembleCreator
	public sealed class AssembleCreator
	{
		#region FIELDS
		protected AssemblyName m_AssemblyName;
		protected AssemblyBuilder m_AsmBuilder;
		public AssemblyBuilder AssemblyBuilder
		{
			get { return m_AsmBuilder; }
		}
		public string FullNameModule
		{
			get
			{
				if (m_AssemblyName == null) return "";
				return m_AssemblyName.Name;
			}
		}
		bool isInit = false;
		public bool IsInit
		{
			get { return isInit; }
		}
		DomainCreator m_domain;
		public DomainCreator Domain
		{
			get { return m_domain; }
			set { m_domain = value; }
		}
		#endregion
		public AssembleCreator(DomainCreator domain, string nameAssemble, AssemblyBuilderAccess acess = AssemblyBuilderAccess.RunAndSave)
		{
			m_domain = domain;
			m_AssemblyName = new AssemblyName((nameAssemble == "" ? StaticNamesAndType.AssemblyName : nameAssemble));
			m_AsmBuilder = m_domain.Domain.DefineDynamicAssembly(m_AssemblyName, acess, m_domain.DynamicDirectory, true, null);
			if (m_AsmBuilder != null)
				isInit = true;
		}
		public AssembleCreator(DomainCreator domain, AssemblyBuilderAccess acess = AssemblyBuilderAccess.RunAndSave)
			: this(domain, StaticNamesAndType.AssemblyName, acess)
		{

		}

	}
	#endregion

	#region ModuleCreator
	public sealed class ModuleCreator : ErrorTracerpt
	{
		#region FIELDS
		string m_moduleName;
		public string ModuleName
		{
			get { return m_moduleName + EXT; }
		}
		string m_ext;
		public string EXT
		{
			get { return m_ext; }
		}
		bool m_isInit = false;
		public bool IsInit
		{
			get { return m_isInit; }
		}
		ModuleBuilder m_moduleBuilder;
		public ModuleBuilder ModuleBuilder
		{
			get { return m_moduleBuilder; }
		}
		AssembleCreator m_AssemblyBuilder;
		public AssembleCreator AssemblyBuilder
		{
			get { return m_AssemblyBuilder; }
			set { m_AssemblyBuilder = value; }
		}
		#endregion
		public ModuleCreator(AssembleCreator asmBuilder, string moduleName, string ext)
		{
			m_AssemblyBuilder = asmBuilder;
			m_ext = (ext == "" ? StaticNamesAndType.ModuleExt : ext);
			m_moduleName = (moduleName == "" ? StaticNamesAndType.ModuleName : moduleName);
			GenerateModule();
		}
		public ModuleCreator(AssembleCreator asmBuilder)
			: this(asmBuilder, StaticNamesAndType.ModuleName, StaticNamesAndType.ModuleExt)
		{
		}
		public bool GenerateModule()
		{
			if (m_isInit)
				return m_isInit;
			if (m_AssemblyBuilder == null || m_moduleName == "" || m_ext == "")
				return m_isInit;
			m_moduleBuilder = m_AssemblyBuilder.AssemblyBuilder.DefineDynamicModule(m_moduleName, m_moduleName + m_ext);
			m_isInit = true;
			return true;
		}
		public bool Save()
		{
			return SaveAs(ModuleName);
		}
		public bool SaveAs(string nameModule)
		{
			try
			{
				if (m_AssemblyBuilder != null)
					m_AssemblyBuilder.AssemblyBuilder.Save(nameModule);
			}
			catch (System.Exception ex)
			{
				Error("ModuleCreator:SaveLibrary", ex, this, true);
				return false;
			}
			return true;
		}
	}
	#endregion

	#region TypeCreator
	struct DataRedirect 
	{
		public object Set;
		public object Get;
	}
	/// <summary>
	/// генерато типа
	/// </summary>
	public sealed class TypeCreator : ErrorTracerpt
	{
		#region FIELDS
		MethodAttributes m_getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual |
				MethodAttributes.Final; 
		bool m_IsSerializable = true;
		public bool IsSerializable
		{
			get { return m_IsSerializable; }
			set { m_IsSerializable = value; }
		}
		/// <summary>
		/// имя класса
		/// </summary>
		string m_className = "";
		/// <summary>
		/// имя классса
		/// </summary>
		public string ClassName
		{
			get { return m_className; }
		}
		/// <summary>
		/// тип базового класса
		/// </summary>
		Type m_baseClass;
		/// <summary>
		/// список базовых интерфейсоф
		/// </summary>
		List<Type> m_IntrfaceList = new List<Type>();
		/// <summary>
		/// буилдер типа
		/// </summary>
		TypeBuilder m_TypeBuilder;
		/// <summary>
		/// тип сгенерированного класса
		/// </summary>
		Type m_generateClass;
		/// <summary>
		/// тип сгенерированного класса
		/// </summary>
		public Type TypeClass
		{
			get { return m_generateClass; }
		}
		/// <summary>
		/// список переменных класса с возможностью автоинициализации
		/// </summary>
		Dictionary<FieldInfo, Type> m_listFieldAutoInitialise = new Dictionary<FieldInfo, Type>();
		/// <summary>
		/// список всех параметров
		/// </summary>
		List<PropertyInfo> m_propertyInfoInterface = new List<PropertyInfo>();
		/// <summary>
		/// спсисок всех членов
		/// </summary>
		List<MemberInfo> m_memberInfoInterface = new List<MemberInfo>();
		/// <summary>
		/// список всех полей
		/// </summary>
		List<FieldInfo> m_fieldInfoInterface = new List<FieldInfo>();
		/// <summary>
		/// список всех методов
		/// </summary>
		List<MethodInfo> m_metodInfoInterface = new List<MethodInfo>();
		/// <summary>
		/// список всех евентов
		/// </summary>
		List<EventInfo> m_eventInfoInerface = new List<EventInfo>();
		/// <summary>
		/// список метов-дублей, с методами гет и сет
		/// </summary>
		Dictionary<string, MetodInfoDouble> m_doubleList = null;
		TypeGenerator m_TypeGenerator;
		#endregion
		public TypeCreator(TypeGenerator typegenerator, Type baseCalss, params Type[] InterfaceList)
		{
			m_TypeGenerator = typegenerator;
			m_baseClass = baseCalss;
			if(InterfaceList != null)
				m_IntrfaceList.AddRange(InterfaceList);
			m_IntrfaceList = ClassesTools.GetAllInterfece(m_baseClass, m_IntrfaceList);
			m_className = GenerateClassName(m_baseClass, m_IntrfaceList);
		}

		#region InitFuction
		public object CreateInstance()
		{
			if (m_generateClass == null)
				return null;
			return Activator.CreateInstance(m_generateClass, true);
		}
		public Type CreateType()
		{
			if (m_generateClass != null)
				return m_generateClass;

			if (m_baseClass == null || m_TypeGenerator == null)
				return null;
			m_TypeBuilder = m_TypeGenerator.ModuleCreator.ModuleBuilder.DefineType(m_className, TypeAttributes.Public, m_baseClass, m_IntrfaceList.ToArray());
			//////////////////////////////////////////////////////////////////////////
			//атрибуты для сериализации
			if (m_IsSerializable)
			{
				ConstructorInfo classCtorInfo = typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes);
				CustomAttributeBuilder myCABuilder = new CustomAttributeBuilder(
									classCtorInfo, new object[] { });
				m_TypeBuilder.SetCustomAttribute(myCABuilder);
			}

			ClassesTools.GetInterfaceMetadata(m_IntrfaceList, ref m_propertyInfoInterface, ref m_memberInfoInterface,
							ref m_fieldInfoInterface, ref m_metodInfoInterface, ref m_eventInfoInerface);
			m_doubleList = RemoveDoubleProperty(m_propertyInfoInterface);
			if (!CreateEvent())
				return null;
			//////////////////////////////////////////////////////////////////////////
			if (!CreateProperty())
				return null;
			if (!CreateConstructor())
				return null;
			try
			{
				m_generateClass = m_TypeBuilder.CreateType();
			}
			catch (System.Exception ex)
			{
				Error("TypeCreator:CreateType", ex);
			}

			return m_generateClass;
		}
		/// <summary>
		/// проверка на тип аттрибутов
		/// </summary>
		/// <param name="eTypeAtr">тип атрибутов</param>
		/// <param name="attrs">список атрибутов в классе\свойстве</param>
		/// <returns>полученный класс, если атрибутов такого типа нет будет нулем</returns>
		static public BuilderClassesPropertyAttribyte IsAttribute(enumBuilderClassessAttribute eTypeAtr, object[] attrs)
		{
			int count = attrs.Length;
			for (int a = 0; a < count; a++)
			{
				BuilderClassesPropertyAttribyte ePatr = attrs[a] as BuilderClassesPropertyAttribyte;
				if (ePatr != null && ePatr.MoreAttribyte == eTypeAtr)
					return ePatr;
			}
			return null;
		}
		/// <summary>
		/// функция сравнения
		/// </summary>
		/// <param name="x">строка</param>
		/// <param name="y">строка</param>
		/// <returns>результат</returns>
		private static int CompareByLength(Type tX, Type tY)
		{
			if (tX == null)
				return (tY == null ? 0 : -1);
			if (tY == null)
				return 1;
			int retval = tX.Name.Length.CompareTo(tY.Name.Length);
			if (retval != 0)
				return retval;
			return tX.Name.CompareTo(tY.Name);
		}
		string GenerateClassName(Type baseClasses, List<Type> interfecesAdded)
		{
			if (baseClasses == null || interfecesAdded == null)
				return "";
			interfecesAdded.Sort(CompareByLength);
			string temp = StaticNamesAndType.ClassNameTitle + baseClasses.Name;
			int count = interfecesAdded.Count;
			if (count > 0)
				temp += "_" + interfecesAdded[0].Name;
			for (int a = 1; a < count; a++)
			{
				int pos = 0;
				if (interfecesAdded[a].Name[pos] == 'i' || interfecesAdded[a].Name[pos] == 'I')
					pos = 1;
				temp += "_" + interfecesAdded[a].Name[pos] + interfecesAdded[a].Name.Length.ToString();
			}
			return temp;
		}
		#endregion

		#region CreatingTypeClass
		/// <summary>
		/// создаем конструктор нашего класса
		/// </summary>
		/// <returns></returns>
		bool CreateConstructor()
		{
			if (m_baseClass == null || m_TypeBuilder == null)
				return false;
			//////////////////////////////////////////////////////////////////////////
			// Добавление кода для конструктора.
			try
			{
				ConstructorInfo objCtor = m_baseClass.GetConstructor(new Type[0]);
				ConstructorBuilder pointCtor = m_TypeBuilder.DefineConstructor(
							   MethodAttributes.Public,
							   CallingConventions.Standard,
							   null);
				ILGenerator ctorIL = pointCtor.GetILGenerator();
				foreach (KeyValuePair<FieldInfo, Type> pair in m_listFieldAutoInitialise)
				{
					FieldInfo flInf = pair.Key;
					Type t = pair.Value;
					ConstructorInfo cinf = t.GetConstructor(Type.EmptyTypes);
					ctorIL.Emit(OpCodes.Ldarg_0);
					ctorIL.Emit(OpCodes.Newobj, cinf);
					ctorIL.Emit(OpCodes.Stfld, flInf);//stfld      class TestThreatPool.c2 TestThreatPool.c3::mmI

					PropertyInfo inf = t.GetProperty("_propertyNameThisClass");
					if (inf != null)
					{
						ctorIL.Emit(OpCodes.Ldarg_0);
						ctorIL.Emit(OpCodes.Ldfld, flInf);
						ctorIL.Emit(OpCodes.Ldstr, flInf.Name.Replace("m_", ""));
						ctorIL.Emit(OpCodes.Callvirt, inf.GetSetMethod());
					}

					inf = t.GetProperty("DelegateCalling");

					if (inf != null)
					{
						MethodInfo mf = m_baseClass.GetMethod("CallPropertyChange");
						if (mf != null)
						{
							ctorIL.Emit(OpCodes.Ldarg_0);
							ctorIL.Emit(OpCodes.Ldfld, flInf);

							ctorIL.Emit(OpCodes.Ldarg_0);
							ctorIL.Emit(OpCodes.Ldftn, mf);
							ConstructorInfo cI = typeof(CallPropertyChange).GetConstructor(new Type[] { typeof(System.Object), typeof(IntPtr) });
							ctorIL.Emit(OpCodes.Newobj, cI);
							ctorIL.Emit(OpCodes.Callvirt, inf.GetSetMethod());
						}
					}


				}
				ctorIL.Emit(OpCodes.Ldarg_0);
				ctorIL.Emit(OpCodes.Call, objCtor);
				ctorIL.Emit(OpCodes.Ret);

			}
			catch (System.Exception ex)
			{
				Error("BuilderClasses - CreateConstructor", ex);
				return false;
			}
			return true;
			//////////////////////////////////////////////////////////////////////////
		}

		void CreatePropertyGetMetod(PropertyInfo property, PropertyBuilder custNamePropBldr, FieldBuilder fieldProperty, object redirectData)
		{
			#region GET_METOD
			//находим метот гет для проперти
			MethodInfo inf = property.GetGetMethod();
			//если такогого нет ищем в дублированных
			if (inf == null)
			{
				if (m_doubleList.ContainsKey(property.Name))
					inf = m_doubleList[property.Name].getMetod;
			}
			//если метод найден то начинаем его делать
			if (inf != null)
			{
				//создаем построитель для метота
				MethodBuilder custNameGetPropMthdBldr =
						m_TypeBuilder.DefineMethod("get_" + property.Name,
												m_getSetAttr,
												  property.PropertyType,
												   Type.EmptyTypes);
				//создаем генерато ИЛ
				ILGenerator custNameGetIL = custNameGetPropMthdBldr.GetILGenerator();
				System.Reflection.Emit.Label end = custNameGetIL.DefineLabel();
				//начинаем формировать асмокод
				custNameGetIL.Emit(OpCodes.Nop);
				custNameGetIL.Emit(OpCodes.Ldarg_0);
#region GET_REDIRECT
				if (redirectData != null)
				{				
					//////////////////////////////////////////////////////////////////////////
					//делаем метод
					MethodInfo mti = redirectData as MethodInfo;					
					if(mti == null)
					{
						PropertyInfo pi = redirectData as PropertyInfo;
						if (pi != null)
							mti = pi.GetGetMethod();
					}

					if (mti != null)
					{
						if(mti.IsGenericMethodDefinition)
						{
							Type[] types = property.PropertyType.GetGenericArguments();
							if(types.Length > 0)
								mti = mti.MakeGenericMethod(types);
							else
							{
								mti = mti.MakeGenericMethod(new Type[] { property.PropertyType });
							}
						}
						custNameGetIL.Emit(OpCodes.Call, mti);
					}

				}
#endregion
				else//если не редиректная функция
				{
					//возвращаем локальную переменную
					custNameGetIL.Emit(OpCodes.Ldfld, fieldProperty);
				}
				//выход из проперти

				custNameGetIL.Emit(OpCodes.Ret);
				//перезаписываем метод по умолчанию
				m_TypeBuilder.DefineMethodOverride(custNameGetPropMthdBldr, inf);
				//устанавливаем этот метод
				custNamePropBldr.SetGetMethod(custNameGetPropMthdBldr);
			}
			//конец создания ГЕТ метода 
			//////////////////////////////////////////////////////////////////////////
			#endregion
		}
		void CreatePropertySetMetod(PropertyInfo property, PropertyBuilder custNamePropBldr, FieldBuilder fieldProperty, object redirectData)
		{
			#region SET_METOD
			//находим сет метод
			MethodInfo inf = property.GetSetMethod();
			//если нет то ищем в дублях
			if (inf == null)
			{
				if (m_doubleList != null && m_doubleList.ContainsKey(property.Name))
					inf = m_doubleList[property.Name].setMetod;
			}
			if (inf != null)
			{
				MethodBuilder custNameSetPropMthdBldr =
					m_TypeBuilder.DefineMethod("set_" + property.Name,
											   m_getSetAttr,
											   null,
											   new Type[] { property.PropertyType });
				ILGenerator custNameSetIL = custNameSetPropMthdBldr.GetILGenerator();
				//System.Reflection.Emit.Label retLbl = custNameSetIL.DefineLabel();
				//создаем локальную переменную

				custNameSetIL.Emit(OpCodes.Ldarg_0);			
				if (fieldProperty != null)
				{
					LocalBuilder loc = custNameSetIL.DeclareLocal(property.PropertyType);
					custNameSetIL.Emit(OpCodes.Ldfld, fieldProperty);
					custNameSetIL.Emit(OpCodes.Stloc_0);
					
					custNameSetIL.Emit(OpCodes.Ldarg_0);
					custNameSetIL.Emit(OpCodes.Ldarg_1);
					//присваем значение переменной класса								
					custNameSetIL.Emit(OpCodes.Stfld, fieldProperty);
					if (m_baseClass.GetInterface("iMatryoshkaCall") != null)
					{
						MethodInfo simpleShow = typeof(iMatryoshkaCall).GetMethod("CallPropertyChange");
						//CallPropertyChange(string propertyName, object CommandID = null, object oldData = null, object newData = null)
						if (simpleShow != null)
						{
							custNameSetIL.Emit(OpCodes.Ldarg_0);

							custNameSetIL.Emit(OpCodes.Ldstr, property.Name);
														
							custNameSetIL.Emit(OpCodes.Ldc_I4_0);
							custNameSetIL.Emit(OpCodes.Box, typeof(int));

							custNameSetIL.Emit(OpCodes.Ldloc_0);
							custNameSetIL.Emit(OpCodes.Box, property.PropertyType);

							custNameSetIL.Emit(OpCodes.Ldarg_0);
							custNameSetIL.Emit(OpCodes.Ldfld, fieldProperty);
							custNameSetIL.Emit(OpCodes.Box, property.PropertyType);						
							
							custNameSetIL.Emit(OpCodes.Callvirt, simpleShow);
						}
					}

					
					 //////////////////////////////////////////////////////////////////////////			
				}
				#region REDIRECR
				//////////////////////////////////////////////////////////////////////////
				//если редирект
				if (redirectData != null)
				{
					//////////////////////////////////////////////////////////////////////////
					//делаем метод
					MethodInfo mti = redirectData as MethodInfo;
					if (mti == null)
					{
						PropertyInfo pi = redirectData as PropertyInfo;
						if (pi != null)
							mti = pi.GetSetMethod();
					}
					if (mti != null)
					{
						custNameSetIL.Emit(OpCodes.Ldarg_1);	
						custNameSetIL.Emit(OpCodes.Call, mti);
						if (!ClassesTools.IsPrimitive(property.PropertyType))
							custNameSetIL.Emit(OpCodes.Isinst, inf.ReturnType);
					}
				}
				#endregion
				#region MATRECHKA_REDIRECT_DELEGATE
				else
					//////////////////////////////////////////////////////////////////////////
					//вставить прилинковывание делегато в случае если это матрешка

					if (property.PropertyType.IsInterface && !property.PropertyType.IsGenericType)
					{
						object[] attrBs = property.PropertyType.GetCustomAttributes(true);//System.Attribute.GetCustomAttributes(property.PropertyType);
						//bool isCreate = (IsAttribute(enumBuilderClassessAttribute.AutoGenerate, attrBs) == null ? false : true);
						if (IsAttribute(enumBuilderClassessAttribute.AutoGenerate, attrBs) != null)
						{
							MethodInfo metodDelegate = typeof(iMatryoshkaCall).GetMethod("CallPropertyChange");
							ConstructorInfo delegateConstructor =
								typeof(CallPropertyChange).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
							PropertyInfo pr = typeof(iMatryoshkaCall).GetProperty("DelegateCalling");
							MethodInfo prDel = pr.GetSetMethod();

							custNameSetIL.Emit(OpCodes.Ldarg_0);
							custNameSetIL.Emit(OpCodes.Ldfld, fieldProperty);
							custNameSetIL.Emit(OpCodes.Ldarg_0);
							custNameSetIL.Emit(OpCodes.Ldftn, metodDelegate);
							custNameSetIL.Emit(OpCodes.Newobj, delegateConstructor);

							custNameSetIL.Emit(OpCodes.Callvirt, prDel);
						}

					}
					#endregion
				
				
				custNameSetIL.Emit(OpCodes.Ret);
				custNamePropBldr.SetSetMethod(custNameSetPropMthdBldr);
				m_TypeBuilder.DefineMethodOverride(custNameSetPropMthdBldr, inf);
			}
			#endregion
		}
		/// <summary>
		/// создание прамаетров
		/// </summary>
		/// <returns>результат</returns>
		bool CreateProperty()
		{
			try
			{
				//создание пропертес
				int count = m_propertyInfoInterface.Count;
				for (int a = 0; a < count; a++)
					if (!BuildProperty(m_propertyInfoInterface[a]))
						return false;
			}
			catch (System.Exception ex)
			{
				Error("BuilderClasses - CreateProperty", ex);
				return false;
			}
			return true;
		}
		object FindRedirecData(string NameRedirect)
		{
			object redirectData = null;
			redirectData = m_baseClass.GetProperty(NameRedirect, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
				 BindingFlags.Instance);
			//если не нашли ищем в интерфейсах
			if (redirectData == null)
				redirectData = m_propertyInfoInterface.Find((PropertyInfo sInf) => sInf.Name == NameRedirect);
			//ищем функцию в базовом классе
			if (redirectData == null)
			{
				//MethodInfo[] mi = m_baseClass.GetMethods();
				redirectData = m_baseClass.GetMethod(NameRedirect, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
				 BindingFlags.Instance);

			}			
			return redirectData;
		}
		DataRedirect FindRedirectAttribyte(PropertyInfo property )
		{
			//получаем расширенные аттрибуты
				object[] attrs = property.GetCustomAttributes(true);
				DataRedirect data = new DataRedirect();	
				//определяем являеться ли проперти редиректной
				BuilderClassesPropertyAttribyte redirectAttributeGet = IsAttribute(enumBuilderClassessAttribute.Redirect, attrs);
				BuilderClassesPropertyAttribyte redirectAttributeSet = null;
				if (redirectAttributeGet == null)
					redirectAttributeGet = IsAttribute(enumBuilderClassessAttribute.RedirectGet, attrs);
				else
					redirectAttributeSet = redirectAttributeGet;
				if (redirectAttributeSet == null)
					redirectAttributeSet = IsAttribute(enumBuilderClassessAttribute.RedirectSet, attrs);
			

				if (redirectAttributeGet != null && (data.Get = redirectAttributeGet.m_metodExt) == null)
				//пытаемся найти проперть на которую ссылаемся в базовом классе 
					data.Get = FindRedirecData(redirectAttributeGet.NameProperyRedirect);

				if (redirectAttributeGet == redirectAttributeSet)
					data.Set = data.Get;
				else if (redirectAttributeSet != null && (data.Set = redirectAttributeSet.m_metodExt) == null)
					data.Set = FindRedirecData(redirectAttributeSet.NameProperyRedirect);

				return data;
		}
		/// <summary>
		/// формируем проперти
		/// </summary>
		/// <param name="property">инфо порперти</param>
		/// <returns>результат</returns>
		bool BuildProperty(PropertyInfo property)
		{
			if (property == null)
				return false;
			try
			{
				FieldBuilder fieldProperty = null;
				if (property.Name == "IsCommented")
					fieldProperty = null;

				DataRedirect dataRedirect = FindRedirectAttribyte(property);
				object[] attrs = property.GetCustomAttributes(true);
				if (IsAttribute(enumBuilderClassessAttribute.EnumerateTransfer, attrs) != null)
				{
					Type t = property.PropertyType;
					Type[] types = property.PropertyType.GetGenericArguments();
					t = typeof(AuvtoEnumerable<>).MakeGenericType(types);
					fieldProperty = m_TypeBuilder.DefineField("m_" + property.Name,
															t,
															FieldAttributes.Public);
						m_listFieldAutoInitialise.Add(fieldProperty, t);
				}
				else if( dataRedirect.Get == null || dataRedirect.Set == null)
				{
					//если не редиректная - то создаем переменную в классе
					fieldProperty = m_TypeBuilder.DefineField("m_" + property.Name,
															property.PropertyType,
															FieldAttributes.Public);
                    BuilderClassesPropertyAttribyte bk = IsAttribute(enumBuilderClassessAttribute.AutoGenerate, attrs);
                    if ( bk != null)
					{
                        if (bk is cPropertyAttributeBaseClassType)
                        {
                            cPropertyAttributeBaseClassType c = bk as cPropertyAttributeBaseClassType;
                            if (c.mIinterfacesf != null)
                            {
                                Array.Resize(ref c.mIinterfacesf, c.mIinterfacesf.Length + 1);
                                c.mIinterfacesf[c.mIinterfacesf.Length - 1] = property.PropertyType;
                            }
                            else
                            {
                                c.mIinterfacesf = new Type[1];
                                c.mIinterfacesf[0] = property.PropertyType;
                            }
                            Type t = m_TypeGenerator.GetType((c.mtypeClasessToBase == null ? typeof(PropertyByInterfece) : c.mtypeClasessToBase),
                                c.mIinterfacesf);
                            m_listFieldAutoInitialise.Add(fieldProperty, t);
                        }
                        else
                        {
                            Type t = m_TypeGenerator.GetType(typeof(PropertyByInterfece), property.PropertyType);
                            m_listFieldAutoInitialise.Add(fieldProperty, t);
                        }
					}
				}
				//создаем построитель для проперти
				/// Последний аргумент DefineProperty является недействительным, так как
				/// Свойство не имеет параметров. (Если вы не укажете нулевое, вы должны
				/// Задать массив типа объектов. Для параметров собственности,
				/// Используем массив без элементов: новый Type [] {})
				PropertyBuilder custNamePropBldr = m_TypeBuilder.DefineProperty(property.Name,
																 System.Reflection.PropertyAttributes.HasDefault,
																 property.PropertyType,
																 null);
				//аттрибуты для проперти
				if (attrs != null && attrs.Length > 0)
				{
					int cnt = attrs.Length;
					for (int a = 0; a < cnt; a++)
					{
						CustomAttributeBuilder caBuild = BuilderClassesPropertyAttribyte.GetNewConstructor(attrs[a] as BuilderClassesPropertyAttribyte);
						if (caBuild != null)
							custNamePropBldr.SetCustomAttribute(caBuild);
					}
				}

				CreatePropertyGetMetod(property, custNamePropBldr, fieldProperty, dataRedirect.Get);
				CreatePropertySetMetod(property, custNamePropBldr, fieldProperty, dataRedirect.Set);

			}
			catch (System.Exception ex)
			{
				Error("BuilderClasses - BuildProperty", ex);
				return false;
			}
			return true;
		}
		/// <summary>
		/// создание евентов
		/// </summary>
		/// <returns>результат</returns>
		bool CreateEvent()
		{
			try
			{

				int count = m_eventInfoInerface.Count;
				for (int a = 0; a < count; a++)
					if (!BuidEvent(m_eventInfoInerface[a]))
						return false;
			}
			catch (System.Exception ex)
			{
				Error("BuilderClasses - CreateEvent", ex);
				return false;
			}
			return true;
		}
		/// <summary>
		/// построитель евентов
		/// </summary>
		/// <param name="eventInfo">евент инфо</param>
		/// <returns>результат</returns>
		bool BuidEvent(EventInfo eventInfo)
		{
			if (eventInfo == null)
				return false;
			try
			{
				//создаем построитель поле
				FieldBuilder eventField = m_TypeBuilder.DefineField(eventInfo.Name, eventInfo.EventHandlerType, FieldAttributes.Private);
				//создаем строитель евента
				EventBuilder eb = m_TypeBuilder.DefineEvent(eventInfo.Name, EventAttributes.None, eventInfo.EventHandlerType);

				MethodBuilder mbEV = null;
				ILGenerator il = null;
				MethodImplAttributes eventMethodFlags = MethodImplAttributes.Managed; //| MethodImplAttributes.Synchronized;
				#region EVENT_REMOVE
				MethodInfo miRemoveEvent = eventInfo.GetRemoveMethod();
				if (miRemoveEvent != null)
				{
					mbEV = m_TypeBuilder.DefineMethod("remove_" + eventInfo.Name, MethodAttributes.Public |
					MethodAttributes.SpecialName | MethodAttributes.NewSlot |
					MethodAttributes.HideBySig | MethodAttributes.Virtual |
					MethodAttributes.Final, null, new[] { eventInfo.EventHandlerType });

					mbEV.SetImplementationFlags(eventMethodFlags);
					il = mbEV.GetILGenerator();
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, eventField);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) }), null);
					il.Emit(OpCodes.Castclass, eventInfo.EventHandlerType);
					il.Emit(OpCodes.Stfld, eventField);
					il.Emit(OpCodes.Ret);
					m_TypeBuilder.DefineMethodOverride(mbEV, miRemoveEvent);
					eb.SetRemoveOnMethod(mbEV);
				}
				#endregion
				#region EVENT_ADD
				MethodInfo miAddEvent = eventInfo.GetAddMethod();
				if (miAddEvent != null)
				{
					mbEV = m_TypeBuilder.DefineMethod("add_" + eventInfo.Name, MethodAttributes.Public |
						MethodAttributes.SpecialName | MethodAttributes.NewSlot |
						MethodAttributes.HideBySig | MethodAttributes.Virtual |
						MethodAttributes.Final, null, new[] { eventInfo.EventHandlerType });

					mbEV.SetImplementationFlags(eventMethodFlags);
					il = mbEV.GetILGenerator();
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, eventField);
					il.Emit(OpCodes.Ldarg_1);
					il.EmitCall(OpCodes.Call, typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) }), null);
					il.Emit(OpCodes.Castclass, eventInfo.EventHandlerType);
					il.Emit(OpCodes.Stfld, eventField);
					il.Emit(OpCodes.Ret);
					m_TypeBuilder.DefineMethodOverride(mbEV, miAddEvent);
					eb.SetAddOnMethod(mbEV);
				}
				#endregion
			}
			catch (System.Exception ex)
			{
				Error("BuilderClasses - BuidEvent", ex);
				return false;
			}
			return true;
		}
		#endregion

		#region REMOVE_DOUBLE_PROPERTY
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
			try
			{

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
						Type t = fnd[0].PropertyType;
						bool isTypeNot = false;
						for (int b = 0; b < fnd.Count; b++)
						{
							tmp += fnd[b].ReflectedType.FullName + "\tТип данных = " + fnd[b].PropertyType.Name + "\r\n";
							if (t != fnd[b].PropertyType)
								isTypeNot = true;
							propertyInfoInterface.Remove(fnd[b]);
							if (t != fnd[b].PropertyType)
								isTypeNot = true;
							//получаем расширенные аттрибуты
							object[] attrs = fnd[b].GetCustomAttributes(false);
							if (attrs.Length > 0)
							{
								fullMetod = fnd[b];
								if (fnd[b].CanRead)
									mDouble.getMetod = fnd[b].GetGetMethod();
								if (fnd[b].CanWrite)
									mDouble.setMetod = fnd[b].GetSetMethod();
							}

							if (fnd[b].CanRead && mDouble.getMetod == null)
								mDouble.getMetod = fnd[b].GetGetMethod();
							if (fnd[b].CanWrite && mDouble.setMetod == null)
								mDouble.setMetod = fnd[b].GetSetMethod();

						}
#if DEBUG
						if (isTypeNot)
							Error("DEBUG:\r\nПовторяющийся параметр с разными возращаемыми типами с именем: " + fnd[0].Name + "\r\nВ интерфейсах:\r\n" + tmp);
							//MessageBox.Show("DEBUG:\r\nПовторяющийся параметр с разными возращаемыми типами с именем: " + fnd[0].Name + "\r\nВ интерфейсах:\r\n" + tmp);
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
			}
			catch (System.Exception ex)
			{
				SV.Tools.ErrorTracerpt.Error("BuilderClasses - RemoveDoubleProperty", ex);
			}

			return m_doubleList;
		}
		#endregion
	}
	#endregion
#endregion
//////////////////////////////////////////////////////////////////////////
#region HELPER_CLASSE \ Вспомогательные классы
	/// <summary>
	/// перечислитель аттрибутов
	/// </summary>
	public enum enumBuilderClassessAttribute
	{
		AutoGenerate,
		EnumerateTransfer,
		Redirect,
		RedirectGet,
		RedirectSet,
		Recalculation,
		NoRecalculation
	}
	/// <summary>
	/// класс дополнительных атрибутов для параметров сметы
	/// <remarks>
	/// при работе с автогенератором классов при имеющемся этом парметре над классом, 
	/// этот класс будет автоматически сгенерирован и прилинкован к параметру,
	/// тоесть при генерации ICost - входяший в него ICostValue будет инициализирован и прилинкован
	/// </remarks>
	/// <example>
	/// [BuilderClassesPropertyAttribyte(enumEstimateAttribute.AutoGenerate)]
	/// public interface ICostValue
	/// {
	/// 	double Basic { get; set; }     // базовая стоимость
	/// 	double Current { get; set; }   // текущая стоимость
	/// }
	/// [BuilderClassesPropertyAttribyte()]
	/// 
	///	public interface ICost
	///	{
	///		ICostValue Value { get; set; }
	/// }
	/// </example>
	/// <example>
	/// [BuilderClassesPropertyAttribyte("Root")]
	/// IEstimate Estimate { get; } 
	/// //////равзвернеться в:
	/// IEstimate Estimate {
	/// get {
	///		return Root as IEstimate;
	/// } 
	/// </example>
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Property)]
	public class BuilderClassesPropertyAttribyte : System.Attribute
	{
		enumBuilderClassessAttribute curAttribute;
		public string NameProperyRedirect = "";
		public MethodInfo m_metodExt = null;
		Type extClassType = null;
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="attr">тип аттрибута</param>
		public BuilderClassesPropertyAttribyte(enumBuilderClassessAttribute attr = enumBuilderClassessAttribute.AutoGenerate)
		{
			curAttribute = attr;
		}
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
		/// <param name="attr"></param>
		public BuilderClassesPropertyAttribyte(string PropertyRedirect, enumBuilderClassessAttribute attr = enumBuilderClassessAttribute.Redirect, Type typeClasessExtMetod = null)
			:this(attr)
		{
			//curAttribute = attr;
			NameProperyRedirect = PropertyRedirect;
			extClassType = typeClasessExtMetod;
			if (typeClasessExtMetod != null)
				m_metodExt = typeClasessExtMetod.GetMethod(PropertyRedirect);
		}		
		/// <summary>
		/// тип аттрибута
		/// </summary>
		public enumBuilderClassessAttribute MoreAttribyte
		{
			get { return curAttribute;}
			set{ curAttribute = value;}
		}

		static public CustomAttributeBuilder GetNewConstructor(BuilderClassesPropertyAttribyte sources)
		{
			if (sources == null)
				return null;
			CustomAttributeBuilder caBuild = null;
			ConstructorInfo conInf = null;// typeof(BuilderClassesPropertyAttribyte).GetConstructor(new Type[] { typeof(enumBuilderClassessAttribute) });
			switch (sources.MoreAttribyte)
			{
				case enumBuilderClassessAttribute.Redirect:
				case enumBuilderClassessAttribute.RedirectGet:
				case enumBuilderClassessAttribute.RedirectSet:
					conInf = typeof(BuilderClassesPropertyAttribyte).GetConstructor(new Type[] { typeof(string), typeof(enumBuilderClassessAttribute), typeof(Type) });
					caBuild = new CustomAttributeBuilder(conInf, new object[] { sources.NameProperyRedirect, sources.MoreAttribyte, sources.extClassType });
					break;				
				case enumBuilderClassessAttribute.AutoGenerate:
				default:
					conInf = typeof(BuilderClassesPropertyAttribyte).GetConstructor(new Type[] { typeof(enumBuilderClassessAttribute) });
					caBuild = new CustomAttributeBuilder(conInf, new object[] { sources.MoreAttribyte });
					break;

			}
			return caBuild;
		}
	}
    /// <summary>
    /// этот аттрибут накладываеться на интерфейс, так же автоматически создает обьект в памяти
    /// передаваемый тип будет служить базовым классом при автогенерации это интерфейса
    /// </summary>
    public class cPropertyAttributeBaseClassType : BuilderClassesPropertyAttribyte
    {
        public Type mtypeClasessToBase;
        public Type[] mIinterfacesf;
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="typeClasessToBase">передаваемый тип будет служить базовым классом при автогенерации это интерфейса
        /// обязательно тип должен быть типом класса</param>
        public cPropertyAttributeBaseClassType(Type typeClasessToBase)
            : base(enumBuilderClassessAttribute.AutoGenerate)
        {
            mtypeClasessToBase = typeClasessToBase;
        }
        public cPropertyAttributeBaseClassType(Type typeClasessToBase, params Type[] interfacesf)
            : this(typeClasessToBase)
        {
            mIinterfacesf = interfacesf;
        }
    }
    /// <summary>
    /// класс-заглушка, так для всякой фигни
    /// </summary>
    [Serializable()]
	public class EmptyTemplate
	{

	}
	public delegate void CallPropertyChange(string propertyName, object CommandID = null, object oldData = null, object newData = null);	
	/// <summary>
	/// интерфейс для матрешек, перенаправление вызова
	/// </summary>
	public interface iMatryoshkaCall : INotifyPropertyChanged, IDisposable
	{
		string _propertyNameThisClass
		{
			get;
			set;
		}
		CallPropertyChange DelegateCalling { get; set; }
		void CallPropertyChange(string propertyName, object CommandID = null, object oldData = null, object newData = null);
		/// <summary>
		/// функция получения данных и параметра по имени параметра
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		/// <param name="param">дополнительные параметры</param>
		/// <returns>данные, могут быть null если не найденн параметр или он нулевой</returns>
		object GetData(object propertyName, object[] param = null);
		/// <summary>
		/// установка данных в параметр по имени
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		/// <param name="newPropertyData">новое значение</param>
		/// <param name="param">дополнительные параметры</param>
		/// <returns>false - если параметр небыл найден</returns>
		bool SetData(object propertyName, object newPropertyData, object[] param = null);
		void ReInitDelegate();
		/// <summary>
		/// родительский узел
		/// </summary>
	}
	/// <summary>
	/// класс реализации
	/// </summary>
	[Serializable()]
	public class PropertyByInterfece :  iMatryoshkaCall
	{

		#region DESTRUCTOR

		bool isDispose = false;
		/// <summary>
		/// диструктор
		/// </summary>
		~PropertyByInterfece()
		{
			if (!isDispose)
				Dispose();
		}
		/// <summary>
		/// освобождение ресурсов
		/// </summary>
		public void Dispose()
		{
			Clear();
		}
		void Clear()
		{
			m_DelegateCalling = null;
			isDispose = true;
			FieldInfo[] fl = this.GetType().GetFields();
			int count = fl.Length;
			for (int a = 0; a < count; a++)
			{
				iMatryoshkaCall field = fl[a].GetValue(this) as iMatryoshkaCall;
				if (field != null)
				{
					field.Dispose();
				}
			}
		}

#endregion
		/// <summary>
		/// конструктор
		/// </summary>
		public PropertyByInterfece()
		{

		}
		string m_propertyNameThisClass = "";
		/// <summary>
		/// название параметра с каким асоциируеться текущий экземпляр класса
		/// </summary>
		public string _propertyNameThisClass
		{
			get { return m_propertyNameThisClass; }
			set { m_propertyNameThisClass = value; }
		}
		/// <summary>
		/// делегат пересылки сообщений в верхний уровень
		/// </summary>
		[NonSerialized]
		CallPropertyChange m_DelegateCalling = null;
		/// <summary>
		/// делегат пересылки сообщений в верхний уровень
		/// </summary>
		public CallPropertyChange DelegateCalling
		{
			get { return m_DelegateCalling; }
			set { m_DelegateCalling = value; }
		}
        /// <summary>
        /// флаг что хоть одна проперти была изменена
        /// </summary>
        private bool m_isPropertyChanged = false;
        protected bool isPropertyChanged
        {
            get { return m_isPropertyChanged; }
        }
        /// <summary>
        /// список измененных парамтров
        /// </summary>
        List<string> m_propertyNamesChanged = null;
        protected List<string> propertyNamesChanged
        { get { return m_propertyNamesChanged; } }
        bool m_isLogPropertyChanged = false;
        public bool StartWriteLogChangeProperty
        {
            set { m_isLogPropertyChanged = value; }
            get { return m_isLogPropertyChanged; }
        }
		/// <summary>
		/// функция пересылки сообщения об изменении паравметра в выщестоящщий уровень
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		public virtual void CallPropertyChange(string propertyName, object CommandID = null, object oldData = null, object newData = null)
		{	
            if(m_isLogPropertyChanged)
            {
                m_isPropertyChanged = true;
                if (m_propertyNamesChanged == null)
                    m_propertyNamesChanged = new List<string>();
                m_propertyNamesChanged.Add(propertyName);
            }            	
			if (m_DelegateCalling != null)
				m_DelegateCalling((m_propertyNameThisClass != "" ? m_propertyNameThisClass + "." : "") + propertyName, CommandID, oldData, newData);
			if (PropertyChanged != null)
				PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// функция получения данных и параметра по имени параметра
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>данные, могут быть null если не найденн параметр или он нулевой</returns>
		public object GetData(object propertyName, object[] param = null)
		{
			object ob = ClassesTools.GetData(this, propertyName.ToString(), param);
			return ob;
		}
		/// <summary>
		/// установка данных в параметр по имени
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		/// <param name="newPropertyData">новое значение</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>false - если параметр небыл найден</returns>
		public bool SetData(object propertyName, object newPropertyData, object[] param = null)
		{
			return ClassesTools.SetData(this, propertyName.ToString(), newPropertyData, param);
		}
		#region Члены INotifyPropertyChanged
		/// <summary>
		/// сообщение об изменении данных параметра
		/// </summary>
		[field: NonSerialized()]
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		public void ReInitDelegate()
		{			
			FieldInfo[] fl = this.GetType().GetFields();
			int count = fl.Length;
			for (int a = 0; a < count; a++)
			{
				iMatryoshkaCall field = fl[a].GetValue(this) as iMatryoshkaCall;
				if (field != null)
				{					
					field.ReInitDelegate();
					field.DelegateCalling = this.CallPropertyChange;
				}
			}			
		}

	}

	[Serializable()]
	public class AuvtoEnumerable<T> : List<T>, IList<T>, System.Collections.IList, ICollection<T>, iMatryoshkaCall
	{

		
		#region DESTRUCTOR

		bool isDispose = false;
		/// <summary>
		/// диструктор
		/// </summary>
		~AuvtoEnumerable()
		{
			if (!isDispose)
				Dispose();
		}
		/// <summary>
		/// освобождение ресурсов
		/// </summary>
		public void Dispose()
		{
			mClear();
		}
		void mClear()
		{
			m_DelegateCalling = null;
			isDispose = true;
			int count = Count;
			for (int a = 0; a < count; a++)
			{
				iMatryoshkaCall field = this[a] as iMatryoshkaCall;
				if (field != null)
				{
					field.Dispose();
				}
			}
			base.Clear();
			base.TrimExcess();
		}

#endregion
		string m_propertyNameThisClass = "";
		/// <summary>
		/// название параметра с каким асоциируеться текущий экземпляр класса
		/// </summary>
		public string _propertyNameThisClass
		{
			get { return m_propertyNameThisClass; }
			set { m_propertyNameThisClass = value; }
		}
		object m_parent = null;
		/// <summary>
		/// родительский узел
		/// </summary>
		public object Parent
		{
			get { return m_parent; }
			set { m_parent = value; }
		}
		/// <summary>
		/// делегат пересылки сообщений в верхний уровень
		/// </summary>
		[NonSerialized]
		CallPropertyChange m_DelegateCalling = null;
		/// <summary>
		/// делегат пересылки сообщений в верхний уровень
		/// </summary>
		public CallPropertyChange DelegateCalling
		{
			get { return m_DelegateCalling; }
			set { m_DelegateCalling = value; }
		}
		/// <summary>
		/// функция пересылки сообщения об изменении паравметра в выщестоящщий уровень
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		public void CallPropertyChange(string propertyName, object CommandID = null, object oldData = null, object newData = null)
		{
			object b = newData;
			b = oldData;
			if (m_DelegateCalling != null)
				m_DelegateCalling((m_propertyNameThisClass != "" ? m_propertyNameThisClass + "." : "") + propertyName, CommandID, oldData, newData);
			if (PropertyChanged != null)
				PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// функция получения данных и параметра по имени параметра
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>данные, могут быть null если не найденн параметр или он нулевой</returns>
		public object GetData(object propertyName, object[] param = null)
		{
			object ob = ClassesTools.GetData(this, propertyName.ToString(), param);
			return ob;
		}
		/// <summary>
		/// установка данных в параметр по имени
		/// </summary>
		/// <param name="propertyName">имя параметра</param>
		/// <param name="newPropertyData">новое значение</param>
		///<param name="param">передаваемые параметры</param>
		/// <returns>false - если параметр небыл найден</returns>
		public bool SetData(object propertyName, object newPropertyData, object[] param = null)
		{
			return ClassesTools.SetData(this, propertyName.ToString(), newPropertyData, param);
		}
		#region Члены INotifyPropertyChanged
		/// <summary>
		/// сообщение об изменении данных параметра
		/// </summary>
		[field: NonSerialized()]
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		public void ReInitDelegate()
		{
			int count = Count;
			for (int a = 0; a < count; a++)
			{
				iMatryoshkaCall field = this[a] as iMatryoshkaCall;
				if (field != null)
				{
					field._propertyNameThisClass = "";// a.ToString();
					field.ReInitDelegate();
					field.DelegateCalling = this.CallPropertyChange;
				}
			}

		}

		int System.Collections.IList.Add(object value)
		{
			if (value is T)
			{
				Add((T)value);
				return Count;
			}
			return -1;
		}
		public new void Add(T item)
		{
			iMatryoshkaCall field = item as iMatryoshkaCall;
			if (field != null)
			{
				field._propertyNameThisClass = Count.ToString();
				field.ReInitDelegate();
				field.DelegateCalling = this.CallPropertyChange;
				CallPropertyChange(m_propertyNameThisClass, null, null, item);
			}
			base.Add(item);
		}
		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		void System.Collections.IList.Insert(int index, object item)
		{
			if (item is T)
				Insert(index, (T)item);
		}
		public new void Insert(int index, T item)
		{
			iMatryoshkaCall field = item as iMatryoshkaCall;
			if (field != null)
			{
				field._propertyNameThisClass = index.ToString();
				field.ReInitDelegate();
				field.DelegateCalling = this.CallPropertyChange;
				CallPropertyChange(m_propertyNameThisClass, null, index, item);
			}
			base.Insert(index, item);
		}
		void IList<T>.Insert(int index, T item)
		{
			Insert(index, item);
		}
	}
/*
	public class XMLFile : IXmlSerializable
	{

		#region Члены IXmlSerializable

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				reader.ReadStartElement("Field");
				string name = reader.GetAttribute("Name");
				string className = reader.GetAttribute("ClassName");
				string data = "";
				if(className != "")
				{

				}
				else
					data = reader.ReadElementString("Data");
				//FieldInfo inf = 
				/ *
				reader.ReadStartElement("item");
								string key = reader.ReadElementString("key");
								string value = reader.ReadElementString("value");
								reader.ReadEndElement();
								reader.MoveToContent();
								this.Add(key, value);* /
				reader.MoveToContent();
			}
			//reader.ReadEndElement();
			
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			FieldInfo[] fileds = GetType().GetFields();
			int count = fileds.Length;
			for (int a = 0; a < count; a++)
			{
				writer.WriteStartElement("Field");
				writer.WriteAttributeString("Name", fileds[a].Name);
				object ob = fileds[a].GetValue(this);
				if (fileds[a].FieldType.IsInterface)
				{					
					XMLFile fl = ob as XMLFile;
					if (fl != null)
					{
						writer.WriteAttributeString("ClassName", fl.ToString());
						fl.WriteXml(writer);
					}
				}
				else
					writer.WriteElementString("Data", ob.ToString());
				writer.WriteEndElement();
			}
		}

		#endregion
	}*/

#endregion

}
