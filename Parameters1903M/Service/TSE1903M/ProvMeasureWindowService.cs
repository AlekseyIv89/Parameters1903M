using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using System.Threading;

namespace Parameters1903M.Service.TSE1903M
{
    internal class ProvMeasureWindowService : IStartStopMeasure
    {
        private CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get => TokenSource.Token; }

        public IMeasure Multimeter { get; private set; }

        public void StartMeasure()
        {
            Multimeter = new MainWindowService().GetMultimeter();

            GlobalVars.IsMeasureRunning = true;
            TokenSource = new CancellationTokenSource();
        }

        public void StopMeasure()
        {
            TokenSource.Cancel();
            GlobalVars.IsMeasureRunning = false;
        }
    }
}
