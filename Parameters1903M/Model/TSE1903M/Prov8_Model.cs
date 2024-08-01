using Newtonsoft.Json;
using Parameters1903M.Service;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov8_Model : BaseModel, IProvModel
    {
        // Стабильность положения ОС за 120 минут 
        private readonly Parameter stabilityOfPositionOS;

        // Масштабный коэффициент
        private ScaleFactorCalculatedData scaleFactorCalculatedData;

        public StabilityOfPositionOSInitialData InitialData { get; private set; }
        public StabilityOfPositionOSCalculatedData CalculatedData { get; private set; }

        private List<double> tempData;

        public Prov8_Model(Parameter stabilityOfPositionOS)
        {
            this.stabilityOfPositionOS = stabilityOfPositionOS;
            GetAngleSensorSlopeCalculatedData();
            ReadData();
        }

        private async void GetAngleSensorSlopeCalculatedData()
        {
            scaleFactorCalculatedData = await new Prov1_Model(new MainWindowService().GetParameterByName("Масштабный коэффициент, Ig")).GetScaleFactorCalculatedData();
        }

        public void ClearAllData()
        {
            InitialData.Clear();
            CalculatedData.Clear();

            tempData = new List<double>();
        }

        public void CalculateDataWhileMeasureRunning(double inValue)
        {
            tempData.Add(inValue);

            if (tempData.Count > 1)
            {
                InitialData.IMaxValue = tempData.Max();
                InitialData.IMinValue = tempData.Min();

                CalculatedData.DeltaIstValue = Math.Abs(InitialData.IMaxValue - InitialData.IMinValue) / 
                    (5E-6 * scaleFactorCalculatedData.IgValue);
            }
        }

        public void CalculateData()
        {
            stabilityOfPositionOS.Value = CalculatedData.DeltaIstValue;

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new StabilityOfPositionOSInitialData();
            CalculatedData = new StabilityOfPositionOSCalculatedData();
            tempData = new List<double>();

            if (!string.IsNullOrWhiteSpace(stabilityOfPositionOS.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{stabilityOfPositionOS.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                StabilityOfPositionOSInitialData initData = JsonConvert.DeserializeObject<StabilityOfPositionOSInitialData>(fileData[0]);
                StabilityOfPositionOSCalculatedData calcData = JsonConvert.DeserializeObject<StabilityOfPositionOSCalculatedData>(fileData[1]);

                InitialData.IValue = initData.IValue;
                InitialData.IMinValue = initData.IMinValue;
                InitialData.IMaxValue = initData.IMaxValue;

                CalculatedData.DeltaIstValue = calcData.DeltaIstValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{stabilityOfPositionOS.Num}", false);
        }
    }

    internal class StabilityOfPositionOSInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double iValue;
        private string iValueStr;

        private double iMinValue;
        private string iMinValueStr;

        private double iMaxValue;
        private string iMaxValueStr;

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

        public void Clear()
        {
            IValue = default;
            IValueStr = default;
            IMinValue = default;
            IMinValueStr = default;
            IMaxValue = default;
            IMaxValueStr = default;
        }
    }

    internal class StabilityOfPositionOSCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double deltaIstValue;
        private string deltaIstValueStr;

        public double DeltaIstValue
        {
            get => deltaIstValue;
            set
            {
                deltaIstValue = value;
                DeltaIstValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string DeltaIstValueStr
        {
            get => deltaIstValueStr;
            private set
            {
                deltaIstValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            DeltaIstValue = default;
            DeltaIstValueStr = default;
        }
    }
}
