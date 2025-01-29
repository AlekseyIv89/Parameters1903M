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
    internal class Prov5_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov5_WindowService prov5_WindowService;
        public ICommand Prov5_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }
        public ICommand ButtonContinueCommand { get; }

        public Prov5_Model Prov5_Model { get; private set; }
        private Prov5_Window ProvWindow { get => prov5_WindowService.GetProvWindow(); }

        public Prov5_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov5_Model = new Prov5_Model(Parameter);

            prov5_WindowService = new Prov5_WindowService();

            Prov5_WindowCloseCommand = new RelayCommand(param => prov5_WindowService.Close(param), x => true);
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
                        Prov5_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov5_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Соедините перемычками клемму «Hi» мультиметра c клеммой «ДУ» блока ВУ-23 и клемму «Lo» мультиметра с клеммой «Экран», " +
                        "предварительно отключив мультиметр от штатных клемм измерительной стойки/";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение и " +
                        "подключите к рабочему месту в режиме измерения выходного напряжения UДУ, разорвите ОС.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Наклоните плоскость поверочной плиты от исходного положения на угол плюс (20 ± 1) угл.мин в сторону выходной колодки изделия.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Выдержите изделие в наклонном положении до установившегося в десятимилливольтовом разряде значения выходного сигнала с ДУ.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    prov5_WindowService.Multimeter.SetAverageTimeMillis(2_500);

                    TimeSpan timeSpan = new TimeSpan(0, 0, 10);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeSpan = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        double missingValue = prov5_WindowService.Multimeter.Measure().Result.Value;

                        MeasureResult result = prov5_WindowService.Multimeter.Measure().Result;
                        Prov5_Model.InitialData.Udy1Value = Converter.ConvertVoltToMilliVolt(result.Value);

                        if (prov5_WindowService.Token.IsCancellationRequested) return;
                    }, prov5_WindowService.Token);
                    if (prov5_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov5_Model.CalculateData();

                    message = "Наклоните плоскость поверочной плиты от исходного положения на угол минус (20 ± 1) угл.мин в сторону противоположную выходной колодке изделия.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Выдержите изделие в наклонном положении до установившегося в десятимилливольтовом разряде значения выходного сигнала с ДУ.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        double missingValue = prov5_WindowService.Multimeter.Measure().Result.Value;

                        MeasureResult result = prov5_WindowService.Multimeter.Measure(true).Result;
                        Prov5_Model.InitialData.Udy2Value = Converter.ConvertVoltToMilliVolt(result.Value);

                        if (prov5_WindowService.Token.IsCancellationRequested) return;
                    }, prov5_WindowService.Token);
                    if (prov5_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov5_Model.CalculateData();

                    message = "Выставить поверочную плиту в горизонт" + Environment.NewLine;
                    message += "Восстановить схему подключения вольтметра к клеммам ±U";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov5_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov5_WindowService.StopMeasure();
            }
        }
    }
}
