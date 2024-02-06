using Parameters1903M.Model;
using Parameters1903M.Service;
using Parameters1903M.Service.Command;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class TimerViewModel : BaseViewModel
    {
        public string Title => "Таймер обратного отсчета";

        private TimeSpan timeSpan;

        private const double deltaTimeToCheck = 9999F;

        public bool IsTimerStoppedByUser { get; private set; }

        public bool IsUserChangedSystemTime { get; private set; }

        public bool IsRunning { get; private set; }

        public ICommand TimerWindowCloseCommand { get; }
        public ICommand TimerWindowLoadedCommand { get; }

        public ICommand TimerFinishedCommand { get; }
        public ICommand TimerCancelledCommand { get; }
        public ICommand TimerUserChangedSystemTimeCommand { get; }

        public TimerModel TimerModel { get; private set; }
        private readonly TimerWindowService timerWindowService;

        public TimerViewModel(TimeSpan timeSpan)
        {
            TimerModel = new TimerModel();
            timerWindowService = new TimerWindowService();

            this.timeSpan = timeSpan;

            TimerWindowCloseCommand = new RelayCommand(param => timerWindowService.Close(param), x => true);
            TimerWindowLoadedCommand = new RelayCommand(param => StartTimer(this.timeSpan), x => true);

            TimerFinishedCommand = new RelayCommand((param) => timerWindowService.TimerFinished(), x => true);
            TimerCancelledCommand = new RelayCommand((param) => timerWindowService.TimerCancelled(), x => true);
            TimerUserChangedSystemTimeCommand = new RelayCommand((param) => timerWindowService.UserChangedSystemTime(), x => true);
        }

        /// <summary>
        /// Запуск таймера
        /// </summary>
        /// <param name="startTimeSpan">Значение времени с которого начнется обратный отсчет</param>
        public async void StartTimer(TimeSpan startTimeSpan)
        {
            DateTime dt = DateTime.Now.Add(startTimeSpan);

            int i = 0;
            double lastMillis = default;
            double currentMillis = default;

            while (dt.Subtract(DateTime.Now).TotalSeconds > 0 && !IsTimerStoppedByUser)
            {
                TimerModel.Time = dt.Subtract(DateTime.Now).ToString("hh\\:mm\\:ss");
                await Task.Delay(250);

                if (i > 0)
                {
                    currentMillis = dt.Subtract(DateTime.Now).TotalMilliseconds;
                    if ((lastMillis - currentMillis > deltaTimeToCheck) || (currentMillis - lastMillis > deltaTimeToCheck))
                    {
                        IsUserChangedSystemTime = true;
                        break;
                    }
                }

                lastMillis = dt.Subtract(DateTime.Now).TotalMilliseconds;
                i++;
            }

            if (IsUserChangedSystemTime) TimerUserChangedSystemTimeCommand.Execute(null);
            else if (IsTimerStoppedByUser) TimerCancelledCommand.Execute(null);
            else TimerFinishedCommand.Execute(null);
        }

        /// <summary>
        /// Остановка таймера
        /// </summary>
        public void StopTimer()
        {
            IsTimerStoppedByUser = true;
        }
    }
}
