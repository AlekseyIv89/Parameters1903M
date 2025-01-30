using Newtonsoft.Json;
using Parameters1903M.Model;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View;
using Parameters1903M.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class MainWindowService : IWindowService
    {
        public MainWindow GetMainWindow()
        {
            return (MainWindow)Application.Current.MainWindow;
        }

        public MainViewModel GetMainWindowViewModel()
        {
            return (MainViewModel)GetMainWindow().DataContext;
        }

        public void Close()
        {
            GetMultimeter().Disconnect();
            Application.Current.MainWindow.Close();
        }

        public void Close(object param)
        {
            if (GlobalVars.IsMeasureRunning)
            {
                CancelEventArgs cancelEventArgs = (CancelEventArgs)param;
                cancelEventArgs.Cancel = true;

                string message = "Невозможно закрыть программу, т.к. в настоящее время проводится измерение параметра.";
                MessageBox.Show(Application.Current.MainWindow, message, ProgramInfo.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        public void OpenDialog(object param)
        {

        }

        public Parameter GetParameterByName(string name)
        {
            List<Parameter> parameters = GetMainWindowViewModel().Parameters;
            return parameters.Find(param => param.Name.Equals(name));
        }

        public void ListViewDoubleClick(object param)
        {
            if (param is Parameter parameter)
            {
                switch (parameter.Name)
                {
                    case "Выход на режим":
                        new Prov0_WindowService().OpenDialog(param);
                        break;
                    case "Масштабный коэффициент, Ig":
                        new Prov1_WindowService().OpenDialog(param);
                        break;
                    case "Крутизна характеристики ДУ, Sду":
                        new Prov2_WindowService().OpenDialog(param);
                        break;
                    case "Дрейф нуля ДУ":
                        if (!IsAdditionalParameterMeasureNeeded("Крутизна характеристики ДУ, Sду"))
                        {
                            new Prov3_WindowService().OpenDialog(param);
                        }
                        break;
                    case "Угол крепления механических упоров":
                        if (!IsAdditionalParameterMeasureNeeded("Крутизна характеристики ДУ, Sду"))
                        {
                            new Prov5_WindowService().OpenDialog(param);
                        }
                        break;
                    case "Флюктуация дрейфа ТОС":
                    case "Нестабильность дрейфа ТОС":
                        new Prov6_WindowService().OpenDialog(param);
                        break;
                    case "Переменная составляющая вых. напряжения ДУ, U~":
                    case "Сопротивление обмотки ОС, Rос":
                        new InputDialogWindowService().OpenDialog(param);
                        break;
                    case "Стабильность положения ОЧ":
                        if (!IsAdditionalParameterMeasureNeeded("Масштабный коэффициент, Ig"))
                        {
                            new Prov8_WindowService().OpenDialog(param);
                        }
                        break;
                    case "Функц. способность термопредохр.":
                        new Prov9_WindowService().OpenDialog(param);
                        break;
                    case "Непараллельность ОЧ базовой плоскости":
                        if (!IsAdditionalParameterMeasureNeeded("Масштабный коэффициент, Ig"))
                        {
                            new Prov10_WindowService().OpenDialog(param);
                        }
                        break;
                    case "Неперпендикулярность ОЧ опорной плоскости":
                        if (!IsAdditionalParameterMeasureNeeded("Крутизна характеристики ДУ, Sду"))
                        {
                            new Prov16_WindowService().OpenDialog(param);
                        }
                        break;
                    case "Температурный коэффициент, Kt":
                        new Prov15_WindowService().OpenDialog(param);
                        break;
                    case "МК в положении маятником вниз":
                        new Prov14_WindowService().OpenDialog(param);
                        break;
                    case "Время установления выходной информации, ВУВИ":
                        new Prov13_WindowService().OpenDialog(param);
                        break;
                    case "Ток отрыва ПС от упора":
                        new Prov17_WindowService().OpenDialog(param);
                        break;
                    case "Время схода ПС с упора":
                        new Prov18_WindowService().OpenDialog(param);
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrWhiteSpace(parameter.StrValue))
                {
                    ReportDataSave();
                }
            }
        }

        private bool IsAdditionalParameterMeasureNeeded(string parameterFullName)
        {
            Parameter parameterToFind = GetMainWindowViewModel().Parameters.Find(prm => prm.Name.Equals(parameterFullName));

            if (string.IsNullOrEmpty(parameterToFind.StrValue))
            {
                string message = $"Перед проведением проверки сначала необходимо произвести проверку параметра \"{parameterToFind.Name}\"";
                MessageBox.Show(Application.Current.MainWindow, message, ProgramInfo.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            return false;
        }

        public IMeasure GetMultimeter()
        {
            return GetMainWindowViewModel().Multimeter;
        }

        private void ReportDataSave()
        {
            DeviceData deviceData = GetMainWindowViewModel().DeviceData;
            string jsonDeviceData = JsonConvert.SerializeObject(deviceData);
            Data.Save(jsonDeviceData, $"{deviceData.DeviceNum}_header");

            List<Parameter> parameters = new List<Parameter>(GetMainWindowViewModel().Parameters);
            string jsonParameters = JsonConvert.SerializeObject(parameters);
            Data.Save(jsonParameters, $"{deviceData.DeviceNum}_params");
        }

        public async Task<DeviceData> ReportHeaderReadAsync(string filePath)
        {
            string data = await Data.Read(filePath + "_header");
            return JsonConvert.DeserializeObject<DeviceData>(data);
        }

        public async Task<List<Parameter>> ReportParamsRead(string filePath)
        {
            string data = await Data.Read(filePath + "_params");
            return JsonConvert.DeserializeObject<List<Parameter>>(data);
        }
    }
}
