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
using log4net;
using Parameters1903M.View;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov1_ViewModel : BaseViewModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Title => Parameter.Name;

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

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov1_WindowService prov1_WindowService;
        public ICommand Prov1_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov1_Model Prov1_Model { get; private set; }
        private Prov1_Window ProvWindow { get => prov1_WindowService.GetProvWindow(); }

        private IMeasure Multimeter { get => prov1_WindowService.Multimeter; }

        public Prov1_ViewModel(Parameter parameter)
        {
            log4net.Config.XmlConfigurator.Configure();

            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov1_Model = new Prov1_Model(Parameter);

            prov1_WindowService = new Prov1_WindowService();

            Prov1_WindowCloseCommand = new RelayCommand(param => prov1_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',')[0];

                log.Debug($"   ========== Начало проверки '{label}' ==========   ");

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Prov1_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov1_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение " +
                        "и подключите изделие к стойке в режиме измерения ТОС, замкните ОС." + Environment.NewLine;
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    Multimeter.SetAverageTimeMillis(2_500);

                    bool vistavkaFlag = true;
                    do
                    {
                        //---------------------------- Начало "Вывод ТОС на экран" ----------------------------
                        bool flag = true;
                        CurrMeasureVisibility = Visibility.Visible;
                        Task task = Task.Run(() =>
                        {
                            while (flag)
                            {
                                CurrMeasureText = $"{Converter.ConvertVoltToMicroAmpere(Multimeter.Measure().Result.Value):F2} мкА";
                            }
                        });

                        message = "Установите изделие поворотом плиты в исходное положение, при котором ТОС находится в пределах ±10 мкА.";
                        mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                            , MessageBoxImage.Information);
                        if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                        flag = false;
                        task.Wait();
                        CurrMeasureVisibility = Visibility.Collapsed;
                        //---------------------------- Конец "Вывод ТОС на экран" ----------------------------

                        TimerWindow timerWindow1 = new TimerWindow(new TimeSpan(0, 0, 3)) { Owner = ProvWindow };
                        if (timerWindow1.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                        await Task.Run(() =>
                        {
                            double missingValue = Multimeter.Measure().Result.Value;
                            missingValue = Converter.ConvertVoltToMicroAmpere(Multimeter.Measure().Result.Value);

                            double checkCurrentMicroA = 10.0;
                            if (GlobalVars.IsDebugEnabled)
                            {
                                checkCurrentMicroA = double.MaxValue;
                            }

                            if (Math.Abs(missingValue) > checkCurrentMicroA)
                            {
                                message = "ТОС не соответствует допуску ±10 мкА. Повторите выставку изделия.";
                                mbr = MessageBox.Show(message, label, MessageBoxButton.OKCancel
                                    , MessageBoxImage.Warning);
                                if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);
                            }
                            else vistavkaFlag = false;
                        });
                    }
                    while (vistavkaFlag);                    

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол 4° с погрешностью ±10″ в сторону выходной колодки относительно исходного положения.";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeSpan = new TimeSpan(0, 0, 10);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeSpan = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    Multimeter.SetAverageTimeMillis(4_000);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = Multimeter.Measure().Result.Value;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = Multimeter.Measure().Result;
                            Prov1_Model.InitialData[i].ScaleFactorValue1 = Converter.ConvertVoltToMilliAmpere(result.Value);

                            log.Debug($"U1({i + 1}) [В] = {result.Value:F7}, Rос, [Ом] = {GlobalVars.Rizm:F5}");
                            log.Debug($"I1({i + 1}) [мА] = {Prov1_Model.InitialData[i].ScaleFactorValue1:F7}");

                            if (prov1_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov1_WindowService.Token);
                    if (prov1_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты относительно исходного положения на угол 4° с погрешностью ±10″ в сторону противоположную выходной колодки";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    timerWindow = new TimerWindow(timeSpan) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = Multimeter.Measure().Result.Value;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = Multimeter.Measure(true).Result;
                            Prov1_Model.InitialData[i].ScaleFactorValue2 = Converter.ConvertVoltToMilliAmpere(result.Value);

                            log.Debug($"U2({i + 1}) [В] = {result.Value:F7}, Rос, [Ом] = {GlobalVars.Rizm:F5}");
                            log.Debug($"I2({i + 1}) [мА] = {Prov1_Model.InitialData[i].ScaleFactorValue1:F7}");

                            if (prov1_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov1_WindowService.Token);
                    if (prov1_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov1_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    log.Debug($"   ========== Проверка '{label}' прервана пользователем ==========   ");
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov1_WindowService.StopMeasure();

                log.Debug($"   ========== Конец проверки '{label}' ==========   ");
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov1_WindowService.StopMeasure();
            }
        }
    }
}
