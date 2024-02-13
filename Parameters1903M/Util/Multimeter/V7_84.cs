using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace Parameters1903M.Util.Multimeter
{
    internal class V7_84 : IMeasure
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SerialPort serialPort;

        private double averagingTimeMillis = 0.0;

        private readonly Regex inputData = new Regex(@"^[+|-]\d*\.\d+");

        public V7_84()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Debug("   ========== В7-84 ПОДКЛЮЧЕН ===========   ");
        }

        public void SendCommand(string command)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(command);
                log.Debug($"В7-84 -> отправлена служебная команда на мультиметр: {command}");
            }
        }

        public void SetAverageTimeMillis(double averagingTimeMillis)
        {
            if (averagingTimeMillis > 0.0)
            {
                this.averagingTimeMillis = averagingTimeMillis;
                log.Debug($"В7-84 -> установлено время осреднения, мс: {averagingTimeMillis}");
            }
        }

        public CommunicationInterface Connect(string address)
        {
            serialPort = new SerialPort(address, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 5000;
            serialPort.WriteTimeout = 5000;
            if (!serialPort.IsOpen)
                serialPort.Open();

            log.Debug($"В7-84 -> соединение установлено по {address}");

            return CommunicationInterface.V7_84;
        }

        public void Disconnect()
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();

            log.Debug("   ========== В7-84 ОТКЛЮЧЕН ===========   ");
        }

        public async Task<MeasureResult> Measure(bool returnNegativeValue = false)
        {
            DateTime startTime = DateTime.Now;
            MeasureResult measureResult = await Task.Run(async () =>
            {
                List<double> tempData = new List<double>();
                do
                {
                    string data = serialPort.ReadLine().Replace(',', '.').Trim();

                    if (!inputData.IsMatch(data))
                    {
                        log.Debug($"В7-84 -> ПРИНЯТО НЕКОРРЕКТНОЕ ЗНАЧЕНИЕ НАПРЯЖЕНИЯ, В: '{data}'");
                        using (StreamWriter sw = new StreamWriter("InputDataErrors.txt", true, System.Text.Encoding.Unicode))
                        {
                            await sw.WriteAsync(data.Trim() + "|");
                        }
                        continue;
                    }

                    double measuredValue = double.Parse(data.Split(' ')[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture);
                    log.Debug($"В7-84 -> получена строка: '{data}', преобразованное значение напряжения, В: '{measuredValue:F7}'");
                    tempData.Add(measuredValue);
                }
                while (DateTime.Now.Subtract(startTime).TotalMilliseconds < averagingTimeMillis);

                double averageValue = tempData.Average();
                log.Debug($"В7-84 -> осредненное значение напряжения, В: '{averageValue:F7}', " +
                    $"время осреднения, мс: '{averagingTimeMillis}', значений, шт: '{tempData.Count}'");
                return new MeasureResult(averageValue);
            });
            return measureResult;
        }
    }
}
