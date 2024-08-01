using System;
using System.Threading.Tasks;
using log4net;

namespace Parameters1903M.Util.Multimeter
{
    internal class Simulator : IMeasure
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Random randomGenerator;

        private double averagingTimeMillis = 0.0;

        public Simulator()
        {
            randomGenerator = new Random();
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("   ========== ЭМУЛЯТОР ПОДКЛЮЧЕН ===========   ");
        }

        public void SendCommand(string command)
        {
            
        }

        public void SetAverageTimeMillis(double averagingTimeMillis)
        {
            if (averagingTimeMillis > 0.0)
            {
                this.averagingTimeMillis = averagingTimeMillis;
                log.Debug($"ЭМУЛЯТОР -> установлено время осреднения, мс: {averagingTimeMillis}");
            }
        }

        public void ResetAverageTime()
        {
            averagingTimeMillis = 0.0;
        }

        public CommunicationInterface Connect(string address)
        {
            return CommunicationInterface.Emulator;
        }

        public async Task<MeasureResult> Measure(bool returnNegativeValue = false)
        {
            MeasureResult measureResult = await Task.Run(async () =>
            {
                await Task.Delay(500);
                double measuredValue = randomGenerator.Next(0, 20_000) / 100_000.0;
                if (returnNegativeValue)
                    if (measuredValue >= 0)
                        measuredValue *= -1;
                log.Debug($"ЭМУЛЯТОР -> сгенерированное значение, В: '{measuredValue:F7}'");
                return new MeasureResult(measuredValue);
            });
            return measureResult;
        }

        public void Disconnect()
        {
            log.Debug("   ========== ЭМУЛЯТОР ОТКЛЮЧЕН ===========   ");
        }
    }
}
