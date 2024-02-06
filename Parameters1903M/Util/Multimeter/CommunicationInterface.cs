using System.ComponentModel;

namespace Parameters1903M.Util.Multimeter
{
    internal enum CommunicationInterface
    {
        [Description("Эмулятор")] Emulator, //Отладка с полной эмуляцией работы программы
        [Description("В2-43")] V2_43, //В2-43
        [Description("В7-84")] V7_84 //В7-78
    }
}
