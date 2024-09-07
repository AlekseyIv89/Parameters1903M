using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov14_Model : BaseModel, IProvModel
    {
        // Масштабный коэффициент
        private readonly Parameter scaleFactorPendulumDown;

        public List<ScaleFactorPendulumDownInitialData> InitialData { get; private set; }
        public ScaleFactorPendulumDownCalculatedData CalculatedData { get; private set; }

        public Prov14_Model(Parameter scaleFactorPendulumDown)
        {
            this.scaleFactorPendulumDown = scaleFactorPendulumDown;
            ReadData();
        }

        public void ClearAllData()
        {
            for (int i = 0; i < InitialData.Count; i++)
            {
                InitialData[i].Clear();
            }

            CalculatedData.Clear();
        }

        public void CalculateData()
        {
            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].ScaleFactorPendulumDownValue1Str))
            {
                CalculatedData.Ioc1Value = Ios1(InitialData);
            }
            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].ScaleFactorPendulumDownValue2Str))
            {
                CalculatedData.Ioc2Value = Ios2(InitialData);
                CalculatedData.Ig90Value = Ig(CalculatedData);

                scaleFactorPendulumDown.Value = CalculatedData.Ig90Value;

                WriteData();
            }
        }

        private double Ios1(List<ScaleFactorPendulumDownInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].ScaleFactorPendulumDownValue1;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private double Ios2(List<ScaleFactorPendulumDownInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].ScaleFactorPendulumDownValue2;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private double Ig(ScaleFactorPendulumDownCalculatedData calculatedData)
        {
            return (Math.Abs(calculatedData.Ioc1Value) + Math.Abs(calculatedData.Ioc2Value)) / 2.0 * Math.Sin(4 * Math.PI / 180);
        }

        private async void ReadData()
        {
            InitialData = new List<ScaleFactorPendulumDownInitialData>
            {
                new ScaleFactorPendulumDownInitialData(),
                new ScaleFactorPendulumDownInitialData(),
                new ScaleFactorPendulumDownInitialData(),
                new ScaleFactorPendulumDownInitialData(),
                new ScaleFactorPendulumDownInitialData()
            };

            CalculatedData = new ScaleFactorPendulumDownCalculatedData();

            if (!string.IsNullOrWhiteSpace(scaleFactorPendulumDown.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{scaleFactorPendulumDown.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<ScaleFactorPendulumDownInitialData> listInitData = JsonConvert.DeserializeObject<List<ScaleFactorPendulumDownInitialData>>(fileData[0]);
                ScaleFactorPendulumDownCalculatedData calcData = JsonConvert.DeserializeObject<ScaleFactorPendulumDownCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].ScaleFactorPendulumDownValue1 = listInitData[i].ScaleFactorPendulumDownValue1;
                    InitialData[i].ScaleFactorPendulumDownValue2 = listInitData[i].ScaleFactorPendulumDownValue2;
                }

                CalculatedData.Ioc1Value = calcData.Ioc1Value;
                CalculatedData.Ioc2Value = calcData.Ioc2Value;
                CalculatedData.Ig90Value = calcData.Ig90Value;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{scaleFactorPendulumDown.Num}", false);
        }
    }

    internal class ScaleFactorPendulumDownInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double scaleFactorPendulumDownValue1;
        private string scaleFactorPendulumDownValue1Str;

        private double scaleFactorPendulumDownValue2;
        private string scaleFactorPendulumDownValue2Str;

        public double ScaleFactorPendulumDownValue1
        {
            get => scaleFactorPendulumDownValue1;
            set
            {
                scaleFactorPendulumDownValue1 = value;
                ScaleFactorPendulumDownValue1Str = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ScaleFactorPendulumDownValue1Str
        {
            get => scaleFactorPendulumDownValue1Str;
            private set
            {
                scaleFactorPendulumDownValue1Str = value;
                OnPropertyChanged();
            }
        }

        public double ScaleFactorPendulumDownValue2
        {
            get => scaleFactorPendulumDownValue2;
            set
            {
                scaleFactorPendulumDownValue2 = value;
                ScaleFactorPendulumDownValue2Str = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ScaleFactorPendulumDownValue2Str
        {
            get => scaleFactorPendulumDownValue2Str;
            private set
            {
                scaleFactorPendulumDownValue2Str = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            ScaleFactorPendulumDownValue1 = default;
            ScaleFactorPendulumDownValue1Str = default;
            ScaleFactorPendulumDownValue2 = default;
            ScaleFactorPendulumDownValue2Str = default;
        }
    }

    internal class ScaleFactorPendulumDownCalculatedData : BaseModel, IProvData
    {
        private double ioc1Value;
        private string ioc1ValueStr;

        private double ioc2Value;
        private string ioc2ValueStr;

        private double ig90Value;
        private string ig90ValueStr;

        public double Ioc1Value
        {
            get => ioc1Value;
            set
            {
                ioc1Value = value;
                int digits = 3;
                Ioc1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Ioc1ValueStr
        {
            get => ioc1ValueStr;
            private set
            {
                ioc1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Ioc2Value
        {
            get => ioc2Value;
            set
            {
                ioc2Value = value;
                int digits = 3;
                Ioc2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Ioc2ValueStr
        {
            get => ioc2ValueStr;
            private set
            {
                ioc2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Ig90Value
        {
            get => ig90Value;
            set
            {
                ig90Value = value;
                int digits = 1;
                Ig90ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Ig90ValueStr
        {
            get => ig90ValueStr;
            private set
            {
                ig90ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            Ioc1Value = default;
            Ioc1ValueStr = default;
            Ioc2Value = default;
            Ioc2ValueStr = default;
            Ig90Value = default;
            Ig90ValueStr = default;
        }
    }
}
