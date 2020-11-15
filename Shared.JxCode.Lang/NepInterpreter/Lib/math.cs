using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.NepInterpreter.Lib
{
    public static class math
    {
        public static float plus(float v1, float v2)
        {
            return v1 + v2;
        }
        public static float minus(float v1, float v2)
        {
            return v1 - v2;
        }
        public static float mul(float v1, float v2)
        {
            return v1 * v2;
        }
        public static float div(float v1, float v2)
        {
            return v1 / v2;
        }
    }
}
