using System;
using System.Collections.Generic;
using System.Text;

namespace eu.railduction.netcore.dll.Database_Attribute_System
{
    class Function
    {
        public static string SqlEscape(string str)
        {
            string str_cpy = str.Replace(@"'", @"\'");
            return str_cpy;
        }

        public static string SqlSerialise(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
