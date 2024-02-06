using System;
using System.ComponentModel;
using System.Reflection;

namespace Parameters1903M.Util
{
    internal static class EnumInfo
    {
        public static string GetDescription(Enum enumElement)
        {
            Type type = enumElement.GetType();

            MemberInfo[] memberInfo = type.GetMember(enumElement.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return enumElement.ToString();
        }
    }
}
