using OxyPlot.Axes;
using OxyPlot;
using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View;
using Parameters1903M.View.TSE1903M;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OxyPlot.Series;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov6_ViewModel : BaseViewModel
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

        private readonly Prov6_WindowService prov6_WindowService;
        private Prov6_Window ProvWindow { get => prov6_WindowService.GetProvWindow(); }

        public Prov6_Model Prov6_Model { get; private set; }

        public ICommand Prov6_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        private IMeasure Multimeter { get => prov6_WindowService.Multimeter; }

        public Prov6_ViewModel(Parameter parameter)
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

            Prov6_Model = new Prov6_Model(Parameter);

            prov6_WindowService = new Prov6_WindowService();

            Prov6_WindowCloseCommand = new RelayCommand(param => prov6_WindowService.Close(param), x => true);
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
                        Prov6_Model.ClearAllData();

                        Points.Clear();
                        ChartModel.InvalidatePlot(true);
                    }
                    else return;
                }

                ButtonContent = BUTTON_STOP;
                prov6_WindowService.StartMeasure();

                #region Начало измерения
                //---------------------------- Начало измерения ----------------------------
                try
                {
                    message = "Установите призму с изделием на выставленную в горизонт поверочную плиту в исходное положение, " +
                        "подключите изделие к стойке в режиме измерения ТОС, замкните ОС и накройте призму с изделием кожухом.";
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
                    int secondsMeasureContinuing = 900;
                    if (GlobalVars.IsDebugEnabled)
                    {
                        secondsMeasureContinuing = 5;
                    }

                    // TODO
                    await Task.Run(() =>
                    {
                        int averageTimeInMillis = 1_000;
                        if (GlobalVars.IsDebugEnabled)
                        {
                            averageTimeInMillis = 1_000;
                        }

                        Multimeter.ResetAverageTime();
                        Multimeter.SetAverageTimeMillis(averageTimeInMillis);

                        double missingValue = Multimeter.Measure().Result.Value;

                        while (!prov6_WindowService.Token.IsCancellationRequested && DateTime.Now.Subtract(dateTimeStart).TotalSeconds < secondsMeasureContinuing)
                        {
                            MeasureResult result = Multimeter.Measure().Result;
                            double resultValueToMicroAmpere = Converter.ConvertVoltToMicroAmpere(result.Value);
                            Prov6_Model.InitialData.IValue = resultValueToMicroAmpere;
                            //Prov6_Model.CalculateDataWhileMeasureRunning(resultValueToMicroAmpere);

                            // TODO

                            Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), resultValueToMicroAmpere));
                            ChartModel.InvalidatePlot(true);
                        }
                    }, prov6_WindowService.Token);
                    if (prov6_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    message = "Нажать кнопку \"БАСН\" на блоке БОС и подтвердить выполнение.";
                    mbr = MessageBox.Show(ProvWindow, message, Parameter.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (mbr != MessageBoxResult.OK) throw new ProvCancelledByUserException(Parameter);

                    timeBetweenMeasurements = TimeSpan.FromSeconds(100);
                    if (GlobalVars.IsDebugEnabled)
                    {
                        timeBetweenMeasurements = TimeSpan.FromSeconds(2);
                    }
                    timerWindow = new TimerWindow(timeBetweenMeasurements) { Owner = ProvWindow };
                    if (timerWindow.ShowDialog() != true) throw new ProvCancelledByUserException(Parameter);

                    dateTimeStart = DateTime.Now;

                    await Task.Run(() =>
                    {
                        int averageTimeInMillis = 1_000;
                        if (GlobalVars.IsDebugEnabled)
                        {
                            averageTimeInMillis = 1_000;
                        }

                        Multimeter.ResetAverageTime();
                        Multimeter.SetAverageTimeMillis(averageTimeInMillis);

                        double missingValue = Multimeter.Measure().Result.Value;

                        while (!prov6_WindowService.Token.IsCancellationRequested && DateTime.Now.Subtract(dateTimeStart).TotalSeconds < secondsMeasureContinuing)
                        {
                            MeasureResult result = Multimeter.Measure().Result;
                            double resultValueToMicroAmpere = Converter.ConvertVoltToMicroAmpere(result.Value);
                            Prov6_Model.InitialData.IValue = resultValueToMicroAmpere;
                            //Prov6_Model.CalculateDataWhileMeasureRunning(resultValueToMicroAmpere);

                            // TODO

                            Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), resultValueToMicroAmpere));
                            ChartModel.InvalidatePlot(true);
                        }
                    }, prov6_WindowService.Token);
                    if (prov6_WindowService.Token.IsCancellationRequested) throw new ProvCancelledByUserException(Parameter);

                    Prov6_Model.CalculateData();
                }
                catch (ProvCancelledByUserException e)
                {
                    message = $"Проверка параметра \"{e.Parameter.Name}\" прервана пользователем";
                    MessageBox.Show(ProvWindow, message, e.Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //---------------------------- Конец измерения -----------------------------
                #endregion

                ButtonContent = BUTTON_START;
                prov6_WindowService.StopMeasure();
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov6_WindowService.StopMeasure();
            }
        }
    }
}
