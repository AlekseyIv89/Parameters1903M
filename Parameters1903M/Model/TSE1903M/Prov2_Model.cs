using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov2_Model : BaseModel, IProvModel
    {
        // Крутизна характеристики ДУ
        private readonly Parameter angleSensorSlope;

        public List<AngleSensorSlopeInitialData> InitialData { get; set; }
        public AngleSensorSlopeCalculatedData CalculatedData { get; set; }

        public Prov2_Model(Parameter angleSensorSlope)
        {
            this.angleSensorSlope = angleSensorSlope;
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
            CalculatedData.PlusSduValue = Udy1(InitialData) / 5;
            CalculatedData.MinusSduValue = Udy2(InitialData) / 5;
            CalculatedData.SduValue = (Math.Abs(CalculatedData.PlusSduValue) + Math.Abs(CalculatedData.MinusSduValue)) / 2;

            angleSensorSlope.Value = CalculatedData.SduValue;

            WriteData();
        }

        private double Udy1(List<AngleSensorSlopeInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].Udy1Value;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private double Udy2(List<AngleSensorSlopeInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].Udy2Value;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private async void ReadData()
        {
            InitialData = new List<AngleSensorSlopeInitialData>
            {
                new AngleSensorSlopeInitialData(),
                new AngleSensorSlopeInitialData(),
                new AngleSensorSlopeInitialData(),
                new AngleSensorSlopeInitialData(),
                new AngleSensorSlopeInitialData()
            };
            CalculatedData = new AngleSensorSlopeCalculatedData();

            if (!string.IsNullOrWhiteSpace(angleSensorSlope.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{angleSensorSlope.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<AngleSensorSlopeInitialData> initData = JsonConvert.DeserializeObject<List<AngleSensorSlopeInitialData>>(fileData[0]);
                AngleSensorSlopeCalculatedData calcData = JsonConvert.DeserializeObject<AngleSensorSlopeCalculatedData>(fileData[1]);

                for (int i = 0; i < initData.Count; i++)
                {
                    InitialData[i].Udy1Value = initData[i].Udy1Value;
                    InitialData[i].Udy2Value = initData[i].Udy2Value;
                }                

                CalculatedData.ZeroSduValue= calcData.ZeroSduValue;
                CalculatedData.PlusSduValue = calcData.PlusSduValue;
                CalculatedData.MinusSduValue = calcData.MinusSduValue;
                CalculatedData.SduValue = calcData.SduValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{angleSensorSlope.Num}", false);
        }
    }

    internal class AngleSensorSlopeInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

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

    internal class AngleSensorSlopeCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 0;

        private double zeroSduValue;
        private string zeroSduValueStr;

        private double plusSduValue;
        private string plusSduValueStr;

        private double minusSduValue;
        private string minusSduValueStr;

        private double sduValue;
        private string sduValueStr;

        public double ZeroSduValue
        {
            get => zeroSduValue;
            set
            {
                zeroSduValue = value;
                ZeroSduValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ZeroSduValueStr
        {
            get => zeroSduValueStr;
            private set
            {
                zeroSduValueStr = value;
                OnPropertyChanged();
            }
        }

        public double PlusSduValue
        {
            get => plusSduValue;
            set
            {
                plusSduValue = value;
                PlusSduValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string PlusSduValueStr
        {
            get => plusSduValueStr;
            private set
            {
                plusSduValueStr = value;
                OnPropertyChanged();
            }
        }

        public double MinusSduValue
        {
            get => minusSduValue;
            set
            {
                minusSduValue = value;
                MinusSduValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MinusSduValueStr
        {
            get => minusSduValueStr;
            private set
            {
                minusSduValueStr = value;
                OnPropertyChanged();
            }
        }

        public double SduValue
        {
            get => sduValue;
            set
            {
                sduValue = value;
                SduValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string SduValueStr
        {
            get => sduValueStr;
            private set
            {
                sduValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            PlusSduValue = default;
            PlusSduValueStr = default;
            MinusSduValue = default;
            MinusSduValueStr = default;
            SduValue = default;
            SduValueStr = default;
        }
    }
}
