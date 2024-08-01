using System.Threading.Tasks;

namespace Parameters1903M.Util.Multimeter
{
    internal interface IMeasure
    {
        /// <summary>
        /// Подключение к мультиметру для обмена информацией
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        CommunicationInterface Connect(string address);

        /// <summary>
        /// Отправка команды на мультиметр
        /// </summary>
        /// <param name="command">Команда</param>
        void SendCommand(string command);

        /// <summary>
        /// Установка времени осреднения мультиметра
        /// </summary>
        /// <param name="averagingTime">Время осреднения в миллисекундах</param>
        void SetAverageTimeMillis(double averagingTime);

        /// <summary>
        /// Сброс значения осреднения
        /// Должен использоваться в конструкторе ViewModel каждой проверки, которая производит обмен данными с мультиметром
        /// </summary>
        void ResetAverageTime();

        /// <summary>
        /// Измерение значения с мультиметра
        /// </summary>
        /// <param name="returnNegativeValue"></param>
        /// <returns>Результат измерения в вольтах</returns>
        Task<MeasureResult> Measure(bool returnNegativeValue = false);

        /// <summary>
        /// Отключение от мультиметра
        /// </summary>
        void Disconnect();
    }
}
