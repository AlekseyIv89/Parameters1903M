using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.View.TSE1903M;
using System;
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
                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    string message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    string label = Parameter.Name.Split(',')[0];

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
                    string message = "Установить призму с изделием на выставленную в горизонт поверочную плиту в исходное положение." + Environment.NewLine;
                    message += "Замкнуть ОС";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Наклоном плиты выставить значение ТОС в пределах плюс (15,00 ± 0,01) мА.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Повернуть призму с изделием на угол 90° в сторону выходной колодки.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "В цепь обратной связи включить последовательно магазин сопротивления типа Р33, ";
                        message += "установить на магазине сопротивление, обеспечивающее ТОС Jосmах в пределах плюс (25,00 ± 0,05) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разорвать ОС.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Повернуть призму с изделием обратно на угол 90° так, чтобы ПС оставалось на положительном упоре.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Замкнуть ОС (тумблер Iос) и записать в секундах время в графу «наклон в «+»», ";
                        message += "в течение которого с момента включения тумблера \"Iос\" по прибору Uду установиться величина в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное время в секундах";
                        string title = "Время схода ПС с упора (наклон в \"+\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov18_Model.InitialData[i].InclinePlusValue = GlobalVars.InputDialogValue;
                    }

                    message = "Наклоном плиты выставить значение ТОС в пределах минус (15,00 ± 0,01) мА.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Повернуть призму с изделием на минус 90° выходной колодкой вверх.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Установить на магазине сопротивление, обеспечивающее ТОС Jосmах в пределах минус (25,00 ± 0,05) мА";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разорвать ОС.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Повернуть призму с изделием обратно на угол 90° так, чтобы ПС оставалось на отрицательном упоре.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Замкнуть ОС (тумблер Iос) и записать в секундах время в графу «наклон в «-»», ";
                        message += "в течение которого с момента включения тумблера \"Iос\" по прибору Uду установиться величина в пределах ±200 мВ.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное время в секундах";
                        string title = "Время схода ПС с упора (наклон в \"+\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov18_Model.InitialData[i].InclineMinusValue = GlobalVars.InputDialogValue;
                    }

                    Prov18_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
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
