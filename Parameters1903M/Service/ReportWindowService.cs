using Parameters1903M.Model;
using Parameters1903M.View;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Parameters1903M.Service
{
    internal class ReportWindowService
    {
        public void OpenDialog(List<TableDeviceData> leftTableDeviceData, List<TableDeviceData> rightTableDeviceData,
            List<Parameter> repParameters, string[] repConclusion, string repAssignment, string repSecondName,
            bool isVisibleOtk, bool isVisiblePz)
        {
            ReportWindow window = Application.Current.Windows.OfType<ReportWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ReportWindow(leftTableDeviceData, rightTableDeviceData, repParameters,
                    repConclusion, repAssignment, repSecondName, isVisibleOtk, isVisiblePz)
                {
                    Owner = Application.Current.MainWindow
                };
            }
            window.ShowDialog();
        }

        public void PrintReport(List<TableDeviceData> leftTableDeviceData, List<TableDeviceData> rightTableDeviceData,
            List<Parameter> repParameters, string[] repConclusion, string repAssignment, string repSecondName,
            bool isVisibleOtk, bool isVisiblePz)
        {
            ReportWindow window = Application.Current.Windows.OfType<ReportWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ReportWindow(leftTableDeviceData, rightTableDeviceData, repParameters,
                    repConclusion, repAssignment, repSecondName, isVisibleOtk, isVisiblePz)
                {
                    Owner = Application.Current.MainWindow
                };
            }
            window.docViewer.Print();
        }

        public void SaveToPDFReport(List<TableDeviceData> leftTableDeviceData, List<TableDeviceData> rightTableDeviceData,
            List<Parameter> repParameters, string[] repConclusion, string repAssignment, string repSecondName,
            bool isVisibleOtk, bool isVisiblePz, string path)
        {
            ReportWindow window = Application.Current.Windows.OfType<ReportWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ReportWindow(leftTableDeviceData, rightTableDeviceData, repParameters,
                    repConclusion, repAssignment, repSecondName, isVisibleOtk, isVisiblePz)
                {
                    Owner = Application.Current.MainWindow
                };
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Package package = Package.Open(memoryStream, FileMode.Create))
                {
                    FixedDocument fixedDocument = (FixedDocument)window.docViewer.Document;
                    using (XpsDocument doc = new XpsDocument(package))
                    {
                        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                        writer.Write(fixedDocument);
                    }
                }

                PdfSharp.Xps.XpsModel.XpsDocument pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(memoryStream);
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, path, 0);
            }
        }
    }
}
