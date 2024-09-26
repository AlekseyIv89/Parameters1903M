using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.View;
using Parameters1903M.View.TSE1903M;
using System;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov13_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov13_WindowService prov13_WindowService;
        public ICommand Prov13_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov13_Model Prov13_Model { get; private set; }
        private Prov13_Window ProvWindow { get => prov13_WindowService.GetProvWindow(); }

        public Prov13_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov13_Model = new Prov13_Model(Parameter);

            prov13_WindowService = new Prov13_WindowService();

            Prov13_WindowCloseCommand = new RelayCommand(param => prov13_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',')[0];
                MessageBoxResult mbr;

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Prov13_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov13_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, замкните ОС.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 5; i++)
                    {
                        message = "Поверните призму с изделием на 90° в сторону выходной колодки.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        if (i == 0)
                        {
                            message = "В цепь обратной связи последовательно включить магазин сопротивлений типа Р33 и " +
                                "установить на магазине сопротивление, обеспечивающее максимальный ток в рамке Iосmax = 25 мА.";
                            mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                                , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                            if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);
                        }

                        TimeSpan timeBetweenMeasurements = new TimeSpan(0, 0, 10);
                        if (GlobalVars.IsDebugEnabled)
                        {
                            timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                        }
                        TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                        if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        message = "Поверните  изделие за (1 – 2) с в исходное положение и " +
                            "определите время от окончания поворота до момента, " +
                            "когда величина выходного напряжения ДУ (Uду) будет находиться в пределах ±200 мВ";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение времени";
                        new InputDialogWindowService().OpenDialog(Parameter, label, message);
                        Prov13_Model.InitialData[i * 2].Position0Value = GlobalVars.InputDialogValue;

                        message = "Поверните призму с изделием на 90° в сторону, противоположную выходной колодке.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        timeBetweenMeasurements = new TimeSpan(0, 0, 10);
                        if (GlobalVars.IsDebugEnabled)
                        {
                            timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                        }
                        timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                        if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        message = "Поверните  изделие за (1 – 2) с в исходное положение и " +
                            "определите время от окончания поворота до момента, " +
                            "когда величина выходного напряжения ДУ (Uду) будет находиться в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение времени";
                        new InputDialogWindowService().OpenDialog(Parameter, label, message);
                        Prov13_Model.InitialData[i * 2 + 1].Position0Value = GlobalVars.InputDialogValue;
                    }

                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в положение, " +
                        "повернутое относительно исходного на 180º вокруг ОЧ (положение 180).";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 5; i++)
                    {
                        message = "Поверните призму с изделием на 90° в сторону выходной колодки.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        TimeSpan timeBetweenMeasurements = new TimeSpan(0, 0, 10);
                        if (GlobalVars.IsDebugEnabled)
                        {
                            timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                        }
                        TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                        if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        message = "Поверните  изделие за (1 – 2) с в положение 180 и " +
                            "определите время от окончания поворота до момента, " +
                            "когда величина выходного напряжения ДУ (Uду) будет находиться в пределах ±200 мВ";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение времени";
                        new InputDialogWindowService().OpenDialog(Parameter, label, message);
                        Prov13_Model.InitialData[i * 2].Position180Value = GlobalVars.InputDialogValue;

                        message = "Поверните призму с изделием на 90° в сторону, противоположную выходной колодке.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        timeBetweenMeasurements = new TimeSpan(0, 0, 10);
                        if (GlobalVars.IsDebugEnabled)
                        {
                            timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                        }
                        timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                        if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        message = "Поверните  изделие за (1 – 2) с в исходное положение и " +
                            "определите время от окончания поворота до момента, " +
                            "когда величина выходного напряжения ДУ (Uду) будет находиться в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение времени";
                        new InputDialogWindowService().OpenDialog(Parameter, label, message);
                        Prov13_Model.InitialData[i * 2 + 1].Position180Value = GlobalVars.InputDialogValue;
                    }

                    Prov13_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov13_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov13_WindowService.StopMeasure();
            }
        }
    }
}
