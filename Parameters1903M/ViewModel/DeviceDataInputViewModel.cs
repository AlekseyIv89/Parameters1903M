using Parameters1903M.Model;
using Parameters1903M.Service;
using Parameters1903M.Service.Command;
using Parameters1903M.Util;
using Parameters1903M.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class DeviceDataInputViewModel : BaseViewModel
    {
        public string Title { get; set; }
        public ProverkaType ProverkaType { get; }
        public DeviceType DeviceType { get => DeviceType.TSE1903M; }

        public string DeviceNum { get; set; }
        public string WorkspaceNum { get; set; }
        public string PrismNum { get; set; }

        public Visibility TextBoxDeviceNumVisibility { get; private set; } = Visibility.Hidden;
        public Visibility ComboBoxDeviceNumVisibility { get; private set; } = Visibility.Visible;

        public Visibility TextBlockWorkspaceNumVisibility { get; private set; } = Visibility.Visible;
        public Visibility TextBoxWorkspaceNumVisibility { get; private set; } = Visibility.Visible;
        public Visibility TextBlockPrismVisibility { get; private set; } = Visibility.Visible;
        public Visibility TextBoxPrismVisibility { get; private set; } = Visibility.Visible;

        public bool ComboBoxProvAfterIsEditable { get; private set; } = false;
        public bool TextBoxProvConclusionIsReadOnly { get; private set; } = true;

        public Visibility TextBlockGrafaTableVisibility { get; private set; } = Visibility.Collapsed;
        public Visibility ComboBoxGrafaTableVisibility { get; private set; } = Visibility.Collapsed;
        public Visibility TextBlockDocumentVisibility { get; private set; } = Visibility.Collapsed;
        public Visibility ComboBoxDocumentVisibility { get; private set; } = Visibility.Collapsed;

        public Visibility BtnPreviewVisibility { get; private set; } = Visibility.Collapsed;

        private readonly DeviceDataInputWindowService deviceDataInputWindowService;
        private readonly StartWindowService startWindowService;
        private readonly MainWindowService mainWindowService;

        private MainViewModel GetMainViewModel => mainWindowService.GetMainWindowViewModel();
        private DeviceDataInputWindow DeviceDataWindow { get => deviceDataInputWindowService.GetProvWindow(); }

        public ICommand DeviceDataInputWindowClosedCommand { get; }
        public ICommand ButtonPreviewCommand { get; }
        public ICommand ButtonOkCommand { get; }
        public ICommand ComboBoxClickCommand { get; }
        public ICommand ComboBoxDeviceNumSelectionChangedCommand { get; }
        public ICommand ComboBoxProvAfterSelectionChangedCommand { get; }

        public ObservableCollection<string> ComboBoxDevicesNums { get; set; }
        public ObservableCollection<string> ComboBoxProvAfterList { get; set; }
        public string ProvAfter { get; set; }
        private string conclusion;
        public string Conclusion
        {
            get => conclusion;
            set
            {
                conclusion = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> ComboBoxGrafaTableList { get; set; }
        public string GrafaTable { get; set; }
        public ObservableCollection<string> ComboBoxDocumentList { get; set; }
        public string Document { get; set; }

        public DeviceDataInputViewModel(ProverkaType proverkaType)
        {
            ProverkaType = proverkaType;
            ComboBoxDevicesNums = new ObservableCollection<string>();
            ComboBoxProvAfterList = new ObservableCollection<string>();
            ComboBoxGrafaTableList = new ObservableCollection<string>();
            ComboBoxDocumentList = new ObservableCollection<string>();

            ButtonContent = "Готово";
            SetWindowComponentsVisibility();

            deviceDataInputWindowService = new DeviceDataInputWindowService();
            startWindowService = new StartWindowService();
            mainWindowService = new MainWindowService();

            DeviceDataInputWindowClosedCommand = new RelayCommand(param => startWindowService.OpenDialog(param), x => true);
            ButtonOkCommand = new RelayCommand(param => DoWorkForSelectedProverkaType(param), x => true);
            //ButtonPreviewCommand = new RelayCommand(param => DoWorkForSelectedProverkaType(param), x => true);
            ComboBoxClickCommand = new RelayCommand(param => FindMeasuredDevicesNums(), x => true);
            ComboBoxDeviceNumSelectionChangedCommand = new RelayCommand(param => SelectMeasuredDevicesNum(), x => true);
            ComboBoxProvAfterSelectionChangedCommand = new RelayCommand(param => SelectProvAfter(), x => proverkaType == ProverkaType.Continue);
        }

        private void SetWindowComponentsVisibility()
        {
            switch (ProverkaType)
            {
                case ProverkaType.New:
                    Title = "Начало испытаний";

                    TextBoxDeviceNumVisibility = Visibility.Visible;
                    ComboBoxDeviceNumVisibility = Visibility.Collapsed;

                    ComboBoxProvAfterList.Add("");
                    ComboBoxProvAfterList.Add("сборка");
                    ComboBoxProvAfterList.Add("регулировка");
                    ComboBoxProvAfterList.Add("ПИ ОТК");
                    ComboBoxProvAfterList.Add("ПСИ ВП МО");
                    ComboBoxProvAfterList.Add("На упаковку");

                    ComboBoxGrafaTableList.Add("2");
                    ComboBoxGrafaTableList.Add("3");
                    ComboBoxGrafaTableList.Add("4");
                    ComboBoxGrafaTableList.Add("5");
                    ComboBoxGrafaTableList.Add("6");

                    ComboBoxDocumentList.Add("ТУ");
                    ComboBoxDocumentList.Add("И14");

                    ComboBoxProvAfterIsEditable = true;
                    TextBoxProvConclusionIsReadOnly = false;

                    TextBlockGrafaTableVisibility = Visibility.Visible;
                    ComboBoxGrafaTableVisibility = Visibility.Visible;
                    TextBlockDocumentVisibility = Visibility.Visible;
                    ComboBoxDocumentVisibility = Visibility.Visible;

                    break;
                case ProverkaType.Continue:
                    Title = "Продолжение испытаний";

                    break;
                case ProverkaType.Print:
                    Title = "Предварительный просмотр и печать протокола";

                    TextBlockWorkspaceNumVisibility = Visibility.Hidden;
                    TextBoxWorkspaceNumVisibility = Visibility.Hidden;
                    TextBlockPrismVisibility = Visibility.Hidden;
                    TextBoxPrismVisibility = Visibility.Hidden;

                    BtnPreviewVisibility = Visibility.Visible;
                    ButtonContent = "Печать";
                    break;
                case ProverkaType.Export:
                    Title = "Экспорт зашифрованной папки";

                    TextBlockWorkspaceNumVisibility = Visibility.Hidden;
                    TextBoxWorkspaceNumVisibility = Visibility.Hidden;
                    TextBlockPrismVisibility = Visibility.Hidden;
                    TextBoxPrismVisibility = Visibility.Hidden;

                    ButtonContent = "Экспорт";
                    break;
                default:
                    throw new Exception("Такой вариант проверки не предусмотрен");
            }
        }

        private async void DoWorkForSelectedProverkaType(object param)
        {
            DeviceData readDeviceData;
            List<Parameter> readParameters;

            switch (ProverkaType)
            {
                case ProverkaType.New:
                    if (string.IsNullOrEmpty(DeviceNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите зав. № прибора.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(WorkspaceNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите номер рабочего места.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(PrismNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите номер призмы.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(ProvAfter))
                    {
                        MessageBox.Show(DeviceDataWindow, "Выберите \"Этап - Проверка после\".", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(Conclusion))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите заключение.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    GetMainViewModel.DeviceType = DeviceType;
                    GetMainViewModel.DeviceData.DeviceNum = DeviceNum;
                    GetMainViewModel.DeviceData.WorkspaceNum = WorkspaceNum;
                    GetMainViewModel.DeviceData.PrismNum = PrismNum;
                    GetMainViewModel.DeviceData.Document = Document;
                    GetMainViewModel.DeviceData.GrafaTable = GrafaTable;
                    GetMainViewModel.DeviceData.ProvAfter = ProvAfter;
                    GetMainViewModel.DeviceData.Conclusion = Conclusion;
                    GetMainViewModel.UpdateDataGrid();

                    GlobalVars.SavePath = $"{Properties.Settings.Default.StartSavePath}" +
                        $"{EnumInfo.GetDescription(DeviceType)}\\{DeviceNum}\\{ProvAfter}_{Conclusion}";

                    if (Directory.Exists(GlobalVars.SavePath) && DirPathToCheckExists(GlobalVars.SavePath))
                    {
                        int i = 1;
                        string savePath = $"{Path.GetDirectoryName(GlobalVars.SavePath)}\\{ProvAfter} повт.{i}_{Conclusion}";
                        while (DirPathToCheckExists(savePath))
                        {
                            savePath = $"{Path.GetDirectoryName(GlobalVars.SavePath)}\\{ProvAfter} повт.{++i}_{Conclusion}";
                        }
                        GlobalVars.SavePath = savePath;
                    }

                    deviceDataInputWindowService.Close(param);
                    break;
                case ProverkaType.Continue:
                    if (string.IsNullOrEmpty(DeviceNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите зав. № прибора.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(WorkspaceNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите номер рабочего места.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(PrismNum))
                    {
                        MessageBox.Show(DeviceDataWindow, "Введите номер призмы.", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (string.IsNullOrEmpty(ProvAfter))
                    {
                        MessageBox.Show(DeviceDataWindow, "Выберите \"Этап - Проверка после\".", null, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    GetMainViewModel.DeviceType = DeviceType;
                    GetMainViewModel.DeviceData.DeviceNum = DeviceNum;
                    GetMainViewModel.DeviceData.ProvAfter = ProvAfter;
                    GetMainViewModel.DeviceData.Conclusion = Conclusion;
                    GetMainViewModel.UpdateDataGrid();

                    GlobalVars.SavePath = $"{Properties.Settings.Default.StartSavePath}" +
                        $"{EnumInfo.GetDescription(DeviceType)}\\{DeviceNum}\\{ProvAfter}_{Conclusion}";

                    readDeviceData = await new MainWindowService().ReportHeaderReadAsync(GlobalVars.SavePath + $@"\{DeviceNum}");
                    readParameters = await new MainWindowService().ReportParamsRead(GlobalVars.SavePath + $@"\{DeviceNum}");

                    GetMainViewModel.DeviceData.WorkspaceNum = readDeviceData.WorkspaceNum;
                    GetMainViewModel.DeviceData.PrismNum = readDeviceData.PrismNum;
                    GetMainViewModel.UpdateDataGrid();

                    readParameters.ForEach(parameter =>
                    {
                        if (!string.IsNullOrWhiteSpace(parameter.StrValue))
                        {
                            new MainWindowService().GetParameterByName(parameter.Name).StrValue = parameter.StrValue;
                        }
                    });

                    deviceDataInputWindowService.Close(param);
                    break;
                case ProverkaType.Print:
                    ComboBoxProvAfterSelectionChangedCommand.Execute(true); //TODO: разобраться почему не срабатывает автоматически при выборе этапа
                    GetMainViewModel.DeviceType = DeviceType;
                    GetMainViewModel.DeviceData.DeviceNum = DeviceNum;
                    GetMainViewModel.DeviceData.ProvAfter = ProvAfter;
                    GetMainViewModel.DeviceData.Conclusion = Conclusion;

                    GlobalVars.SavePath = $"{Properties.Settings.Default.StartSavePath}" +
                        $"{EnumInfo.GetDescription(DeviceType)}\\{DeviceNum}\\{ProvAfter}_{Conclusion}";

                    readDeviceData = await new MainWindowService().ReportHeaderReadAsync(GlobalVars.SavePath + $@"\{DeviceNum}");
                    readParameters = await new MainWindowService().ReportParamsRead(GlobalVars.SavePath + $@"\{DeviceNum}");

                    GetMainViewModel.DeviceData.WorkspaceNum = readDeviceData.WorkspaceNum;
                    GetMainViewModel.DeviceData.PrismNum = readDeviceData.PrismNum;
                    GetMainViewModel.UpdateDataGrid();

                    readParameters.ForEach(parameter =>
                    {
                        if (!string.IsNullOrWhiteSpace(parameter.StrValue))
                        {
                            new MainWindowService().GetParameterByName(parameter.Name).StrValue = parameter.StrValue;
                        }
                    });

                    GetMainViewModel.ReportPrintCommand.Execute(true);
                    break;
                case ProverkaType.Export:
                    MessageBox.Show("В разработке");
                    break;
                default:
                    throw new Exception("Такой вариант проверки не предусмотрен");
            }
        }

        private void FindMeasuredDevicesNums()
        {
            string deviceTypePath = $"{Properties.Settings.Default.StartSavePath}{EnumInfo.GetDescription(DeviceType)}";
            if (Directory.Exists(deviceTypePath))
            {
                ComboBoxDevicesNums.Clear();
                new DirectoryInfo(deviceTypePath).GetDirectories().ToList()
                    .ForEach(dir =>
                    {
                        if (Regex.IsMatch(dir.Name, Properties.Settings.Default.DeviceNumRegex))
                        {
                            ComboBoxDevicesNums.Add(dir.Name);
                        }
                    });
            }
        }

        private void SelectMeasuredDevicesNum()
        {
            string deviceNumPath = $@"{Properties.Settings.Default.StartSavePath}{EnumInfo.GetDescription(DeviceType)}\{DeviceNum}";
            if (Directory.Exists(deviceNumPath))
            {
                ComboBoxProvAfterList.Clear();
                new DirectoryInfo(deviceNumPath).GetDirectories().ToList()
                    .ForEach(dir => ComboBoxProvAfterList.Add(dir.Name.Split('_')[0]));
            }
        }

        private void SelectProvAfter()
        {
            string deviceNumPath = $@"{Properties.Settings.Default.StartSavePath}{EnumInfo.GetDescription(DeviceType)}\{DeviceNum}";
            if (Directory.Exists(deviceNumPath))
            {
                Conclusion = new DirectoryInfo(deviceNumPath).GetDirectories().ToList()
                    .Find(dir => dir.Name.Split('_')[0].Equals(ProvAfter))
                    .Name.Split('_')[1];
            }
        }

        private bool DirPathToCheckExists(string path)
        {
            List<string> temp = path.Split('_').ToList();
            temp.Remove(temp.Last());
            string dirNameToCheckExists = string.Join("_", temp);

            return Directory.GetParent(path).GetDirectories()
                .Where(dirInfo => dirInfo.FullName.Contains(dirNameToCheckExists))
                .Any();
        }

    }
}
