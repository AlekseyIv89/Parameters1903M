using Parameters1903M.Model;
using Parameters1903M.Service.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class ReportPrintSettingsViewModel : BaseViewModel
    {
        public string Title => "Настройки формы печати протокола";

        public string Document { get; set; }

        public string GrafaTable { get; set; }

        public bool IsSignAdjustor { get; set; } = true;

        public bool IsSignOTK { get; set; }

        public bool IsSignVPMO { get; set; }

        public List<TableDeviceData> LeftTableDeviceData { get; }
        public List<Parameter> AllParameters { get; }
        public ObservableCollection<Parameter> PrintParameters { get; private set; }

        public ICommand AllParametersShowCommand { get; }
        public ICommand MeasuredParametersShowCommand { get; }

        public ICommand BtnPrintReportCommand { get; }
        public ICommand BtnPreviewReportCommand { get; }

        public ReportPrintSettingsViewModel(List<TableDeviceData> leftTableDeviceData, List<Parameter> allParameters)
        {
            LeftTableDeviceData = new List<TableDeviceData>(leftTableDeviceData);
            AllParameters = new List<Parameter>();
            allParameters.ForEach(param =>
            {
                if (!param.Name.Equals("Выход на режим"))
                {
                    AllParameters.Add(param);
                }
            });
            PrintParameters = new ObservableCollection<Parameter>();
            AllOrMeasuredParametersShow(true);

            Document = LeftTableDeviceData[3].DeviceData;
            GrafaTable = LeftTableDeviceData[4].DeviceData;

            AllParametersShowCommand = new RelayCommand(param => AllOrMeasuredParametersShow(true), x => true);
            MeasuredParametersShowCommand = new RelayCommand(param => AllOrMeasuredParametersShow(false), x => true);
        }

        private void AllOrMeasuredParametersShow(bool isAllParametersShow)
        {
            if (isAllParametersShow)
            {
                PrintParameters.Clear();
                AllParameters.ForEach(param => PrintParameters.Add(param));
            }
            else
            {
                List<Parameter> measuredParameters = AllParameters.FindAll(param => !string.IsNullOrEmpty(param.StrValue));

                PrintParameters.Clear();
                AllParameters.ForEach(param => {
                    if (!string.IsNullOrEmpty(param.StrValue))
                    {
                        PrintParameters.Add(param);
                    }
                });
            }
        }
    }
}
