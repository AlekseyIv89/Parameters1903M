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
    internal class Prov2_ViewModel : BaseViewModel
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

        private readonly Prov2_WindowService prov2_WindowService;
        public ICommand Prov2_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }
        public ICommand ButtonContinueCommand { get; }

        public Prov2_Model Prov2_Model { get; private set; }
        private Prov2_Window ProvWindow { get => prov2_WindowService.GetProvWindow(); }

        public Prov2_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;
            BtnContinueVisibility = Visibility.Hidden;

            Prov2_Model = new Prov2_Model(Parameter);

            prov2_WindowService = new Prov2_WindowService();

            Prov2_WindowCloseCommand = new RelayCommand(param => prov2_WindowService.Close(param), x => true);
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
                        Prov2_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov2_WindowService.StartMeasure();

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
                    message += "Отрегулировать напряжение на выходе ДУ в пределах ±10мВ.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Отклоните плиту на угол +5 угл.мин. или -5 угл.мин.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    BtnContinueVisibility = Visibility.Visible;

                    await Task.Run(() =>
                    {
                        while (!ContinueFlag)
                        {
                            MeasureResult result = prov2_WindowService.Multimeter.Measure().Result;
                            if (result.Result >= .0)
                            {
                                Prov2_Model.InitialData.Udy1Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }
                            else
                            {
                                Prov2_Model.InitialData.Udy2Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }

                            if (prov2_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov2_WindowService.Token);
                    if (prov2_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    ContinueFlag = false;

                    if (string.IsNullOrWhiteSpace(Prov2_Model.InitialData.Udy2ValueStr))
                        message = "Отклоните плиту на угол -5 угл.мин.";
                    else message = "Отклоните плиту на угол +5 угл.мин.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        while (!ContinueFlag)
                        {
                            MeasureResult result = prov2_WindowService.Multimeter.Measure(true).Result;
                            if (result.Result >= .0)
                            {
                                Prov2_Model.InitialData.Udy1Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }
                            else
                            {
                                Prov2_Model.InitialData.Udy2Value = Converter.ConvertVoltToMilliVolt(result.Result);
                            }

                            if (prov2_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov2_WindowService.Token);
                    if (prov2_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov2_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov2_WindowService.StopMeasure();

                ContinueFlag = false;
                BtnContinueVisibility = Visibility.Hidden;
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ContinueFlag = true;

                ButtonContent = BUTTON_START;
                prov2_WindowService.StopMeasure();

                ContinueFlag = false;
                BtnContinueVisibility = Visibility.Hidden;
            }
        }
    }
}
