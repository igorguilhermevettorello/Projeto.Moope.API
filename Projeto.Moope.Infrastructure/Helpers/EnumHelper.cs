using System.ComponentModel;
using System.Reflection;

namespace Projeto.Moope.Infrastructure.Helpers
{
    public static class EnumHelper
    {
        public static List<object> GetEnumAsList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new
                {
                    value = Convert.ToInt32(e),
                    label = GetDescription(e)
                })
                .ToList<object>();
        }

        private static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
