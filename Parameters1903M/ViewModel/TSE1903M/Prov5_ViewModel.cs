using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View.TSE1903M;
using System;
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

        private Visibility btnContinueVisibility;
        public Visibility BtnContinueVisibility
        {
            get => btnContinueVisibility;
            private set
            {
                btnContinueVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool ContinueFlag = false;

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
            ButtonContinueCommand = new RelayCommand(param => ContinueFlag = true, x => true);
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
                    ContinueFlag = false;

                    string message = "Соединить перемычками вход вольтметра к клеммам ДУ и ЭКРАН, предварительно отключив его от клемм ±U";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Выставить плиту в горизонт." + Environment.NewLine;
                    message += "Установить прибор в положение маятником вниз." + Environment.NewLine;
                    message += "С помощью переключателя ВХОД-ЭКВ и тумблера IОС-РАЗР.ОС разорвать ОС." + Environment.NewLine;
                    message += "Отрегулировать напряжение на выходе ДУ";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Наклонить плоскость поверочной плиты на угол +(20±1) угл.мин. или -(20±1) угл.мин.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    BtnContinueVisibility = Visibility.Visible;

                    await Task.Run(() =>
                    {
                        while (!ContinueFlag)
                        {
                            MeasureResult result = prov5_WindowService.Multimeter.Measure().Result;
                            if (result.Result >= .0)
                            {
                                Prov5_Model.InitialData.Udy1Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }
                            else
                            {
                                Prov5_Model.InitialData.Udy2Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }

                            if (prov5_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov5_WindowService.Token);
                    if (prov5_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    ContinueFlag = false;

                    if (string.IsNullOrWhiteSpace(Prov5_Model.InitialData.Udy2ValueStr))
                        message = "Наклонить плоскость поверочной плиты на угол -(20±1) угл.мин.";
                    else message = "Наклонить плоскость поверочной плиты на угол +(20±1) угл.мин.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        while (!ContinueFlag)
                        {
                            MeasureResult result = prov5_WindowService.Multimeter.Measure(true).Result;
                            if (result.Result >= .0)
                            {
                                Prov5_Model.InitialData.Udy1Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }
                            else
                            {
                                Prov5_Model.InitialData.Udy2Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }

                            if (prov5_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov5_WindowService.Token);
                    if (prov5_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov5_Model.CalculateData();

                    message = "Выставить поверочную плиту в горизонт" + Environment.NewLine;
                    message += "Восстановить схему подключения вольтметра к клеммам ±U";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov5_WindowService.StopMeasure();

                ContinueFlag = false;
                BtnContinueVisibility = Visibility.Hidden;
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ContinueFlag = true;

                ButtonContent = BUTTON_START;
                prov5_WindowService.StopMeasure();

                ContinueFlag = false;
                BtnContinueVisibility = Visibility.Hidden;
            }
        }
    }
}
