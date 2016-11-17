using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SV.Tools;

namespace GCSVTest.Test
{
   public interface i1
    {
        [cPropertyAttributeBaseClassType(typeof(BaseTestClass))]
        i2 i1_oI2 { get; set; }
        int i1_i1 { get; set; }
        int i1_i2 { get; set; }
        int i1_i3 { get; set; }
        bool i1_b1 { get; set; }
        string i1_s1 { get; set; }       
        [cPropertyAttributeBaseClassType(typeof(BaseTestClass))]
        i3 i1_oI3 { get; set; }
    }
    
    public interface i2
    {
        string i2_s1 { get; set; }
        int i2_i1 { get; set; }
        double i2_d1 { get; set; }
        string i2_s2 { get; set; }
        [cPropertyAttributeBaseClassType(typeof(BaseTestClass))]
        i3 i2_oI3 { get; set; }
    }
    public interface i3
    {
        int i3_i1 { get; set; }
        int i3_i2 { get; set; }
    }
    public class BaseTestClass : PropertyByInterfece
    {
        public static string[] TestData = { "1;5;23;true;asadas", "hju;34;45,6;adas", "0;23" };
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="pos">позиция в массиве данных</param>
        public BaseTestClass()
        {
            Init();
        }
        /// <summary>
        /// инициализация, заполнения класса данными
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            bool result = true;
            i1 ii1 = this as i1;
            i2 ii2 = this as i2;
            i3 ii3 = this as i3;
            string[] s;
            if (ii1 != null)
            {
                s = TestData[0].Split(';');
                ii1.i1_i1 = Convert.ToInt16(s[0]);
                ii1.i1_i2 = Convert.ToInt16(s[1]);
                ii1.i1_i3 = Convert.ToInt16(s[2]);
                ii1.i1_b1 = Convert.ToBoolean(s[3]);
                ii1.i1_s1 = s[4];
            }
            if(ii2 != null)
            {
                s = TestData[1].Split(';');
                ii2.i2_s1 = s[0];
                ii2.i2_i1 = Convert.ToInt16(s[1]);
                ii2.i2_d1 = Convert.ToDouble(s[2]);
                ii2.i2_s2 = s[3];
            }
            if(ii3 != null)
            {
                s = TestData[2].Split(';');
                ii3.i3_i1 = Convert.ToInt16(s[0]);
                ii3.i3_i2 = Convert.ToInt16(s[1]);
            }
            return result;
        }
    }
}
