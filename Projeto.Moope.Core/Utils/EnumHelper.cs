using System.ComponentModel;
using System.Reflection;

namespace Projeto.Moope.Core.Utils
{
    public static class EnumHelper
    {
        public static (TEnum EnumValue, int Code, string Description)? GetEnumInfo<TEnum>(string value)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            TEnum? enumValue = null;

            // 🔹 Tenta casar pelo nome do enum (ignora case)
            foreach (var e in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (string.Equals(e.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    enumValue = e;
                    break;
                }
            }

            // 🔹 Se não achou pelo nome, tenta pela descrição
            if (enumValue == null)
            {
                foreach (var e in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
                {
                    var description = e.GetType()
                        .GetMember(e.ToString())
                        .First()
                        .GetCustomAttribute<DescriptionAttribute>()?
                        .Description;

                    if (description != null &&
                        string.Equals(description, value, StringComparison.OrdinalIgnoreCase))
                    {
                        enumValue = e;
                        break;
                    }
                }
            }

            // 🔹 Se não encontrou nada, retorna null
            if (enumValue == null)
                return null;

            // 🔹 Recupera a descrição
            string descriptionAttr = enumValue.Value
                .GetType()
                .GetMember(enumValue.Value.ToString())
                .First()
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? enumValue.Value.ToString();

            return (enumValue.Value, Convert.ToInt32(enumValue.Value), descriptionAttr);
        }
    }
}

