using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SV.Tools
{
	#region ErrorTracerpt
	/// <summary>
	/// класс вывода ошибок
	/// </summary>
	public class ErrorTracerpt
	{
		protected static string m_LastError = "";
		public static string LastError
		{
			get { return m_LastError; }
		}
		/// <summary>
		/// обработка ошибки
		/// </summary>
		/// <param name="function">имя функции отправителя</param>
		/// <param name="ex">ошибка</param>
		public static void Error(string function, System.Exception ex = null, object classSender = null, bool isSenderMessage = true)
		{
			if (ex == null)
				m_LastError = function;
			else
				m_LastError = (classSender != null ? "Class: " + classSender.GetType().Name + "\r\n" : "") + "Function error:" + function + "\r\nMessage: " + ex.Message;//"Class: " + this.GetType().Name + "\r\n +"\r\nStackTrace:" + ex.StackTrace;
#if DEBUG
			//if(isSenderMessage)
			//System.Windows.Forms.MessageBox.Show(m_LastError);

			System.Diagnostics.Debug.Print(m_LastError);
#else
			if (isSenderMessage)
				System.Windows.Forms.MessageBox.Show(m_LastError);
			//System.Diagnostics.Debug.Assert(false, m_LastError);
#endif
		}
	}
	#endregion
}
