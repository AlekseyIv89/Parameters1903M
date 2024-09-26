using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov15_Model : BaseModel, IProvModel
    {
        // Температурный коэффициент
        private readonly Parameter temperatureKoefficient;

        public List<TemperatureKoefficientInitialData> InitialData { get; set; }
        public List<TemperatureKoefficientCalculatedData> CalculatedData { get; set; }

        public Prov15_Model(Parameter temperatureKoefficient)
        {
            this.temperatureKoefficient = temperatureKoefficient;
            ReadData();
        }

        public void ClearAllData()
        {
            for (int i = 0; i < InitialData.Count; i++)
            {
                InitialData[i].Clear();
            }

            for (int i = 0; i < CalculatedData.Count; i++)
            {
                CalculatedData[i].Clear();
            }
        }

        public void CalculateData()
        {
            if (!string.IsNullOrEmpty(InitialData[0].I0T1ValueStr) && string.IsNullOrEmpty(CalculatedData[0].I0T1AverageValueStr))
            {
                double iSum = 0.0;
                for (int i = 1; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I0T1Value;
                }
                CalculatedData[0].I0T1AverageValue = iSum / 4;
                return;
            }

            if (!string.IsNullOrEmpty(InitialData[0].I4T1ValueStr) && string.IsNullOrEmpty(CalculatedData[0].I4T1AverageValueStr))
            {
                double iSum = 0.0;
                for (int i = 1; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I4T1Value;
                }
                CalculatedData[0].I4T1AverageValue = iSum / 4;
                return;
            }

            if (!string.IsNullOrEmpty(InitialData[0].I4T2ValueStr) && string.IsNullOrEmpty(CalculatedData[0].I4T2AverageValueStr))
            {
                double iSum = 0.0;
                for (int i = 1; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I4T2Value;
                }
                CalculatedData[0].I4T2AverageValue = iSum / 4;
                return;
            }

            if (!string.IsNullOrEmpty(InitialData[0].I4T1ValueStr) && string.IsNullOrEmpty(CalculatedData[0].I4T1AverageValueStr))
            {
                double iSum = 0.0;
                for (int i = 1; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I4T1Value;
                }
                CalculatedData[0].I4T1AverageValue = iSum / 4;
                return;
            }

            if (!string.IsNullOrEmpty(CalculatedData[0].I0T1AverageValueStr) &&
                !string.IsNullOrEmpty(CalculatedData[0].I4T1AverageValueStr) &&
                !string.IsNullOrEmpty(CalculatedData[0].I4T2AverageValueStr) &&
                !string.IsNullOrEmpty(CalculatedData[0].I4T1AverageValueStr))
            {
                double t1 = 30;
                double t2 = 50;

                CalculatedData[0].IT1Value = CalculatedData[0].I4T1AverageValue - CalculatedData[0].I0T1AverageValue;
                CalculatedData[0].IT2Value = CalculatedData[0].I4T2AverageValue - CalculatedData[0].I0T2AverageValue;
                CalculatedData[0].KtValue = 2 * (CalculatedData[0].IT2Value - CalculatedData[0].IT1Value) * 100
                    / ((CalculatedData[0].IT2Value + CalculatedData[0].IT1Value) * (t2 - t1));

                temperatureKoefficient.Value = CalculatedData[0].KtValue;
            }

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new List<TemperatureKoefficientInitialData>
            {
                new TemperatureKoefficientInitialData(),
                new TemperatureKoefficientInitialData(),
                new TemperatureKoefficientInitialData(),
                new TemperatureKoefficientInitialData(),
                new TemperatureKoefficientInitialData()
            };

            CalculatedData = new List<TemperatureKoefficientCalculatedData>
            {
                new TemperatureKoefficientCalculatedData()
            };

            if (!string.IsNullOrWhiteSpace(temperatureKoefficient.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{temperatureKoefficient.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<TemperatureKoefficientInitialData> listInitData = JsonConvert.DeserializeObject<List<TemperatureKoefficientInitialData>>(fileData[0]);
                List<TemperatureKoefficientCalculatedData> listCalcData = JsonConvert.DeserializeObject<List<TemperatureKoefficientCalculatedData>>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].I0T1Value = listInitData[i].I0T1Value;
                    InitialData[i].I4T1Value = listInitData[i].I4T1Value;
                    InitialData[i].I0T2Value = listInitData[i].I0T2Value;
                    InitialData[i].I4T2Value = listInitData[i].I4T2Value;
                }

                CalculatedData[0].I0T1AverageValue = listCalcData[0].I0T1AverageValue;
                CalculatedData[0].I4T1AverageValue = listCalcData[0].I4T1AverageValue;
                CalculatedData[0].I0T2AverageValue = listCalcData[0].I0T2AverageValue;
                CalculatedData[0].I4T2AverageValue = listCalcData[0].I4T2AverageValue;
                CalculatedData[0].IT1Value = listCalcData[0].IT1Value;
                CalculatedData[0].IT2Value = listCalcData[0].IT2Value;
                CalculatedData[0].KtValue = listCalcData[0].KtValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{temperatureKoefficient.Num}", false);
        }
    }

    internal class TemperatureKoefficientInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double i0T1Value;
        private string i0T1ValueStr;

        private double i4T1Value;
        private string i4T1ValueStr;

        private double i0T2Value;
        private string i0T2ValueStr;

        private double i4T2Value;
        private string i4T2ValueStr;

        public double I0T1Value
        {
            get => i0T1Value;
            set
            {
                i0T1Value = value;
                I0T1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I0T1ValueStr
        {
            get => i0T1ValueStr;
            set
            {
                i0T1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I4T1Value
        {
            get => i4T1Value;
            set
            {
                i4T1Value = value;
                I4T1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I4T1ValueStr
        {
            get => i4T1ValueStr;
            set
            {
                i4T1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I0T2Value
        {
            get => i0T2Value;
            set
            {
                i0T2Value = value;
                I0T2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I0T2ValueStr
        {
            get => i0T2ValueStr;
            set
            {
                i0T2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I4T2Value
        {
            get => i4T2Value;
            set
            {
                i4T2Value = value;
                I4T2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I4T2ValueStr
        {
            get => i4T2ValueStr;
            set
            {
                i4T2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            I0T1Value = default;
            I0T1ValueStr = default;
            I4T1Value = default;
            I4T1ValueStr = default;
            I0T2Value = default;
            I0T2ValueStr = default;
            I4T2Value = default;
            I4T2ValueStr = default;
        }
    }

    internal class TemperatureKoefficientCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 3;
        private readonly int kt_digits = 4;

        private double i0T1AverageValue;
        private string i0T1AverageValueStr;

        private double i4T1AverageValue;
        private string i4T1AverageValueStr;

        private double i0T2AverageValue;
        private string i0T2AverageValueStr;

        private double i4T2AverageValue;
        private string i4T2AverageValueStr;

        private double iT1Value;
        private string iT1ValueStr;

        private double iT2Value;
        private string iT2ValueStr;

        private double ktValue;
        private string ktValueStr;

        public double I0T1AverageValue
        {
            get => i0T1AverageValue;
            set
            {
                i0T1AverageValue = value;
                I0T1AverageValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I0T1AverageValueStr
        {
            get => i0T1AverageValueStr;
            set
            {
                i0T1AverageValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I4T1AverageValue
        {
            get => i4T1AverageValue;
            set
            {
                i4T1AverageValue = value;
                I4T1AverageValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I4T1AverageValueStr
        {
            get => i4T1AverageValueStr;
            set
            {
                i4T1AverageValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I0T2AverageValue
        {
            get => i0T2AverageValue;
            set
            {
                i0T2AverageValue = value;
                I0T2AverageValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I0T2AverageValueStr
        {
            get => i0T2AverageValueStr;
            set
            {
                i0T2AverageValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I4T2AverageValue
        {
            get => i4T2AverageValue;
            set
            {
                i4T2AverageValue = value;
                I4T2AverageValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I4T2AverageValueStr
        {
            get => i4T2AverageValueStr;
            set
            {
                i4T2AverageValueStr = value;
                OnPropertyChanged();
            }
        }

        public double IT1Value
        {
            get => iT1Value;
            set
            {
                iT1Value = value;
                IT1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IT1ValueStr
        {
            get => iT1ValueStr;
            set
            {
                iT1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double IT2Value
        {
            get => iT2Value;
            set
            {
                iT2Value = value;
                IT2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IT2ValueStr
        {
            get => iT2ValueStr;
            set
            {
                iT2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double KtValue
        {
            get => ktValue;
            set
            {
                
                ktValue = value;
                KtValueStr = Math.Round(value, kt_digits, MidpointRounding.AwayFromZero).ToString($"F{kt_digits}");
            }
        }

        public string KtValueStr
        {
            get => ktValueStr;
            set
            {
                ktValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            I0T1AverageValue = default;
            I0T1AverageValueStr = default;
            I4T1AverageValue = default;
            I4T1AverageValueStr = default;
            I0T2AverageValue = default;
            I0T2AverageValueStr = default;
            I4T2AverageValue = default;
            I4T2AverageValueStr = default;
            IT1Value = default;
            IT1ValueStr = default;
            IT2Value = default;
            IT2ValueStr = default;
            KtValue = default;
            KtValueStr = default;
        }
    }
}
