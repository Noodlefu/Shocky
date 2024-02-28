using System;
using System.Reflection;

namespace Shocky.Classes
{
    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attribute = fieldInfo?.GetCustomAttribute(typeof(StringValueAttribute)) as StringValueAttribute;

            return attribute?.Value ?? value.ToString();
        }
    }
}