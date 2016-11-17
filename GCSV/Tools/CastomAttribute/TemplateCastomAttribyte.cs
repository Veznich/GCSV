using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV.Tools.CastomAttribute
{
    #region Attribyte
   
    public class cPropertyAttribyteGet : BuilderClassesPropertyAttribyte
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyteGet(string PropertyRedirect, Type typeClasessExtMetod)
            : base(PropertyRedirect, enumBuilderClassessAttribute.RedirectGet, typeClasessExtMetod)
        {

        }
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyteGet(string PropertyRedirect)
            : base(PropertyRedirect, enumBuilderClassessAttribute.RedirectGet, null)
        {

        }
    }
    public class cPropertyAttribyteSet : BuilderClassesPropertyAttribyte
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyteSet(string PropertyRedirect, Type typeClasessExtMetod)
            : base(PropertyRedirect, enumBuilderClassessAttribute.RedirectSet, typeClasessExtMetod)
        {

        }
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyteSet(string PropertyRedirect)
            : base(PropertyRedirect, enumBuilderClassessAttribute.RedirectSet, null)
        {

        }
    }
    public class cPropertyAttribyteRedirect : BuilderClassesPropertyAttribyte
    {
        /// <summary>
		/// конструктор
		/// </summary>
		/// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyteRedirect(string PropertyRedirect)
            : base(PropertyRedirect, enumBuilderClassessAttribute.RedirectSet)
        {

        }
    }
    public class cPropertyAttribyte : BuilderClassesPropertyAttribyte
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="attr">тип аттрибута</param>
        public cPropertyAttribyte(enumBuilderClassessAttribute attr = enumBuilderClassessAttribute.AutoGenerate)
            : base(attr)
        {
        }
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public cPropertyAttribyte(string PropertyRedirect)
            : base(PropertyRedirect)
        {

        }

        public cPropertyAttribyte(string PropertyRedirect, Type tpExtClasses)
            : base(PropertyRedirect, enumBuilderClassessAttribute.Redirect, tpExtClasses)
        {

        }
    }
    public class RedirectFunctionAttribyte : BuilderClassesPropertyAttribyte
    {
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="PropertyRedirect">имя проперти для редиректа если Redirect</param>
        public RedirectFunctionAttribyte(string PropertyRedirect)
            : base(PropertyRedirect, enumBuilderClassessAttribute.Redirect)
        {

        }
    }
    #endregion
    
}
