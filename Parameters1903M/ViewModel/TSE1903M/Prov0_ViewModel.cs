using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Parameters1903M.Model;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov0_ViewModel : BaseViewModel
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

        private readonly Prov0_WindowService prov0_WindowService;
        public ICommand Prov0_WindowCloseCommand { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov0_ViewModel(Parameter parameter)
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
                Title = "Ток, мА",
                MajorGridlineStyle = LineStyle.Solid
            });
            ChartModel.Series.Add(LineSeries);
            LineSeries.ItemsSource = Points;

            Parameter = parameter;
            ButtonContent = BUTTON_START;

            prov0_WindowService = new Prov0_WindowService();

            Prov0_WindowCloseCommand = new RelayCommand(param => prov0_WindowService.Close(param), x => true);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                ButtonContent = BUTTON_STOP;
                prov0_WindowService.StartMeasure();

                prov0_WindowService.Multimeter.SetAverageTimeMillis(2_500);

                await Task.Run(() =>
                {
                    while (!prov0_WindowService.Token.IsCancellationRequested)
                    {
                        MeasureResult result = prov0_WindowService.Multimeter.Measure().Result;
                        Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), Converter.ConvertVoltToMilliAmpere(result.Result)));
                        ChartModel.InvalidatePlot(true);
                    }
                }, prov0_WindowService.Token);
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov0_WindowService.StopMeasure();
            }
        }
    }
}
