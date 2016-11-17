using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCSV;
using GCSVTest.Test;
using SV.Tools;
using SV.Tools.CastomAttribute;

namespace GCSVTest
{
    class Program
    {
        static void Main(string[] args)
        {
           i1 i = EXP.RealiseInterfeces(typeof(BaseTestClass), typeof(i1)) as i1;          
        }
    }
}
