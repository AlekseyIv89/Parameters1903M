using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using System.Collections.Generic;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class Prov9_ViewModel : BaseViewModel
    {
        public string Title => Parameter.Name;

        public Parameter Parameter { get; }

        private const string BUTTON_SAVE = "Сохранить";

        public List<string> CbData { get; }

        private readonly Prov9_WindowService prov9_WindowService;
        public ICommand Prov9_WindowCloseCommand { get; }
        public ICommand ButtonSaveCommand { get; }

        public Prov9_Model Prov9_Model { get; }

        public Prov9_ViewModel(Parameter parameter)
        {
            Parameter = parameter;
            ButtonContent = BUTTON_SAVE;

            CbData = new List<string>
            {
                "Соответствует",
                "Не соответств."
            };

            Prov9_Model = new Prov9_Model(Parameter);
            Prov9_Model.CalculatedData.ThermalFuseFunctionalityValueStr = "Не соответств.";

            prov9_WindowService = new Prov9_WindowService();

            Prov9_WindowCloseCommand = new RelayCommand(param => prov9_WindowService.Close(param), x => true);
            ButtonSaveCommand = new RelayCommand(param => ParameterMeasure(), x => true);
        }

        private void ParameterMeasure()
        {
            #region Начало измерения
            //---------------------------- Начало измерения ----------------------------
            Prov9_Model.CalculateData();
            //---------------------------- Конец измерения -----------------------------
            #endregion
        }
    }
}
