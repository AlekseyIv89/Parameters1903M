using Parameters1903M.Model;
using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View.TSE1903M;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Parameters1903M.Service.TSE1903M
{
    internal class Prov0_WindowService : IWindowService, IStartStopMeasure
    {
        public Prov0_Window GetProvWindow() => Application.Current.Windows.OfType<Prov0_Window>().FirstOrDefault();

        private CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get => TokenSource.Token; }

        public IMeasure Multimeter { get; private set; }

        public void Close(object param)
        {
            if (GlobalVars.IsMeasureRunning)
            {
                CancelEventArgs cancelEventArgs = (CancelEventArgs)param;
                cancelEventArgs.Cancel = true;

                string message = "Невозможно закрыть окно с проверкой, т.к. в настоящее время проводится измерение параметра.";
                MessageBox.Show(Application.Current.MainWindow, message, ProgramInfo.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            GlobalVars.IsProvWindowOpened = false;
        }

        public void OpenDialog(object param)
        {
            if (param is Parameter parameter)
            {
                Prov0_Window window = Application.Current.Windows.OfType<Prov0_Window>().FirstOrDefault();
                if (window == null)
                {
                    window = new Prov0_Window(parameter) { Owner = Application.Current.MainWindow };
                }
                window.ShowDialog();

                GlobalVars.IsProvWindowOpened = true;
            }
        }

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
