using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.EnumExtensions
{
    public static class EnumExtensions
    {
        public static string ParseEnumToString<T>(this T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }
    }
}
