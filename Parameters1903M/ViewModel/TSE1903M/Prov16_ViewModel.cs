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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
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
                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    string message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    string label = Parameter.Name.Split(',')[0];

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
                    string message = "Установить призму на выставленную в горизонт поверочную плиту в исходное положение.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Повернуть изделие с призмой вокруг ОЧ на 90° в сторону маятника.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Соединить перемычками вход вольтметра к клеммам ДУ и ЭКРАН, предварительно отключив его от клемм ±U."
                        + Environment.NewLine;
                    message += "С помощью переключателя ВХОД-ЭКВ и тумблера IОС-РАЗР.ОС разорвать ОС.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeBetweenMeasurements = new TimeSpan(0, 10, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov16_WindowService.Multimeter.Measure(true).Result;
                            Prov16_Model.InitialData[i].UdyValue = Converter.ConvertVoltToMilliVolt(result.Value);

                            if (prov16_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov16_WindowService.Token);
                    if (prov16_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov16_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
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
