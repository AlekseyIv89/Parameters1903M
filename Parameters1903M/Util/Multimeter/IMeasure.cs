using System.Threading.Tasks;

namespace Parameters1903M.Util.Multimeter
{
    internal interface IMeasure
    {
        CommunicationInterface Connect(string address);
        void SendCommand(string command);
        void SetAverageTimeMillis(double averagingTime);
        Task<MeasureResult> Measure(bool returnNegativeValue = false);
        void Disconnect();
    }
}
