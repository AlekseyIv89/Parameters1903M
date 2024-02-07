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

        private string gridviewcolumnHeader;
        public string GridviewcolumnHeader 
        { 
            get => gridviewcolumnHeader; 
            private set
            {
                gridviewcolumnHeader = value;
                OnPropertyChanged();
            } 
        }

        private bool groupBoxIsEnabled = true;
        public bool GroupBoxIsEnabled 
        { 
            get => groupBoxIsEnabled;
            private set
            {
                groupBoxIsEnabled = value;
                OnPropertyChanged();
            }
        }

        public List<MeasureResult> ListviewDataPoints { get; }

        public PlotModel ChartModel { get; } = new PlotModel
        {
            PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1)
        };
        private LineSeries LineSeries { get; } = new LineSeries
        {
            Color = OxyColors.Red
        };
        private IList<DataPoint> Points { get; }

        public Parameter Parameter { get; }

        private const string BUTTON_START = "Пуск";
        private const string BUTTON_STOP = "Стоп";

        private AxesY axesY;

        private readonly Prov0_WindowService prov0_WindowService;
        public ICommand Prov0_WindowCloseCommand { get; }
        public ICommand AxesYRadiobuttonSelected { get; }
        public ICommand ButtonStartOrStopCommand { get; }

        public Prov0_ViewModel(Parameter parameter)
        {
            ListviewDataPoints = new List<MeasureResult>();

            ChartModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Время, чч:мм:сс",
                StringFormat = "HH:mm:ss",
                MajorGridlineStyle = LineStyle.Solid,
                IsZoomEnabled = true
            });
            
            ChartModel.Series.Add(LineSeries);

            Points = new List<DataPoint>();
            LineSeries.ItemsSource = Points;

            Parameter = parameter;
            ButtonContent = BUTTON_START;

            prov0_WindowService = new Prov0_WindowService();

            Prov0_WindowCloseCommand = new RelayCommand(param => prov0_WindowService.Close(param), x => true);
            AxesYRadiobuttonSelected = new RelayCommand(param => AxesYSelected(param), x => GroupBoxIsEnabled);
            ButtonStartOrStopCommand = new RelayCommand(param => ParameterMeasure(), x => true);

            AxesYRadiobuttonSelected.Execute("Ima");
        }

        private void AxesYSelected(object param)
        {
            if (ChartModel.Axes.Count > 1)
                ChartModel.Axes.RemoveAt(ChartModel.Axes.Count - 1);            

            switch ((string)param)
            {
                case "Ima":
                    axesY = AxesY.Ima;
                    ChartModel.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "I, мА",
                        MajorGridlineStyle = LineStyle.Solid
                    });
                    GridviewcolumnHeader = "I, мА";
                    break;
                case "Imka":
                    axesY = AxesY.Imka;
                    ChartModel.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "I, мкА",
                        MajorGridlineStyle = LineStyle.Solid
                    });
                    GridviewcolumnHeader = "I, мкА";
                    break;
                case "Uv":
                    axesY = AxesY.Uv;
                    ChartModel.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "U, В",
                        MajorGridlineStyle = LineStyle.Solid
                    });
                    GridviewcolumnHeader = "U, В";
                    break;
                case "Umv":
                    axesY = AxesY.Umv;
                    ChartModel.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "U, мВ",
                        MajorGridlineStyle = LineStyle.Solid
                    });
                    GridviewcolumnHeader = "U, мВ";
                    break;
                default:
                    break;
            }

            ListviewDataPoints.Clear();
            Points.Clear();
            ChartModel.InvalidatePlot(true);
        }

        private async void ParameterMeasure()
        {
            if (ButtonContent.Equals(BUTTON_START))
            {
                ButtonContent = BUTTON_STOP;
                prov0_WindowService.StartMeasure();
                GroupBoxIsEnabled = false;

                ListviewDataPoints.Clear();
                Points.Clear();
                ChartModel.InvalidatePlot(true);

                prov0_WindowService.Multimeter.SetAverageTimeMillis(2_500);

                await Task.Run(() =>
                {
                    while (!prov0_WindowService.Token.IsCancellationRequested)
                    {
                        MeasureResult result = prov0_WindowService.Multimeter.Measure().Result;
                        switch (axesY)
                        {
                            case AxesY.Ima:
                                ListviewDataPoints.Add(result);
                                Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), Converter.ConvertVoltToMilliAmpere(result.Result)));
                                break;
                            case AxesY.Imka:
                                ListviewDataPoints.Add(result);
                                Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), Converter.ConvertVoltToMicroAmpere(result.Result)));
                                break;
                            case AxesY.Uv:
                                ListviewDataPoints.Add(result);
                                Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), result.Result));
                                break;
                            case AxesY.Umv:
                                ListviewDataPoints.Add(result);
                                Points.Add(new DataPoint(DateTimeAxis.ToDouble(result.DateTime), Converter.ConvertVoltToMilliVolt(result.Result)));
                                break;
                            default:
                                break;
                        }
                        
                        ChartModel.InvalidatePlot(true);
                    }
                }, prov0_WindowService.Token);
            }
            else if (ButtonContent.Equals(BUTTON_STOP))
            {
                ButtonContent = BUTTON_START;
                prov0_WindowService.StopMeasure();
                GroupBoxIsEnabled = true;
            }
        }
    }

    enum AxesY
    {
        Ima,
        Imka,
        Uv,
        Umv
    }
}
