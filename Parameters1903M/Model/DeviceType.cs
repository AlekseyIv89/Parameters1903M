using System.ComponentModel;

namespace Parameters1903M.Model
{
    internal enum DeviceType
    {
        [Description(default)] None,
        [Description("ЦЕ1903М")] TSE1903M
    }
}
