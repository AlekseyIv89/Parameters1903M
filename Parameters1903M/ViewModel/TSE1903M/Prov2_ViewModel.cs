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
    internal class Prov2_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private string currMeasureText;
        public string CurrMeasureText
        {
            get => currMeasureText;
            set
            {
                currMeasureText = value;
                OnPropertyChanged();
            }
        }

        private Visibility currMeasureVisibility = Visibility.Collapsed;
        public Visibility CurrMeasureVisibility
        {
            get => currMeasureVisibility;
            set
            {
                currMeasureVisibility = value;
                OnPropertyChanged();
            }
        }

        private readonly Prov2_WindowService prov2_WindowService;
        public ICommand Prov2_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov2_Model Prov2_Model { get; private set; }
        private Prov2_Window ProvWindow { get => prov2_WindowService.GetProvWindow(); }

        public Prov2_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov2_Model = new Prov2_Model(Parameter);

            prov2_WindowService = new Prov2_WindowService();

            Prov2_WindowCloseCommand = new RelayCommand(param => prov2_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',')[0];

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
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
                    message = "Соедините  перемычками клемму «Hi» мультиметра c клеммой «ДУ» блока ВУ-23 и клемму «Lo» мультиметра с клеммой «Экран», " +
                        "предварительно отключив мультиметр от штатных клемм измерительной стойки.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение и " +
                        "подключите изделие к стойке в режиме измерения выходного напряжения UДУ и его переменной составляющей.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Поверните призму с изделием на угол 90° в сторону маятника вокруг оси чувствительности.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    prov2_WindowService.Multimeter.SetAverageTimeMillis(2_500);

                    bool vistavkaFlag = true;
                    do
                    {
                        //---------------------------- Начало "Вывод Uду на экран" ----------------------------
                        bool flag = true;
                        CurrMeasureVisibility = Visibility.Visible;
                        Task task = Task.Run(() =>
                        {
                            while (flag)
                            {
                                CurrMeasureText = $"{Converter.ConvertVoltToMilliVolt(prov2_WindowService.Multimeter.Measure().Result.Result):F2} мВ";
                            }
                        });

                        message = "Наклоном поверочной плиты установить выходное напряжение ДУ в пределах ±10 мВ (положение 0).";
                        mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                            , MessageBoxImage.Information);
                        if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                        message = "Нажмите кнопку \"ОК\" для измерения значения Uду0.";
                        mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                            , MessageBoxImage.Information);
                        if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                        flag = false;
                        task.Wait();
                        CurrMeasureVisibility = Visibility.Collapsed;
                        //---------------------------- Конец "Вывод Uду на экран" ----------------------------

                        TimerWindow timerWindow1 = new TimerWindow(new TimeSpan(0, 0, 3)) { Owner = ProvWindow };
                        if (timerWindow1.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        await Task.Run(() =>
                        {
                            double missingValue = prov2_WindowService.Multimeter.Measure().Result.Result;
                            missingValue = Converter.ConvertVoltToMilliVolt(prov2_WindowService.Multimeter.Measure().Result.Result);
                            
                            double checkCurrentMicroA = 10.0;
                            if (GlobalVars.IsDebugEnabled)
                            {
                                checkCurrentMicroA = double.MaxValue;
                            }

                            if (Math.Abs(missingValue) > checkCurrentMicroA)
                            {
                                message = "Uду не соответствует допуску ±10 мВ. Повторите выставку изделия.";
                                mbr = MessageBox.Show(message, label, MessageBoxButton.OKCancel
                                    , MessageBoxImage.Warning);
                                if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);
                            }
                            else vistavkaFlag = false;

                            Prov2_Model.CalculatedData.ZeroSduValue = missingValue;
                        });
                    }
                    while (vistavkaFlag);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол +5´ в сторону выходной колодки изделия относительно положения 0";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeSpan = new TimeSpan(0, 0, 10);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeSpan = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    prov2_WindowService.Multimeter.SetAverageTimeMillis(4_000);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = prov2_WindowService.Multimeter.Measure().Result.Result;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov2_WindowService.Multimeter.Measure().Result;
                            Prov2_Model.InitialData[i].Udy1Value = Converter.ConvertVoltToMilliVolt(result.Result);

                            if (prov2_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov2_WindowService.Token);
                    if (prov2_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov2_Model.CalculateData();

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол -5´ в сторону, противоположную выходной колодке изделия относительно положения 0";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = prov2_WindowService.Multimeter.Measure().Result.Result;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov2_WindowService.Multimeter.Measure(true).Result;
                            Prov2_Model.InitialData[i].Udy2Value = Converter.ConvertVoltToMilliVolt(result.Result);

                            if (prov2_WindowService.Token.IsCancellationRequested) return;
                        }                        
                    }, prov2_WindowService.Token);
                    if (prov2_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov2_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov2_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov2_WindowService.StopMeasure();
            }
        }
    }
}
