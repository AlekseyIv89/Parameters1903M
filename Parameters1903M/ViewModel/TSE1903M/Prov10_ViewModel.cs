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
    // Проверка непараллельности ОЧ базовой плоскости (φ 0,  φ180 )
    internal class Prov10_ViewModel: BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov10_WindowService prov10_WindowService;
        public ICommand Prov10_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov10_Model Prov10_Model { get; private set; }
        private Prov10_Window ProvWindow { get => prov10_WindowService.GetProvWindow(); }

        private IMeasure Multimeter { get => prov10_WindowService.Multimeter; }

        public Prov10_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov10_Model = new Prov10_Model(Parameter);

            prov10_WindowService = new Prov10_WindowService();

            Prov10_WindowCloseCommand = new RelayCommand(param => prov10_WindowService.Close(param), x => true);
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
                        Prov10_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov10_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, " +
                        "замкните  ОС и накройте призму с изделием кожухом.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeSpan = new TimeSpan(0, 0, 10);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeSpan = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
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
                            Prov10_Model.InitialData[i].I0Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov10_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov10_WindowService.Token);
                    if (prov10_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov10_Model.CalculateData();

                    message = "Повернуть призму с изделием на 180° в сторону маятника вокруг ОЧ.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        double missingValue = Multimeter.Measure().Result.Value;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = Multimeter.Measure(true).Result;
                            Prov10_Model.InitialData[i].I180Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov10_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov10_WindowService.Token);
                    if (prov10_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov10_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov10_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov10_WindowService.StopMeasure();
            }
        }
    }
}
