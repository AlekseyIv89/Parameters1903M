using Parameters1903M.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Parameters1903M.ViewModel
{
    internal class ReportViewModel : BaseViewModel
    {
        public ReportData MyReportData { get; }

        public Visibility OtkVisibility { get; }

        public Visibility PzVisibility { get; }

        public ReportViewModel(List<TableDeviceData> leftTableDeviceData, List<TableDeviceData> rightTableDeviceData,
            List<Parameter> repParameters, string[] repConclusion, string repAssignment, string repSecondName,
            bool isVisibleOtk, bool isVisiblePz)
        {
            MyReportData = new ReportData
            {
                LeftTableDeviceData = leftTableDeviceData,
                RightTableDeviceData = rightTableDeviceData,
                RepParameters = repParameters,
                RepConclusion = repConclusion[0],
                RepConclusion2 = "         " + repConclusion[1],
                RepAssignment = repAssignment,
                RepSecondName = repSecondName
            };

            if (!isVisibleOtk)
            {
                //Скрываем разделитель и строку для ОТК
                OtkVisibility = Visibility.Collapsed;
            }

            if (!isVisiblePz)
            {
                //Скрываем разделитель и строку для ПЗ
                PzVisibility = Visibility.Collapsed;
            }
        }
    }
}
