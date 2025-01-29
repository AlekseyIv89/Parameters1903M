using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.View.TSE1903M;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov18_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov18_WindowService prov18_WindowService;
        public ICommand Prov18_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov18_Model Prov18_Model { get; private set; }
        private Prov18_Window ProvWindow { get => prov18_WindowService.GetProvWindow(); }

        public Prov18_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov18_Model = new Prov18_Model(Parameter);

            prov18_WindowService = new Prov18_WindowService();

            Prov18_WindowCloseCommand = new RelayCommand(param => prov18_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private void ParameterMeasure()
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
                        Prov18_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov18_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, замкните ОС.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Наклоном плиты выставите значение ТОС в пределах (15 ± 0,01) мА.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Поверните призму с изделием вокруг ОМ на 90º в сторону выходной колодки, " +
                            "включите последовательно цепи обратной связи магазин сопротивления типа Р33 и " +
                            "установите на магазине сопротивление обеспечивающее ТОС Iосmах в пределах (25 ± 0,05) мА, " +
                            "разорвите ОС, " +
                            "поверните призму с изделием обратно вокруг ОМ на 90º в сторону противоположную выходной колодке так, " +
                            "чтобы ПС оставалась на положительном упоре.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Замкните ОС и замерьте в секундах время t1, " +
                            "в течение которого с момента включения тумблера \"Iос\" по прибору Uду установится величина " +
                            "в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное время в секундах.";
                        string title = "Время схода ПС с упора (наклон в \"+\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov18_Model.InitialData[i].InclinePlusValue = GlobalVars.InputDialogValue;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Поверните призму с изделием вокруг ОМ на 90º в сторону противоположную выходной колодке, " +
                            "включите последовательно цепи обратной связи магазин сопротивления типа Р33 и " +
                            "установите на магазине сопротивление обеспечивающее ТОС Iосmах в пределах минус (25 ± 0,05) мА, " +
                            "разорвите ОС, " +
                            "поверните призму с изделием обратно вокруг ОМ на 90º в сторону выходной колодки так, " +
                            "чтобы ПС оставалась на отрицательном упоре.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Замкните ОС и замерьте в секундах время t4, " +
                            "в течение которого с момента включения тумблера \"Iос\" по прибору Uду установится величина " +
                            "в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное время в секундах.";
                        string title = "Время схода ПС с упора (наклон в \"-\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov18_Model.InitialData[i].InclineMinusValue = GlobalVars.InputDialogValue;
                    }

                    Prov18_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov18_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov18_WindowService.StopMeasure();
            }
        }
    }
}
