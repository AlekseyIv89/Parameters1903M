using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Parameters1903M.Util.Multimeter
{
    internal class V2_43 : IMeasure
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SerialPort serialPort;

        private double averagingTimeMillis = 0.0;

        public V2_43()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("   ========== В2-43 ПОДКЛЮЧЕН ===========   ");
        }

        public void SendCommand(string command)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(command);
                log.Debug($"В2-43 -> отправлена служебная команда на вольтметр: {command}");
            }
        }

        public void SetAverageTimeMillis(double averagingTimeMillis)
        {
            if (averagingTimeMillis > 0.0)
            {
                this.averagingTimeMillis = averagingTimeMillis;
                log.Debug($"В2-43 -> установлено время осреднения, мс: {averagingTimeMillis}");
            }
        }

        public CommunicationInterface Connect(string address)
        {
            serialPort = new SerialPort(address, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 5000;
            serialPort.WriteTimeout = 5000;
            if (!serialPort.IsOpen)
                serialPort.Open();

            log.Debug($"В2-43 -> соединение установлено по {address}");

            return CommunicationInterface.V2_43;
        }

        public void Disconnect()
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();

            log.Debug("   ========== В2-43 ОТКЛЮЧЕН ===========   ");
        }

        public async Task<MeasureResult> Measure(bool returnNegativeValue = false)
        {
            DateTime startTime = DateTime.Now;
            MeasureResult measureResult = await Task.Run(() =>
            {
                List<double> tempData = new List<double>();
                do
                {
                    SendCommand("V");
                    string data = serialPort.ReadLine().Replace(',', '.').Trim();
                    log.Debug($"В2-43 -> получена строка: '{data}'");

                    double measuredValue = double.Parse(data, NumberStyles.Number, CultureInfo.InvariantCulture);
                    log.Debug($"В2-43 -> преобразованное значение напряжения, В: '{measuredValue:F7}'");

                    tempData.Add(measuredValue);
                }
                while (DateTime.Now.Subtract(startTime).TotalMilliseconds < averagingTimeMillis);

                double averageValue = tempData.Average();
                log.Debug($"В2-43 -> осредненное значение напряжения, В: '{averageValue:F7}'");
                log.Debug($"В2-43 -> принято значений за установленное время осреднения ({averagingTimeMillis} мс), шт: '{tempData.Count}'");
                return new MeasureResult(averageValue);
            });
            return measureResult;
        }
    }
}
