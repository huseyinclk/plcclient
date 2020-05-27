using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon
{
    public static class ExtensionMethod
    {
        public static bool IsNull(this object Obj)
        {
            if (object.ReferenceEquals(Obj, null))
                return true;

            return false;
        }

        public static bool IsNotNull(this object Obj)
        {
            if (object.ReferenceEquals(Obj, null))
                return false;
            return true;
        }

        public static string StringNull(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            return str;
        }

        public static Enum IntToEnum(this int e, Type t)
        {
            if (Enum.IsDefined(t, e))
            {
                return (Enum)Enum.ToObject(t, e);
            }
            return (Enum)Activator.CreateInstance(t);
        }

        public static string TrToEn(this string s)
        {
            s = s.Replace("--", "");
            s = s.Replace("\'", "");
            s = s.Replace("\"", "");
            s = s.Replace("%", "");

            s = s.Replace('Ç', 'C');
            s = s.Replace('ç', 'c');
            s = s.Replace('ı', 'i');
            s = s.Replace('İ', 'I');

            s = s.Replace('ğ', 'g');
            s = s.Replace('Ğ', 'G');

            s = s.Replace('ö', 'o');
            s = s.Replace('Ö', 'O');

            s = s.Replace('ü', 'u');
            s = s.Replace('Ü', 'U');

            s = s.Replace('ş', 's');
            s = s.Replace('Ş', 'S');
            return s;
        }
    }
}
