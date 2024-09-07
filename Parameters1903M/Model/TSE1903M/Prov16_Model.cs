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
        private AngleSensorSlopeCalculatedData angleSensorSlopeCalculatedData;

        public List<SensitivityAxisNonperpendicularityInitialData> InitialData { get; set; }
        public SensitivityAxisNonperpendicularityCalculatedData CalculatedData { get; set; }

        public Prov16_Model(Parameter sensitivityAxisNonperpendicularity)
        {
            this.sensitivityAxisNonperpendicularity = sensitivityAxisNonperpendicularity;
            GetAngleSensorSlopeCalculatedData();
            ReadData();
        }

        private async void GetAngleSensorSlopeCalculatedData()
        {
            angleSensorSlopeCalculatedData = await new Prov2_Model(new MainWindowService().GetParameterByName("Крутизна характеристики ДУ, Sду")).GetAngleSensorSlopeCalculatedData();
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
            double uSum = 0.0;
            for (int i = 0; i < InitialData.Count; i++)
            {
                uSum += InitialData[i].UdyValue;
            }

            CalculatedData.UdyAverageValue = uSum / InitialData.Count;
            CalculatedData.DeltaMValue = CalculatedData.UdyAverageValue/ angleSensorSlopeCalculatedData.SduValue;

            sensitivityAxisNonperpendicularity.Value = CalculatedData.DeltaMValue;

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

            CalculatedData = new SensitivityAxisNonperpendicularityCalculatedData();

            if (!string.IsNullOrWhiteSpace(sensitivityAxisNonperpendicularity.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{sensitivityAxisNonperpendicularity.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<SensitivityAxisNonperpendicularityInitialData> listInitData = JsonConvert.DeserializeObject<List<SensitivityAxisNonperpendicularityInitialData>>(fileData[0]);
                SensitivityAxisNonperpendicularityCalculatedData listCalcData = JsonConvert.DeserializeObject<SensitivityAxisNonperpendicularityCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].UdyValue = listInitData[i].UdyValue;
                }

                CalculatedData.UdyAverageValue = listCalcData.UdyAverageValue;
                CalculatedData.DeltaMValue = listCalcData.DeltaMValue;
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
        private readonly int digits = 2;

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
