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
            double valueResult = valueInVolt * 1000.0;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мВ]");

            return valueResult;
        }

        public static double ConvertVoltToMilliAmpere(double valueInVolt)
        {
            double valueResult = valueInVolt / GlobalVars.Rizm * 1_000.0;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мА]");

            return valueResult;
        }

        public static double ConvertVoltToMicroAmpere(double valueInVolt)
        {
            double valueResult = valueInVolt / GlobalVars.Rizm * 1_000_000.0;
            log.Debug($"КОНВЕРТЕР: {valueInVolt:F7} [В] -> {valueResult:F7} [мкА]");

            return valueResult;
        }
    }
}
