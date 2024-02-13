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

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov1_ViewModel : BaseViewModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov1_WindowService prov1_WindowService;
        public ICommand Prov1_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov1_Model Prov1_Model { get; private set; }
        private Prov1_Window ProvWindow { get => prov1_WindowService.GetProvWindow(); }

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
                    message = "Установить призму с изделием на выставленную в горизонт поверочную плиту в исходное положение." + Environment.NewLine;
                    message += "Замкнуть ОС.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    message = "Повернуть призму с изделием на угол 90° в сторону маятника вокруг оси чувствительности.";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол 4° с погрешностью 10″ в сторону выходной колодки.";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    prov1_WindowService.Multimeter.SetAverageTimeMillis(4_000);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = prov1_WindowService.Multimeter.Measure().Result.Result;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov1_WindowService.Multimeter.Measure().Result;
                            Prov1_Model.InitialData[i].ScaleFactorValue1 = Converter.ConvertVoltToMilliAmpere(result.Result);

                            log.Debug($"U1({i + 1}) [В] = {result.Result:F7}, Rос, [Ом] = {GlobalVars.Rizm:F5}");
                            log.Debug($"I1({i + 1}) [мА] = {Prov1_Model.InitialData[i].ScaleFactorValue1:F7}");

                            if (prov1_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov1_WindowService.Token);
                    if (prov1_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол 4° с погрешностью 10″ в сторону противоположную выходной колодки";
                    mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        // Первую точку пропускаем
                        double missingValue = prov1_WindowService.Multimeter.Measure().Result.Result;

                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov1_WindowService.Multimeter.Measure(true).Result;
                            Prov1_Model.InitialData[i].ScaleFactorValue2 = Converter.ConvertVoltToMilliAmpere(result.Result);

                            log.Debug($"U2({i + 1}) [В] = {result.Result:F7}, Rос, [Ом] = {GlobalVars.Rizm:F5}");
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
