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
    internal class Prov14_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov14_WindowService prov14_WindowService;
        public ICommand Prov14_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov14_Model Prov14_Model { get; private set; }
        private Prov14_Window ProvWindow { get => prov14_WindowService.GetProvWindow(); }

        public Prov14_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov14_Model = new Prov14_Model(Parameter);

            prov14_WindowService = new Prov14_WindowService();

            Prov14_WindowCloseCommand = new RelayCommand(param => prov14_WindowService.Close(param), x => true);
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
                        Prov14_Model.ClearAllData();
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov14_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    string message = "Установить призму с изделием на выставленную в горизонт поверочную плиту в исходное положение." + Environment.NewLine;
                    message += "Замкнуть ОС.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    message = "Повернуть призму с изделием на угол 90° в сторону маятника вокруг оси чувствительности.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол 4° с погрешностью 10″ в сторону выходной колодки.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov14_WindowService.Multimeter.Measure().Result;
                            Prov14_Model.InitialData[i].ScaleFactorPendulumDownValue1 = Converter.ConvertVoltToMilliAmpere(result.Result);

                            if (prov14_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov14_WindowService.Token);
                    if (prov14_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    message = "С помощью оптического квадранта наклонить плоскость поверочной плиты на угол 4° с погрешностью 10″ в сторону противоположную выходной колодки";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel
                        , MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MeasureResult result = prov14_WindowService.Multimeter.Measure(true).Result;
                            Prov14_Model.InitialData[i].ScaleFactorPendulumDownValue2 = Converter.ConvertVoltToMilliAmpere(result.Result);

                            if (prov14_WindowService.Token.IsCancellationRequested) return;
                        }
                    }, prov14_WindowService.Token);
                    if (prov14_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov14_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov14_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov14_WindowService.StopMeasure();
            }
        }
    }
}
