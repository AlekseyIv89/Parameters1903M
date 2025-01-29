using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View;
using Parameters1903M.View.TSE1903M;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    // Проверка неперпендикулярности ОЧ опорной плоскости (∆m).
    internal class Prov16_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov16_WindowService prov16_WindowService;
        public ICommand Prov16_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov16_Model Prov16_Model { get; private set; }
        private Prov16_Window ProvWindow { get => prov16_WindowService.GetProvWindow(); }

        private IMeasure Multimeter { get => prov16_WindowService.Multimeter; }

        public Prov16_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov16_Model = new Prov16_Model(Parameter);

            prov16_WindowService = new Prov16_WindowService();

            Prov16_WindowCloseCommand = new RelayCommand(param => prov16_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',').First();

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";

                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Prov16_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov16_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Соедините  перемычками клемму «Hi» мультиметра c клеммой «ДУ» блока ВУ-23 и клемму «Lo» мультиметра с клеммой «Экран», " +
                        "предварительно отключив мультиметр от штатных клемм измерительной стойки.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение и " +
                        "подключите изделие к стойке в режиме измерения выходного напряжения UДУ и его переменной составляющей.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Поверните призму с изделием на угол 90° в сторону маятника вокруг оси чувствительности.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeBetweenMeasurements = new TimeSpan(0, 0, 10);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    int averageTimeInMillis = 4_000;
                    if (GlobalVars.IsDebugEnabled)
                    {
                        averageTimeInMillis = 2_000;
                    }

                    Multimeter.ResetAverageTime();
                    Multimeter.SetAverageTimeMillis(averageTimeInMillis);

                    await Task.Run(() =>
                    {
                        double missingValue = Multimeter.Measure().Result.Value;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = Multimeter.Measure().Result;
                            Prov16_Model.InitialData[i].UdyValue = Converter.ConvertVoltToMilliVolt(result.Value);

                            if (prov16_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov16_WindowService.Token);
                    if (prov16_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov16_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov16_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov16_WindowService.StopMeasure();
            }
        }
    }
}
