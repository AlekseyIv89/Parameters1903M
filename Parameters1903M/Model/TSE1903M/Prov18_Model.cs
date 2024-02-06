using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov18_Model : BaseModel, IProvModel
    {
        // Время схода ПС с упора
        private readonly Parameter releaseTime;

        public List<ReleaseTimeInitialData> InitialData { get; set; }
        public ReleaseTimeCalculatedData CalculatedData { get; set; }

        public Prov18_Model(Parameter releaseTime)
        {
            this.releaseTime = releaseTime;
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
            double maxReleaseTimeValue = InitialData[0].InclinePlusValue;
            if (InitialData[0].InclineMinusValue > maxReleaseTimeValue)
                maxReleaseTimeValue = InitialData[0].InclineMinusValue;

            for (int i = 1; i < InitialData.Count; i++)
            {
                if (InitialData[i].InclinePlusValue > maxReleaseTimeValue)
                    maxReleaseTimeValue = InitialData[i].InclinePlusValue;

                if (InitialData[i].InclineMinusValue > maxReleaseTimeValue)
                    maxReleaseTimeValue = InitialData[i].InclineMinusValue;
            }

            CalculatedData.ReleaseTimeValue = maxReleaseTimeValue;

            releaseTime.Value = CalculatedData.ReleaseTimeValue;

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new List<ReleaseTimeInitialData>
            {
                new ReleaseTimeInitialData(),
                new ReleaseTimeInitialData(),
                new ReleaseTimeInitialData()
            };

            CalculatedData = new ReleaseTimeCalculatedData();

            if (!string.IsNullOrWhiteSpace(releaseTime.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{releaseTime.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<ReleaseTimeInitialData> listInitData = JsonConvert.DeserializeObject<List<ReleaseTimeInitialData>>(fileData[0]);
                ReleaseTimeCalculatedData calcData = JsonConvert.DeserializeObject<ReleaseTimeCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].InclinePlusValue = listInitData[i].InclinePlusValue;
                    InitialData[i].InclineMinusValue = listInitData[i].InclineMinusValue;
                }

                CalculatedData.ReleaseTimeValue = calcData.ReleaseTimeValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{releaseTime.Num}", false);
        }
    }

    internal class ReleaseTimeInitialData : BaseModel, IProvData
    {
        private readonly int digits = 0;

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

    internal class ReleaseTimeCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 0;

        private double releaseTimeValue;
        private string releaseTimeValueStr;

        public double ReleaseTimeValue
        {
            get => releaseTimeValue;
            set
            {
                releaseTimeValue = value;
                ReleaseTimeValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ReleaseTimeValueStr
        {
            get => releaseTimeValueStr;
            set
            {
                releaseTimeValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            ReleaseTimeValue = default;
            ReleaseTimeValueStr = default;
        }
    }
}
