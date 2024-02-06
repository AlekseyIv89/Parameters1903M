using Newtonsoft.Json;
using Parameters1903M.Service;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov16_Model : BaseModel, IProvModel
    {
        // Неперпендикулярность ОЧ опорной плоскости
        private readonly Parameter sensitivityAxisNonperpendicularity;

        // Крутизна характеристики ДУ
        private readonly Parameter angleSensorSlope;

        public List<SensitivityAxisNonperpendicularityInitialData> InitialData { get; set; }
        public List<SensitivityAxisNonperpendicularityCalculatedData> CalculatedData { get; set; }

        public Prov16_Model(Parameter sensitivityAxisNonperpendicularity)
        {
            this.sensitivityAxisNonperpendicularity = sensitivityAxisNonperpendicularity;
            angleSensorSlope = new MainWindowService().GetParameterByName("Крутизна характеристики ДУ, Sду");
            ReadData();
        }

        public void ClearAllData()
        {
            for (int i = 0; i < InitialData.Count; i++)
            {
                InitialData[i].Clear();
            }

            CalculatedData[0].Clear();
        }

        public void CalculateData()
        {
            double uSum = 0.0;
            for (int i = 1; i < InitialData.Count; i++)
            {
                uSum += InitialData[i].UdyValue;
            }

            CalculatedData[0].UdyAverageValue = uSum / 4;
            CalculatedData[0].DeltaMValue = CalculatedData[0].UdyAverageValue * 60 / angleSensorSlope.Value;

            sensitivityAxisNonperpendicularity.Value = CalculatedData[0].DeltaMValue;

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new List<SensitivityAxisNonperpendicularityInitialData>
            {
                new SensitivityAxisNonperpendicularityInitialData(),
                new SensitivityAxisNonperpendicularityInitialData(),
                new SensitivityAxisNonperpendicularityInitialData(),
                new SensitivityAxisNonperpendicularityInitialData(),
                new SensitivityAxisNonperpendicularityInitialData()
            };

            CalculatedData = new List<SensitivityAxisNonperpendicularityCalculatedData>
            {
                new SensitivityAxisNonperpendicularityCalculatedData()
            };

            if (!string.IsNullOrWhiteSpace(sensitivityAxisNonperpendicularity.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{sensitivityAxisNonperpendicularity.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<SensitivityAxisNonperpendicularityInitialData> listInitData = JsonConvert.DeserializeObject<List<SensitivityAxisNonperpendicularityInitialData>>(fileData[0]);
                List<SensitivityAxisNonperpendicularityCalculatedData> listCalcData = JsonConvert.DeserializeObject<List<SensitivityAxisNonperpendicularityCalculatedData>>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].UdyValue = listInitData[i].UdyValue;
                }

                CalculatedData[0].UdyAverageValue = listCalcData[0].UdyAverageValue;
                CalculatedData[0].DeltaMValue = listCalcData[0].DeltaMValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{sensitivityAxisNonperpendicularity.Num}", false);
        }
    }

    internal class SensitivityAxisNonperpendicularityInitialData : BaseModel, IProvData
    {
        private readonly int digits = 1;

        private double udyValue;
        private string udyValueStr;

        public double UdyValue
        {
            get => udyValue;
            set
            {
                udyValue = value;
                UdyValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string UdyValueStr
        {
            get => udyValueStr;
            private set
            {
                udyValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            UdyValue = default;
            UdyValueStr = default;
        }
    }

    internal class SensitivityAxisNonperpendicularityCalculatedData : BaseModel, IProvData
    {
        private double udyAverageValue;
        private string udyAverageValueStr;

        private double deltaMValue;
        private string deltaMValueStr;

        public double UdyAverageValue
        {
            get => udyAverageValue;
            set
            {
                int digits = 2;
                udyAverageValue = value;
                UdyAverageValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string UdyAverageValueStr
        {
            get => udyAverageValueStr;
            private set
            {
                udyAverageValueStr = value;
                OnPropertyChanged();
            }
        }

        public double DeltaMValue
        {
            get => deltaMValue;
            set
            {
                int digits = 0;
                deltaMValue = value;
                DeltaMValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string DeltaMValueStr
        {
            get => deltaMValueStr;
            private set
            {
                deltaMValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            UdyAverageValue = default;
            UdyAverageValueStr = default;
            DeltaMValue = default;
            DeltaMValueStr = default;
        }
    }
}
