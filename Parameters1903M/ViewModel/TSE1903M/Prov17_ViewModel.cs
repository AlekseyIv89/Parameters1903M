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

        public Prov17_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov17_Model = new Prov17_Model(Parameter);

            prov17_WindowService = new Prov17_WindowService();

            Prov17_WindowCloseCommand = new RelayCommand(param => prov17_WindowService.Close(param), x => true);
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
                    string message = "Установить призму с изделием на выставленную в горизонт поверочную плиту в исходное положение." + Environment.NewLine;
                    message += "Замкнуть ОС";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Наклоном плиты выставить значение ТОС Jосmах в пределах плюс (28,0 ± 0,5) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "В цепь обратной связи включить последовательно магазин сопротивления типа Р33, ";
                        message += "установить на магазине сопротивление, обеспечивающее ТОС Jосmах в пределах плюс (25,00 ± 0,05) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разомкнуть на 5 секунд и замкнуть ОС.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Плавно вернуть плиту в исходное положение до момента начала замыкания ОС. ";
                        message += "После замыкания ОС записать величину ТОС в графу «наклон в «+».";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение ТОС";
                        string title = "Ток отрыва ПС от упора (наклон в \"+\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov17_Model.InitialData[i].InclinePlusValue = GlobalVars.InputDialogValue;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        message = "Наклоном плиты выставить значение ТОС Jосmах в пределах минус (28,0 ± 0,5) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "В цепь обратной связи включить последовательно магазин сопротивления типа Р33, ";
                        message += "установить на магазине сопротивление, обеспечивающее ТОС Jосmах в пределах минус (25,00 ± 0,05) мА.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Разомкнуть на 5 секунд и замкнуть ОС.";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Плавно вернуть плиту в исходное положение до момента начала замыкания ОС. ";
                        message += "После замыкания ОС записать величину ТОС в графу «наклон в «-».";
                        mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                            , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                        message = "Введите измеренное значение ТОС";
                        string title = "Ток отрыва ПС от упора (наклон в \"-\")";
                        new InputDialogWindowService().OpenDialog(Parameter, title, message);
                        Prov17_Model.InitialData[i].InclineMinusValue = GlobalVars.InputDialogValue;
                    }

                    Prov17_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
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
