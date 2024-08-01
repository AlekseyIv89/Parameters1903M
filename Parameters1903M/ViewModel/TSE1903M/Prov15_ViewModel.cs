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
    internal class Prov15_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        private string textMessage1;
        public string TextMessage1
        {
            get => textMessage1;
            set
            {
                textMessage1 = value;
                if (string.IsNullOrEmpty(value))
                {
                    TextMessage1Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextMessage1Visibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private Visibility textMessage1Visibility = Visibility.Collapsed;
        public Visibility TextMessage1Visibility
        {
            get => textMessage1Visibility;
            private set
            {
                textMessage1Visibility = value;
                OnPropertyChanged();
            }
        }

        private string textMessage2;
        public string TextMessage2
        {
            get => textMessage2;
            set
            {
                textMessage2 = value;

                if (string.IsNullOrEmpty(value))
                {
                    TextMessage2Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextMessage2Visibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private Visibility textMessage2Visibility = Visibility.Collapsed;
        public Visibility TextMessage2Visibility
        {
            get => textMessage2Visibility;
            private set
            {
                textMessage2Visibility = value;
                OnPropertyChanged();
            }
        }

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov15_WindowService prov15_WindowService;
        public ICommand Prov15_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov15_Model Prov15_Model { get; private set; }
        private Prov15_Window ProvWindow { get => prov15_WindowService.GetProvWindow(); }

        public Prov15_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov15_Model = new Prov15_Model(Parameter);

            prov15_WindowService = new Prov15_WindowService();

            Prov15_WindowCloseCommand = new RelayCommand(param => prov15_WindowService.Close(param), x => true);
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
                        Prov15_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov15_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    string message = "Установить изделие в ";
                    if (true)
                    {
                        message += "приспособление ИА-ПО-ЦЕ1903";
                    }
                    else // TODO: при определении температурного коэффициента на этапе регулировки в ИА-Г-ЦЕ1903М
                    {
                        message += "обогревную призму ИА-ПО ЦЕ1906";
                    }
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Установить обогревную призму с изделием на поверочную плиту в исходное положение.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Установить на поверочную призму два уровня: основной и дополнительный." + Environment.NewLine;
                    message += "Дополнительный уровень установить на технологическую подставку, обеспечивающую наклон уровня на (4,0±0,5)°." + Environment.NewLine;
                    message += "Установочные плоскости уровней должны быть перпендикулярны грани поверочной плиты, вдоль которой расположены два регулировочных винта (домкраты)";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TextMessage1 = "Положение уровней на поверочной плите за все время определения \nтемпературного коэффициента должно быть неизменным";

                    message = "Выставить поверочную плиту в горизонт с погрешностью 1″." + Environment.NewLine;
                    message += "Контроль горизонтальности производить по основному уровню";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Включить УРТ на 30˚С." + Environment.NewLine;
                    message += "Выдержать изделие в обогревной призме 2 часа";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TextMessage2 = "Контролировать точность выставки поверочной плиты с погрешностью 1″";

                    TimeSpan timeBetweenMeasurements = new TimeSpan(2, 0, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    TextMessage2 = string.Empty;

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov15_WindowService.Multimeter.Measure(true).Result;
                            Prov15_Model.InitialData[i].I0T1Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov15_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov15_WindowService.Token);
                    if (prov15_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov15_Model.CalculateData();

                    message = "Наклонить изделие поворотом поверочной плиты на угол 4°." + Environment.NewLine;
                    message += "Контроль угла выставки плиты осуществлять по нулевому показанию дополнительного уровня";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timeBetweenMeasurements = new TimeSpan(0, 15, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 3);
                    }
                    timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov15_WindowService.Multimeter.Measure(true).Result;
                            Prov15_Model.InitialData[i].I4T1Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov15_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov15_WindowService.Token);
                    if (prov15_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov15_Model.CalculateData();

                    message = "Переключить УРТ на 50 °С и выдержать изделие 2 часа.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timeBetweenMeasurements = new TimeSpan(2, 0, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                    }
                    timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov15_WindowService.Multimeter.Measure(true).Result;
                            Prov15_Model.InitialData[i].I4T2Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov15_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov15_WindowService.Token);
                    if (prov15_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov15_Model.CalculateData();

                    message = "Наклонить изделие поворотом поверочной плиты исходное положение." + Environment.NewLine;
                    message += "Контроль выставки горизонтальности плиты осуществлять по нулевому показанию основного уровня";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name
                        , MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timeBetweenMeasurements = new TimeSpan(0, 15, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 3);
                    }
                    timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov15_WindowService.Multimeter.Measure(true).Result;
                            Prov15_Model.InitialData[i].I0T2Value = Converter.ConvertVoltToMicroAmpere(result.Value);

                            if (prov15_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov15_WindowService.Token);
                    if (prov15_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov15_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                TextMessage1 = string.Empty;
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov15_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov15_WindowService.StopMeasure();
            }
        }
    }
}
