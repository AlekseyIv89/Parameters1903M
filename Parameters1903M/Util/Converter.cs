using log4net;

namespace Parameters1903M.Util
{
    public static class Converter
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Converter()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static double ConvertVoltToMilliVolt(double valueInVolt)
        {
            decimal valueResult = (decimal)(valueInVolt) * 1000m;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мВ]");

            return decimal.ToDouble(valueResult);
        }

        public static double ConvertVoltToMilliAmpere(double valueInVolt)
        {
            decimal valueResult = (decimal)valueInVolt / (decimal)GlobalVars.Rizm * 1_000m;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мА] (R = {GlobalVars.Rizm:F5} Ом)");

            return decimal.ToDouble(valueResult);
        }

        public static double ConvertVoltToMicroAmpere(double valueInVolt)
        {
            decimal valueResult = (decimal)valueInVolt / (decimal)GlobalVars.Rizm * 1_000_000m;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мкА] (R = {GlobalVars.Rizm:F5} Ом)");

            return decimal.ToDouble(valueResult);
        }
    }
}
