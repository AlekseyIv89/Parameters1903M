using Parameters1903M.Model;
using Parameters1903M.View;
using Parameters1903M.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class TimerWindowService : IWindowService
    {
        private TimerEvent MyTimerEvent = TimerEvent.Cancelled;

        public TimerWindow GetTimerWindow() => Application.Current.Windows.OfType<TimerWindow>().FirstOrDefault();
        private TimerViewModel GetTimerViewModel() => (TimerViewModel)GetTimerWindow().DataContext;

        public void Close(object param)
        {
            string message;
            TimerWindow timerWindow = GetTimerWindow();

            switch (MyTimerEvent)
            {
                case TimerEvent.Finished:
                    timerWindow.DialogResult = true;
                    break;
                case TimerEvent.Cancelled:
                    //if (timerWindow == null) break;
                    message = "Вы хотите закрыть окно с таймером обратного отсчета?" + Environment.NewLine + Environment.NewLine;
                    message += "ВНИМАНИЕ: Если нажмете \"Да\" закроется окно с таймером и текущая проверка завершится.";
                    MessageBoxResult result = MessageBox.Show(timerWindow, message, GetTimerViewModel().Title, MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        GetTimerViewModel().StopTimer();
                    }
                    else
                    {
                        CancelEventArgs cancelEventArgs = (CancelEventArgs)param;
                        cancelEventArgs.Cancel = true;
                    }
                    break;
                case TimerEvent.UserChangedSystemTime:
                    message = "Обнаружено изменение системного времени для воздействия на таймер обратного отсчета." + Environment.NewLine;
                    message += "Проверка параметра отменена.";
                    MessageBox.Show(timerWindow, message, GetTimerViewModel().Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
                    
            }
        }

        public void OpenDialog(object param)
        {
            TimeSpan timeSpan = (TimeSpan)param;
            TimerWindow window = Application.Current.Windows.OfType<TimerWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new TimerWindow(timeSpan) { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }

        public void TimerCancelled()
        {
            MyTimerEvent = TimerEvent.Cancelled;
            Close(new CancelEventArgs());
        }

        public void TimerFinished()
        {
            MyTimerEvent = TimerEvent.Finished;
            Close(null);
        }

        public void UserChangedSystemTime()
        {
            MyTimerEvent = TimerEvent.UserChangedSystemTime;
            Close(null);
        }
    }
}
