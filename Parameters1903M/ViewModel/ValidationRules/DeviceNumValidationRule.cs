using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Parameters1903M.ViewModel.ValidationRules
{
    internal class DeviceNumValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch(value.ToString(), Properties.Settings.Default.DeviceNumRegex)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Номер изделия введен некорректно");
        }
    }
}
