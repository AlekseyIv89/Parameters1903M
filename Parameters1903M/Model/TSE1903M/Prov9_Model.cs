using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov9_Model : BaseModel, IProvModel
    {
        // Функциональная способность термопредохранителя
        private readonly Parameter thermalFuseFunctionality;

        public ThermalFuseFunctionalityCalculatedData CalculatedData { get; set; }

        public Prov9_Model(Parameter thermalFuseFunctionality)
        {
            this.thermalFuseFunctionality = thermalFuseFunctionality;
            ReadData();
        }

        public void CalculateData()
        {
            thermalFuseFunctionality.StrValue = CalculatedData.ThermalFuseFunctionalityValueStr;

            WriteData();
        }

        private async void ReadData()
        {
            CalculatedData = new ThermalFuseFunctionalityCalculatedData();

            if (!string.IsNullOrWhiteSpace(thermalFuseFunctionality.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{thermalFuseFunctionality.Num}");

                string data = await Data.Read(filePath);
                ThermalFuseFunctionalityCalculatedData calcData = JsonConvert.DeserializeObject<ThermalFuseFunctionalityCalculatedData>(data);

                CalculatedData.ThermalFuseFunctionalityValueStr = calcData.ThermalFuseFunctionalityValueStr;
            }
        }

        private void WriteData()
        {
            Data.Save(JsonConvert.SerializeObject(CalculatedData), $@"{GlobalVars.DeviceNum}_{thermalFuseFunctionality.Num}", false);
        }
    }

    internal class ThermalFuseFunctionalityCalculatedData : BaseModel
    {
        private string thermalFuseFunctionalityValueStr;

        public string ThermalFuseFunctionalityValueStr
        {
            get => thermalFuseFunctionalityValueStr;
            set
            {
                thermalFuseFunctionalityValueStr = value;
                OnPropertyChanged();
            }
        }
    }

}
