using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View.TSE1903M;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov17_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov17_WindowService prov17_WindowService;
        public ICommand Prov17_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov17_Model Prov17_Model { get; private set; }
        private Prov17_Window ProvWindow { get => prov17_WindowService.GetProvWindow(); }

        private IMeasure Multimeter { get => prov17_WindowService.Multimeter; }

        public Prov17_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov17_Model = new Prov17_Model(Parameter);

            prov17_WindowService = new Prov17_WindowService();

            Prov17_WindowCloseCommand = new RelayCommand(param => prov17_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',').First();
                MessageBoxResult mbr;

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Prov17_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov17_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, замкните ОС.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    Multimeter.SetAverageTimeMillis(4_000);

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Наклоном плиты выставите значение ТОС Jосmах в пределах (28±0,5) мА. " +
                            "В цепь обратной связи включите последовательно магазин сопротивления типа Р33 и " +
                            "установите на магазине сопротивление, обеспечивающее ТОС Iосmах в пределах (25 ± 0,05) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разомкните на 5 секунд и замкните ОС и плавно возвращайте плиту в исходное положение " +
                            "до момента начала замыкания ОС (показания по прибору Uду начинают изменяться к нулю).";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        await Task.Run(() =>
                        {
                            // Первую точку пропускаем
                            double missingValue = Multimeter.Measure().Result.Value;

                            MeasureResult result = Multimeter.Measure().Result;
                            Prov17_Model.InitialData[i].InclinePlusValue = Converter.ConvertVoltToMilliAmpere(result.Value);

                            if (prov17_WindowService.Token.IsCancellationRequested) return;
                        }, prov17_WindowService.Token);
                        if (prov17_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                        message = "Установите на магазине Р33 сопротивление 0 Ом.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);
                    }

                    Prov17_Model.CalculateData();

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Наклоном плиты выставите значение ТОС Jосmах в пределах минус (28±0,5) мА. " +
                            "В цепь обратной связи включите последовательно магазин сопротивления типа Р33 и " +
                            "установите на магазине сопротивление, обеспечивающее ТОС Iосmах в пределах минус (25 ± 0,05) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разомкните на 5 секунд и замкните ОС и плавно возвращайте плиту в исходное положение " +
                            "до момента начала замыкания ОС (показания по прибору Uду начинают изменяться к нулю).";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        await Task.Run(() =>
                        {
                            // Первую точку пропускаем
                            double missingValue = Multimeter.Measure().Result.Value;

                            MeasureResult result = Multimeter.Measure().Result;
                            Prov17_Model.InitialData[i].InclineMinusValue = Converter.ConvertVoltToMilliAmpere(result.Value);

                            if (prov17_WindowService.Token.IsCancellationRequested) return;
                        }, prov17_WindowService.Token);
                        if (prov17_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                        message = "Установите на магазине Р33 сопротивление 0 Ом.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);
                    }

                    Prov17_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov17_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov17_WindowService.StopMeasure();
            }
        }
    }
}
