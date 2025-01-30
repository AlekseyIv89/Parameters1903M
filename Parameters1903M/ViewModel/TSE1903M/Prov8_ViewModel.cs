using OxyPlot.Series;
using OxyPlot;
using Parameters1903M.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.View.TSE1903M;
using System.Windows.Input;
using Parameters1903M.Service.TSE1903M;
using OxyPlot.Axes;
using Parameters1903M.Service.Command;
using System.Windows;
using Parameters1903M.View;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.Util;
using System.Linq;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov8_ViewModel : BaseViewModel
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

        private readonly Prov8_WindowService prov8_WindowService;
        public ICommand Prov8_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov8_Model Prov8_Model { get; private set; }
        private Prov8_Window ProvWindow { get => prov8_WindowService.GetProvWindow(); }

        private IMeasure Multimeter { get => prov8_WindowService.Multimeter; }

        public Prov8_ViewModel(Parameter parameter)
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
                Title = "I, мкА",
                MajorGridlineStyle = LineStyle.Solid
            });
            ChartModel.Series.Add(LineSeries);
            LineSeries.ItemsSource = Points;

            Parameter = parameter;
            ButtonContent = BUTTON_START;

            Prov8_Model = new Prov8_Model(Parameter);

            prov8_WindowService = new Prov8_WindowService();

            Prov8_WindowCloseCommand = new RelayCommand(param => prov8_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                string message;
                string label = Parameter.Name.Split(',').First();

                if (!string.IsNullOrWhiteSpace(Parameter.StrValue))
                {
                    message = "Измерения уже проводились. Вы желаете стереть все данные по текущей проверке?";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, label, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Prov8_Model.ClearAllData();

                        Points.Clear();
                        ChartModel.InvalidatePlot(true);
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov8_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, замкните  ОС и накройте призму с изделием кожухом." + Environment.NewLine;
                    message += "Нажать \"ОК\" для начала отсчета 60-минутной временной задержки.";
                    MessageBoxResult mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    TimeSpan timeBetweenMeasurements = TimeSpan.FromMinutes(60);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = TimeSpan.FromSeconds(2);
                    }
                    TimerWindow timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    message = "Нажмите \"ОК\" для начала приема информации.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    DateTime dateTimeStart = DateTime.Now;
                    int secondsMeasureContinuing = 7200;
                    if (GlobalVars.IsDebugEnabled)
                    {
                        secondsMeasureContinuing = 5;
                    }

                    await Task.Run(() =>
                    {
                        int averageTimeInMillis = 20_000;
                        if (GlobalVars.IsDebugEnabled)
                        {
                            averageTimeInMillis = 1_000;
                        }

                        Multimeter.ResetAverageTime();
                        Multimeter.SetAverageTimeMillis(averageTimeInMillis);

                        double missingValue = Multimeter.Measure().Result.Value;

                        while (!prov8_WindowService.Token.IsCancellationRequested && DateTime.Now.Subtract(dateTimeStart).TotalSeconds < secondsMeasureContinuing)
                        {
                            MeasureResult result = Multimeter.Measure().Result;
                            double resultValueToMicroAmpere = Converter.ConvertVoltToMicroAmpere(result.Value);
                            Prov8_Model.InitialData.IValue = resultValueToMicroAmpere;
                            Prov8_Model.CalculateDataWhileMeasureRunning(resultValueToMicroAmpere);

                            Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), resultValueToMicroAmpere));
                            ChartModel.InvalidatePlot(true);
                        }
                    }, prov8_WindowService.Token);
                    if (prov8_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov8_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov8_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov8_WindowService.StopMeasure();
            }
        }
    }
}
