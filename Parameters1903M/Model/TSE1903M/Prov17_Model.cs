using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov17_Model : BaseModel, IProvModel
    {
        // Ток отрыва ПС от упора
        private readonly Parameter liftOffCurrent;

        public List<LiftOffCurrentInitialData> InitialData { get; set; }
        public LiftOffCurrentCalculatedData CalculatedData { get; set; }

        public Prov17_Model(Parameter liftOffCurrent)
        {
            this.liftOffCurrent = liftOffCurrent;
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
            double minPlusValue = InitialData[0].InclinePlusValue;
            double minMinusValue = InitialData[0].InclineMinusValue;

            for (int i = 1; i < InitialData.Count; i++)
            {
                if (InitialData[i].InclinePlusValue < minPlusValue)
                    minPlusValue = InitialData[i].InclinePlusValue;

                if (InitialData[i].InclineMinusValue < minMinusValue)
                    minMinusValue = InitialData[i].InclineMinusValue;
            }

            CalculatedData.MinInclinePlusValue = minPlusValue;
            CalculatedData.MinInclineMinusValue = minMinusValue;

            liftOffCurrent.StrValue = $"{CalculatedData.MinInclinePlusValueStr};{CalculatedData.MinInclineMinusValueStr}";

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new List<LiftOffCurrentInitialData>
            {
                new LiftOffCurrentInitialData(),
                new LiftOffCurrentInitialData(),
                new LiftOffCurrentInitialData()
            };

            CalculatedData = new LiftOffCurrentCalculatedData();

            if (!string.IsNullOrWhiteSpace(liftOffCurrent.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{liftOffCurrent.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<LiftOffCurrentInitialData> listInitData = JsonConvert.DeserializeObject<List<LiftOffCurrentInitialData>>(fileData[0]);
                LiftOffCurrentCalculatedData calcData = JsonConvert.DeserializeObject<LiftOffCurrentCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].InclinePlusValue = listInitData[i].InclinePlusValue;
                    InitialData[i].InclineMinusValue = listInitData[i].InclineMinusValue;
                }

                CalculatedData.MinInclinePlusValue = calcData.MinInclinePlusValue;
                CalculatedData.MinInclineMinusValue = calcData.MinInclineMinusValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{liftOffCurrent.Num}", false);
        }
    }

    internal class LiftOffCurrentInitialData : BaseModel, IProvData
    {
        private readonly int digits = 1;

        private double inclinePlusValue;
        private string inclinePlusValueStr;

        private double inclineMinusValue;
        private string inclineMinusValueStr;

        public double InclinePlusValue
        {
            get => inclinePlusValue;
            set
            {
                inclinePlusValue = value;
                InclinePlusValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string InclinePlusValueStr
        {
            get => inclinePlusValueStr;
            set
            {
                inclinePlusValueStr = value;
                OnPropertyChanged();
            }
        }

        public double InclineMinusValue
        {
            get => inclineMinusValue;
            set
            {
                inclineMinusValue = value;
                InclineMinusValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string InclineMinusValueStr
        {
            get => inclineMinusValueStr;
            set
            {
                inclineMinusValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            InclinePlusValue = default;
            InclinePlusValueStr = default;
            InclineMinusValue = default;
            InclineMinusValueStr = default;
        }
    }

    internal class LiftOffCurrentCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 1;

        private double minInclinePlusValue;
        private string minInclinePlusValueStr;

        private double minInclineMinusValue;
        private string minInclineMinusValueStr;

        public double MinInclinePlusValue
        {
            get => minInclinePlusValue;
            set
            {
                minInclinePlusValue = value;
                MinInclinePlusValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MinInclinePlusValueStr
        {
            get => minInclinePlusValueStr;
            set
            {
                minInclinePlusValueStr = value;
                OnPropertyChanged();
            }
        }

        public double MinInclineMinusValue
        {
            get => minInclineMinusValue;
            set
            {
                minInclineMinusValue = value;
                MinInclineMinusValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MinInclineMinusValueStr
        {
            get => minInclineMinusValueStr;
            set
            {
                minInclineMinusValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            MinInclinePlusValue = default;
            MinInclinePlusValueStr = default;
            MinInclineMinusValue = default;
            MinInclineMinusValueStr = default;
        }
    }
}
