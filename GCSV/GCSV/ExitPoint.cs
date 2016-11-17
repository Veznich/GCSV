using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SV.Tools;

namespace GCSV
{
    /// <summary>
    /// точка входа, основной клас через которые происходят вызовы
    /// </summary>
    public static class EXP
    {
        static TypeGenerator m_typeGenerator = new TypeGenerator();
        /// <summary>
        /// инициализация (реализация) интерфейсоф
        /// </summary>
        /// <param name="interfeceList">список интерфейсоф для реализации
        /// внимание!!! передаваемые интерфейсы должны быть публичные</param>
        /// <param name="baseClass">базовый класс, по улчанию (null) будет PropertyByInterfece</param>
        /// <returns>инициализированный	 обьект</returns>
        public static object RealiseInterfeces(Type baseClass, params Type[] interfeceList)
        {
            if (interfeceList == null || interfeceList.Length == 0)
            {
                return null;
            }
            if (baseClass != null && !baseClass.IsClass)
            {
                return null;
            }
            object ob = null;
            ob = m_typeGenerator.GetInstance((baseClass == null ? typeof(PropertyByInterfece) : baseClass), interfeceList);
            return ob;
        }

        /// <summary>
        /// инициализация (реализация) интерфейсоф
        /// </summary>
        /// <param name="interfeceList">список интерфейсоф для реализации
        /// внимание!!! передаваемые интерфейсы должны быть публичные</param>
        /// <param name="baseClass">базовый класс, по улчанию (null) будет PropertyByInterfece</param>
        /// <returns>инициализированный	 обьект</returns>
        public static object RealiseInterfeces(List<Type> interfeceList, Type baseClass = null)
        {
            if (interfeceList == null || interfeceList.Count == 0)
            {
                return null;
            }
            if (baseClass != null && !baseClass.IsClass)
            {
                return null;
            }
            object ob = null;
            ob = m_typeGenerator.GetInstance((baseClass == null ? typeof(PropertyByInterfece) : baseClass), interfeceList.ToArray());
            return ob;
        }
        /// <summary>
        /// инициализация (реализация) интерфейсоф базовый класс, по улчанию  будет PropertyByInterfece
        /// </summary>
        /// <param name="value">список интерфейсоф для реализации
        /// внимание!!! передаваемые интерфейсы должны быть публичные</param>
        /// <returns>инициализированный	 обьект</returns>
        public static object RealiseInterfeces(params Type[] value)
        {
            if (value == null)
            {
                return null;
            }            
            object ob = null;
            ob = m_typeGenerator.GetInstance(typeof(PropertyByInterfece), value);
            return ob;
        }
        /// <summary>
        /// инициализация (реализация) интерфейсоф базовый класс, по улчанию  будет PropertyByInterfece
        /// </summary>
        /// <param name="value">список интерфейсоф для реализации
        /// внимание!!! передаваемые интерфейсы должны быть публичные</param>
        /// <returns>инициализированный	 обьект</returns>
        public static T RealiseInterfeces<T>(params Type[] value)
        {
            if (value == null)
            {
                return default(T);
            }
            object ob = m_typeGenerator.GetInstance(typeof(PropertyByInterfece), value);
            return (T)ob;
        }
        /// <summary>
        /// инициализация (реализация) интерфейсоф базовый класс, по улчанию  будет PropertyByInterfece
        /// </summary>
        /// <param name="value">список интерфейсоф для реализации
        /// внимание!!! передаваемые интерфейсы должны быть публичные</param>
        /// <returns>инициализированный	 обьект</returns>
        public static T RealiseInterfeces<T>(Type baseClass,params Type[] value)
        {
            if (value == null)
            {
                return default(T);
            }
            object ob = m_typeGenerator.GetInstance(typeof(PropertyByInterfece), value);
            return (T)ob;
        }

    }
}
