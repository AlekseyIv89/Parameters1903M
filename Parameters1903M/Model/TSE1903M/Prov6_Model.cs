using Newtonsoft.Json;
using Parameters1903M.Util.Data;
using Parameters1903M.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov6_Model : BaseModel, IProvModel
    {
        // Стабильность положения ОС за 120 минут 
        private readonly Parameter osDriftFluctuationAndInstability;

        public OsDriftFluctuationInitialData InitialData { get; private set; }
        public OsDriftFluctuationCalculatedData CalculatedData { get; private set; }

        public Prov6_Model(Parameter osDriftFluctuation)
        {
            this.osDriftFluctuationAndInstability = osDriftFluctuation;
            ReadData();
        }

        public void ClearAllData()
        {
            InitialData.Clear();
            CalculatedData.Clear();
        }

        public void CalculateData()
        {
            // TODO

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new OsDriftFluctuationInitialData();
            CalculatedData = new OsDriftFluctuationCalculatedData();

            if (!string.IsNullOrWhiteSpace(osDriftFluctuationAndInstability.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{osDriftFluctuationAndInstability.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                OsDriftFluctuationInitialData initData = JsonConvert.DeserializeObject<OsDriftFluctuationInitialData>(fileData[0]);
                OsDriftFluctuationCalculatedData calcData = JsonConvert.DeserializeObject<OsDriftFluctuationCalculatedData>(fileData[1]);

                // TODO
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{osDriftFluctuationAndInstability.Num}", false);
        }
    }

    internal class OsDriftFluctuationInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double iValue;
        private string iValueStr;

        private double iMinValue;
        private string iMinValueStr;

        private double iMaxValue;
        private string iMaxValueStr;

        private double deltaIMaxValue;
        private string deltaIMaxValueStr;

        public double IValue
        {
            get => iValue;
            set
            {
                iValue = value;
                IValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IValueStr
        {
            get => iValueStr;
            private set
            {
                iValueStr = value;
                OnPropertyChanged();
            }
        }

        public double IMinValue
        {
            get => iMinValue;
            set
            {
                iMinValue = value;
                IMinValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IMinValueStr
        {
            get => iMinValueStr;
            private set
            {
                iMinValueStr = value;
                OnPropertyChanged();
            }
        }

        public double IMaxValue
        {
            get => iMaxValue;
            set
            {
                iMaxValue = value;
                IMaxValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IMaxValueStr
        {
            get => iMaxValueStr;
            private set
            {
                iMaxValueStr = value;
                OnPropertyChanged();
            }
        }

        public double DeltaIMaxValue
        {
            get => deltaIMaxValue;
            set
            {
                deltaIMaxValue = value;
                DeltaIMaxValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string DeltaIMaxValueStr
        {
            get => deltaIMaxValueStr;
            private set
            {
                deltaIMaxValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            IValue = default;
            IValueStr = default;
            IMinValue = default;
            IMinValueStr = default;
            IMaxValue = default;
            IMaxValueStr = default;
            DeltaIMaxValue = default;
            DeltaIMaxValueStr = default;
        }
    }

    internal class OsDriftFluctuationCalculatedData : BaseModel, IProvData
    {
        // TODO

        public void Clear()
        {
            // TODO
        }
    }
}
