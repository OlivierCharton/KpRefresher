using System;
using System.ComponentModel.DataAnnotations;

namespace KpRefresher.Extensions
{
    public static class StringExtensions
    {
        public static T GetValueFromName<T>(this string name) where T : Enum
        {
            var type = typeof(T);

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute)
                {
                    if (string.Equals(attribute.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (T)field.GetValue(null);
                    }
                }

                if (string.Equals(field.Name, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }
    }
}