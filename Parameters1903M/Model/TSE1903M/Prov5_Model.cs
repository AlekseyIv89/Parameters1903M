using Newtonsoft.Json;
using Parameters1903M.Service;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov5_Model : BaseModel, IProvModel
    {
        // Угол крепления механических упоров
        private readonly Parameter mechanicalStopAngle;

        // Крутизна характеристики ДУ
        private AngleSensorSlopeCalculatedData angleSensorSlopeCalculatedData;

        public MechanicalStopAngleInitialData InitialData { get; set; }
        public MechanicalStopAngleCalculatedData CalculatedData { get; set; }

        public Prov5_Model(Parameter mechanicalStopAngle)
        {
            this.mechanicalStopAngle = mechanicalStopAngle;
            GetAngleSensorSlopeCalculatedData();
            ReadData();
        }

        private async void GetAngleSensorSlopeCalculatedData()
        {
            angleSensorSlopeCalculatedData = await new Prov2_Model(new MainWindowService().GetParameterByName("Крутизна характеристики ДУ, Sду")).GetAngleSensorSlopeCalculatedData();
        }

        public void ClearAllData()
        {
            InitialData.Clear();
            CalculatedData.Clear();
        }

        public void CalculateData()
        {
            double plusS = angleSensorSlopeCalculatedData.PlusSduValue;
            CalculatedData.PlusAlphaValue = InitialData.Udy1Value / plusS;

            if (!string.IsNullOrWhiteSpace(InitialData.Udy2ValueStr))
            {
                double minusS = angleSensorSlopeCalculatedData.MinusSduValue;
                CalculatedData.MinusAlphaValue = InitialData.Udy2Value / minusS;
                CalculatedData.TotalAlphaValueStr = $"{CalculatedData.PlusAlphaValueStr};{CalculatedData.MinusAlphaValueStr}";

                mechanicalStopAngle.StrValue = CalculatedData.TotalAlphaValueStr;

                WriteData();
            }
        }

        private async void ReadData()
        {
            InitialData = new MechanicalStopAngleInitialData();
            CalculatedData = new MechanicalStopAngleCalculatedData();

            if (!string.IsNullOrWhiteSpace(mechanicalStopAngle.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{mechanicalStopAngle.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                MechanicalStopAngleInitialData initData = JsonConvert.DeserializeObject<MechanicalStopAngleInitialData>(fileData[0]);
                MechanicalStopAngleCalculatedData calcData = JsonConvert.DeserializeObject<MechanicalStopAngleCalculatedData>(fileData[1]);

                InitialData.Udy1Value = initData.Udy1Value;
                InitialData.Udy2Value = initData.Udy2Value;

                CalculatedData.PlusAlphaValue = calcData.PlusAlphaValue;
                CalculatedData.MinusAlphaValue = calcData.MinusAlphaValue;
                CalculatedData.TotalAlphaValueStr = calcData.TotalAlphaValueStr;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{mechanicalStopAngle.Num}", false);
        }
    }

    internal class MechanicalStopAngleInitialData : BaseModel, IProvData
    {
        private readonly int digits = 2;

        private double udy1Value;
        private string udy1ValueStr;

        private double udy2Value;
        private string udy2ValueStr;

        public double Udy1Value
        {
            get => udy1Value;
            set
            {
                udy1Value = value;
                Udy1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Udy1ValueStr
        {
            get => udy1ValueStr;
            private set
            {
                udy1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Udy2Value
        {
            get => udy2Value;
            set
            {
                udy2Value = value;
                Udy2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Udy2ValueStr
        {
            get => udy2ValueStr;
            private set
            {
                udy2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            Udy1Value = default;
            Udy1ValueStr = default;
            Udy2Value = default;
            Udy2ValueStr = default;
        }
    }

    internal class MechanicalStopAngleCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 1;

        private double plusAlphaValue;
        private string plusAlphaValueStr;

        private double minusAlphaValue;
        private string minusAlphaValueStr;

        private string totalAlphaValueStr;

        public double PlusAlphaValue
        {
            get => plusAlphaValue;
            set
            {
                plusAlphaValue = value;
                PlusAlphaValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string PlusAlphaValueStr
        {
            get => plusAlphaValueStr;
            private set
            {
                plusAlphaValueStr = value;
                OnPropertyChanged();
            }
        }

        public double MinusAlphaValue
        {
            get => minusAlphaValue;
            set
            {
                minusAlphaValue = value;
                MinusAlphaValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MinusAlphaValueStr
        {
            get => minusAlphaValueStr;
            private set
            {
                minusAlphaValueStr = value;
                OnPropertyChanged();
            }
        }

        public string TotalAlphaValueStr
        {
            get => totalAlphaValueStr;
            set
            {
                totalAlphaValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            PlusAlphaValue = default;
            PlusAlphaValueStr = default;
            MinusAlphaValue = default;
            MinusAlphaValueStr = default;
            TotalAlphaValueStr = default;
        }
    }
}
