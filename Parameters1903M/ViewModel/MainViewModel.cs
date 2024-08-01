using Microsoft.Win32;
using Parameters1903M.Model;
using Parameters1903M.Service;
using Parameters1903M.Service.Command;
using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public DeviceType deviceType;
        public DeviceType DeviceType
        {
            get => deviceType;
            set
            {
                deviceType = value;
                DeviceData.DeviceType = EnumInfo.GetDescription(deviceType);
            }
        }

        public DeviceData DeviceData { get; set; }

        public string Title { get; private set; } = ProgramInfo.SoftwareNameWithVersion;

        private readonly MainWindowService mainWindowService;
        private readonly StartWindowService startWindowService;
        public ICommand StartWindowShowDialogCommand { get; }
        public ICommand MainWindowClosingCommand { get; }
        public ICommand MainWindowCloseCommand { get; }
        public ICommand ListViewDoubleClickCommand { get; }

        //----------------- Меню "Протокол"
        private readonly ReportWindowService reportWindowService;
        public ICommand ReportPreviewShowCommand { get; }
        public ICommand ReportPrintCommand { get; }
        public ICommand ReportSaveToPDFCommand { get; }

        private readonly ReportPrintSettingsWindowService reportPrintSettingsWindowService;
        public ICommand ReportPrintSettingsShowCommand { get; }

        //----------------- Меню "Изменить"
        public ICommand ChangeAdjusterSecondNameCommand { get; set; }
        public ICommand ChangeConclusionCommand { get; set; }
        public ICommand ChangeRizmCommand { get; set; }
        private readonly ProgramSettingsWindowService programSettingsWindowService;
        public ICommand ChangeSettingsCommand { get; set; }

        //----------------- Меню "Справка"
        private readonly WhatsNewWindowService whatsNewWindowService;
        public ICommand WhatsNewWindowShowCommand { get; }

        private readonly AboutWindowService aboutWindowService;
        public ICommand AboutWindowShowDialogCommand { get; }
        //---------------------------------

        public ObservableCollection<TableDeviceData> LeftTableDeviceData { get; }
        public ObservableCollection<TableDeviceData> RightTableDeviceData { get; }
        public List<Parameter> Parameters { get; }

        public IMeasure Multimeter { get; private set; }
        public CommunicationInterface CommunicationInterface { get; }

        public MainViewModel()
        {
            DeviceData = new DeviceData
            {
                Date = DateTime.Now.ToString("d"),
                Rizm = Properties.Settings.Default.Rizm
            };

            LeftTableDeviceData = new ObservableCollection<TableDeviceData>
            {
                new TableDeviceData() { Name = $"ЦЕ1903М зав. №" },
                new TableDeviceData() { Name = "Место" },
                new TableDeviceData() { Name = "Призма" },
                new TableDeviceData() { Name = "Документ" },
                new TableDeviceData() { Name = "Графа таблицы" },
            };

            RightTableDeviceData = new ObservableCollection<TableDeviceData>
            {
                new TableDeviceData() { Name = "Дата", DeviceData = DeviceData.Date },
                new TableDeviceData() { Name = "Начало исп." },
                new TableDeviceData() { Name = "Проверка после" },
                new TableDeviceData() { Name = "Rизм, Ом", DeviceData = DeviceData.RizmStr },
            };

            Parameters = new List<Parameter>
            {
                new Parameter() { Num = "", Name = "Выход на режим" },
                new Parameter() { Num = "1", Name = "Масштабный коэффициент, Ig", Unit = "мА/g", Digits = 1 },
                new Parameter() { Num = "2", Name = "Крутизна характеристики ДУ, Sду", Unit = "мВ/...ʹ", Digits = 0 },
                new Parameter() { Num = "3", Name = "Дрейф нуля ДУ", Unit = "...″", Digits = 0 },
                new Parameter() { Num = "4", Name = "Переменная составляющая вых. напряжения ДУ, U~", Unit = "мВ", Digits = 0 },
                new Parameter() { Num = "5", Name = "Угол крепления механических упоров", Unit = "...ʹ", Digits = 0 },
                new Parameter() { Num = "6", Name = "Флюктуация дрейфа ТОС", Unit = "...″", Digits = 3 },
                new Parameter() { Num = "11", Name = "Нестабильность дрейфа ТОС", Unit = "...″/сек.", Digits = 0 },
                new Parameter() { Num = "7", Name = "Сопротивление обмотки ОС, Rос", Unit = "Ом", Digits = 1 },
                new Parameter() { Num = "8", Name = "Стабильность положения ОЧ", Unit = "...″", Digits = 2 },
                new Parameter() { Num = "9", Name = "Функц. способность термопредохр.", Unit = "", Digits = 0 },
                new Parameter() { Num = "10", Name = "Непараллельность ОЧ базовой плоскости", Unit = "...″", Digits = 0 },
                new Parameter() { Num = "16", Name = "Неперпендикулярность ОЧ опорной плоскости", Unit = "...″", Digits = 0 },
                new Parameter() { Num = "15", Name = "Температурный коэффициент, Kt", Unit = "%/°С", Digits = 0 },
                new Parameter() { Num = "14", Name = "МК в положении маятником вниз", Unit = "мА/g", Digits = 0 },
                new Parameter() { Num = "13", Name = "Время установления выходной информации, ВУВИ", Unit = "сек.", Digits = 0 },
                new Parameter() { Num = "17", Name = "Ток отрыва ПС от упора", Unit = "мА", Digits = 0 },
                new Parameter() { Num = "18", Name = "Время схода ПС с упора", Unit = "сек.", Digits = 0 }
            };

            mainWindowService = new MainWindowService();
            startWindowService = new StartWindowService();
            StartWindowShowDialogCommand = new RelayCommand(param => startWindowService.OpenDialog(param), x => true);
            MainWindowClosingCommand = new RelayCommand(cancelEventArgs => mainWindowService.Close(cancelEventArgs), x => true);
            MainWindowCloseCommand = new RelayCommand(param => mainWindowService.Close(), x => true);
            ListViewDoubleClickCommand = new RelayCommand(param => mainWindowService.ListViewDoubleClick(param), x => true);

            reportWindowService = new ReportWindowService();
            ReportPreviewShowCommand = new RelayCommand(param => ReportWindowShowDialog(), x => true);
            ReportPrintCommand = new RelayCommand(param => ReportPrint(), x => true);
            ReportSaveToPDFCommand = new RelayCommand(param => ReportSaveToPDF(), x => true);

            ChangeAdjusterSecondNameCommand = new RelayCommand(param => ChangeAdjusterSecondName(), x => true);
            ChangeConclusionCommand = new RelayCommand(param => ChangeConclusion(), x => true);
            ChangeRizmCommand = new RelayCommand(param => ChangeRizm(), x => true);
            programSettingsWindowService = new ProgramSettingsWindowService();
            ChangeSettingsCommand = new RelayCommand(param => programSettingsWindowService.OpenDialog(param), x => true);

            reportPrintSettingsWindowService = new ReportPrintSettingsWindowService();
            ReportPrintSettingsShowCommand = new RelayCommand(param => reportPrintSettingsWindowService.OpenDialog(new List<TableDeviceData>(LeftTableDeviceData), Parameters), x => true);

            whatsNewWindowService = new WhatsNewWindowService();
            WhatsNewWindowShowCommand = new RelayCommand(param => whatsNewWindowService.Open(), x => true);

            aboutWindowService = new AboutWindowService();
            AboutWindowShowDialogCommand = new RelayCommand(param => aboutWindowService.OpenDialog(param), x => true);

            CommunicationInterface = (CommunicationInterface)Enum.Parse(typeof(CommunicationInterface), Properties.Settings.Default.VoltmeterType);
            switch (CommunicationInterface)
            {
                case CommunicationInterface.Emulator:
                    Multimeter = new Simulator();
                    break;
                case CommunicationInterface.V7_84:
                    Multimeter = new V7_84();
                    break;
                case CommunicationInterface.V2_43:
                    Multimeter = new V2_43();
                    break;
                default:
                    throw new Exception("Не существует такого типа вольтметра");
            }
            CommunicationInterface = Multimeter.Connect(Properties.Settings.Default.ComPort);

#if DEBUG
            GlobalVars.IsDebugEnabled = true;
            Title += " DEBUG MODE ENABLED";
#endif
        }

        public void UpdateDataGrid()
        {
            LeftTableDeviceData[0].Name = $"{DeviceData.DeviceType} зав. №";
            LeftTableDeviceData[0].DeviceData = DeviceData.DeviceNum;
            LeftTableDeviceData[1].DeviceData = DeviceData.WorkspaceNum;
            LeftTableDeviceData[2].DeviceData = DeviceData.PrismNum;
            LeftTableDeviceData[3].DeviceData = DeviceData.Document;
            LeftTableDeviceData[4].DeviceData = DeviceData.GrafaTable;

            RightTableDeviceData[2].DeviceData = DeviceData.ProvAfter;
            RightTableDeviceData[3].DeviceData = DeviceData.RizmStr;

            GlobalVars.DeviceNum = DeviceData.DeviceNum;
        }

        private string[] ComputeConclusionText(bool showMessageBox = true)
        {
            // TODO: Сделать вычисление фразы в зависимости от измеренных значений параметров
            // т.е. для каждой графы хранить допуска по каждому параметру
            string message = $"Значения параметров изделия {DeviceData.DeviceType} зав.№ {DeviceData.DeviceNum} соответствуют нормам графы {DeviceData.GrafaTable} " +
                $"{DeviceData.Document}?";
            string emptyOrNot = "";
            if (showMessageBox)
            {
                emptyOrNot = MessageBox.Show(mainWindowService.GetMainWindow(), message, "Заключение", MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes
                    ? ""
                    : "не ";
            }

            string firstHalfConclusionText = $"значения параметров изделия {DeviceData.DeviceType} зав.№ {DeviceData.DeviceNum} {emptyOrNot}соответствуют нормам ";
            string secondHalfConclusionText = $"графы {DeviceData.GrafaTable} {DeviceData.Document}";

            return new string[] { firstHalfConclusionText, secondHalfConclusionText };
        }

        private void ReportWindowShowDialog()
        {
            List<Parameter> parametersForReport = Parameters.FindAll(param => !string.IsNullOrEmpty(param.StrValue));

            reportWindowService.OpenDialog(
                new List<TableDeviceData>(LeftTableDeviceData),
                new List<TableDeviceData>(RightTableDeviceData),
                parametersForReport,
                ComputeConclusionText(),
                DeviceData.Conclusion,
                Properties.Settings.Default.AdjusterSecondName,
                GlobalVars.IsReportOtkVisible,
                GlobalVars.IsReportPzVisible);
        }

        private void ReportPrint()
        {
            List<Parameter> parametersForReport = Parameters.FindAll(param => !string.IsNullOrEmpty(param.StrValue));

            reportWindowService.PrintReport(
                new List<TableDeviceData>(LeftTableDeviceData),
                new List<TableDeviceData>(RightTableDeviceData),
                parametersForReport,
                ComputeConclusionText(),
                DeviceData.Conclusion,
                Properties.Settings.Default.AdjusterSecondName,
                GlobalVars.IsReportOtkVisible,
                GlobalVars.IsReportPzVisible);
        }

        private void ReportSaveToPDF()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                InitialDirectory = GlobalVars.SavePath,
                FileName = "Протокол",
                DefaultExt = ".pdf",
                Filter = "PDF (*.pdf)|*.pdf",
                Title = "Сохранить протокол измерения как"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                List<Parameter> parametersForReport = Parameters.FindAll(param => !string.IsNullOrEmpty(param.StrValue));

                reportWindowService.SaveToPDFReport(
                new List<TableDeviceData>(LeftTableDeviceData),
                new List<TableDeviceData>(RightTableDeviceData),
                parametersForReport,
                ComputeConclusionText(false),
                DeviceData.Conclusion,
                Properties.Settings.Default.AdjusterSecondName,
                GlobalVars.IsReportOtkVisible,
                GlobalVars.IsReportPzVisible,
                saveFileDialog.FileName);
            }
        }

        private void ChangeAdjusterSecondName()
        {

        }

        private void ChangeConclusion()
        {
            
        }

        private void ChangeRizm()
        {

        }
    }
}
