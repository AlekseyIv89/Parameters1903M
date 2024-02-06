using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov3_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public PlotModel ChartModel { get; } = new PlotModel
        {
            PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1)
        };
        private LineSeries LineSeries { get; } = new LineSeries
        {
            Color = OxyColors.Red
        };
        private IList<DataPoint> Points { get; } = new List<DataPoint>();

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private readonly Prov3_WindowService prov3_WindowService;
        public ICommand Prov3_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov3_Model Prov3_Model { get; private set; }
        private Prov3_Window ProvWindow { get => prov3_WindowService.GetProvWindow(); }

        public Prov3_ViewModel(Parameter parameter)
        {
            ChartModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Время, чч:мм:сс",
                StringFormat = "HH:mm:ss",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = true
            });
            ChartModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Uду, мВ",
                MajorGridlineStyle = LineStyle.Solid
            });
            ChartModel.Series.Add(LineSeries);
            LineSeries.ItemsSource = Points;

            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov3_Model = new Prov3_Model(Parameter);

            prov3_WindowService = new Prov3_WindowService();

            Prov3_WindowCloseCommand = new RelayCommand(param => prov3_WindowService.Close(param), x => true);
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
                        Prov3_Model.ClearAllData();

                        Points.Clear();
                        ChartModel.InvalidatePlot(true);
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov3_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    string message = "Соединить перемычками вход вольтметра к клеммам ДУ и ЭКРАН, предварительно отключив его от клемм ±U";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Выставить плиту в горизонт." + Environment.NewLine;
                    message += "Установить прибор в положение маятником вниз." + Environment.NewLine;
                    message += "С помощью переключателя ВХОД-ЭКВ и тумблера IОС-РАЗР.ОС разорвать ОС." + Environment.NewLine;
                    message += "Отрегулировать напряжение на выходе ДУ";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    message = "Нажмите \"Ок\" для начала приема информации";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Cancel) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeBetweenMeasurements = new TimeSpan(1, 0, 0);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = new TimeSpan(0, 0, 2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow }; //Таймер на 20 секунд
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter); //Выход из проверки, если закрыто окно с таймером

                    DateTime dateTimeStart = DateTime.Now;
                    int secondsMeasureContinuing = 1800;
                    if (GlobalVars.IsDebugEnabled)
                    {
                        secondsMeasureContinuing = 5;
                    }

                    await Task.Run(() =>
                    {
                        while (!prov3_WindowService.Token.IsCancellationRequested && DateTime.Now.Subtract(dateTimeStart).TotalSeconds < secondsMeasureContinuing)
                        {
                            MeasureResult result = prov3_WindowService.Multimeter.Measure().Result;
                            double resultValueToMilliV = Converter.ConvertVoltToMilliVolt(result.Result);
                            Prov3_Model.InitialData.UdyValue = resultValueToMilliV;
                            Prov3_Model.CalculateDataWhileMeasureRunning(resultValueToMilliV);

                            Points.Add(new DataPoint(DateTimeAxis.ToDouble(resultValueToMilliV), result.Result));
                            ChartModel.InvalidatePlot(true);
                        }
                    }, prov3_WindowService.Token);
                    if (prov3_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov3_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    string message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov3_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov3_WindowService.StopMeasure();
            }
        }
    }
}
