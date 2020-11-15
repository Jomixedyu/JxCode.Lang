using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.NepInterpreter.Lib
{
    public static class logic
    {
        public static bool eq(object obj1, object obj2)
        {
            return obj1.Equals(obj2);
        }
    }
}
