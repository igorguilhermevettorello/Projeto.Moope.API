using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DecimalRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public DecimalRangeAttribute(double min, double max)
        {
            _min = Convert.ToDecimal(min);
            _max = Convert.ToDecimal(max);
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            try
            {
                decimal valor = Convert.ToDecimal(value);
                return valor >= _min && valor <= _max;
            }
            catch
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return $"O campo {name} deve estar entre {_min} e {_max}.";
        }
    }
}
