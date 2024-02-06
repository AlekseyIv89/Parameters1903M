using Parameters1903M.Model;
using Parameters1903M.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace Parameters1903M.View
{
    public partial class ReportPrintSettingsWindow : Window
    {
        public ReportPrintSettingsWindow(List<TableDeviceData> leftTableDeviceData, List<Parameter> allParameters)
        {
            DataContext = new ReportPrintSettingsViewModel(leftTableDeviceData, allParameters);
            InitializeComponent();
        }
    }
}
