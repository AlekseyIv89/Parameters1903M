using Parameters1903M.Model;
using Parameters1903M.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Parameters1903M.View
{
    public partial class ReportWindow : Window
    {
        public ReportWindow(List<TableDeviceData> leftTableDeviceData, List<TableDeviceData> rightTableDeviceData,
            List<Parameter> repParameters, string[] repConclusion, string repAssignment, string repSecondName,
            bool isVisibleOtk, bool isVisiblePz)
        {
            ReportViewModel viewModel = new ReportViewModel(leftTableDeviceData, rightTableDeviceData,
                repParameters, repConclusion, repAssignment, repSecondName,
                isVisibleOtk, isVisiblePz);
            InitializeComponent();

            TbProgramVersion.Text = viewModel.MyReportData.RepProgramVersion;

            DgLeftTableDeviceData.ItemsSource = viewModel.MyReportData.LeftTableDeviceData;
            DgRightTableDeviceData.ItemsSource = viewModel.MyReportData.RightTableDeviceData;
            DgParameters.ItemsSource = viewModel.MyReportData.RepParameters;

            TbRepConclusion.Text = viewModel.MyReportData.RepConclusion;
            TbRepConclusion2.Text = viewModel.MyReportData.RepConclusion2;
            tbRepAssignment.Text = viewModel.MyReportData.RepAssignment;
            TbRepSecondName.Text = viewModel.MyReportData.RepSecondName;
            TbRepDate.Text = viewModel.MyReportData.RepDate;

            TbOTK.Visibility = SepOtk.Visibility = viewModel.OtkVisibility;
            TbPZ.Visibility = SepPZ.Visibility = viewModel.PzVisibility;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid)sender).UnselectAllCells();
        }
    }
}
