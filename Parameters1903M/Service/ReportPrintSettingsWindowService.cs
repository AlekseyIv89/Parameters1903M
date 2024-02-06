using Parameters1903M.Model;
using Parameters1903M.View;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class ReportPrintSettingsWindowService
    {
        public void OpenDialog(List<TableDeviceData> leftTableDeviceData, List<Parameter> allParameters)
        {
            ReportPrintSettingsWindow window = Application.Current.Windows.OfType<ReportPrintSettingsWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ReportPrintSettingsWindow(leftTableDeviceData, allParameters) { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }
    }
}
