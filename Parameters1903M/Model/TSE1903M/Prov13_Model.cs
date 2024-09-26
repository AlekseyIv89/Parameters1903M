using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov13_Model : BaseModel, IProvModel
    {
        // Время установления выходной информации
        private readonly Parameter outputInformationEstablishmentTime;

        public List<OutputInformationEstablishmentTimeInitialData> InitialData { get; set; }
        public OutputInformationEstablishmentTimeCalculatedData CalculatedData { get; set; }

        public Prov13_Model(Parameter outputInformationEstablishmentTime)
        {
            this.outputInformationEstablishmentTime = outputInformationEstablishmentTime;
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
            double maxValue = InitialData[0].Position0Value >= InitialData[0].Position180Value
                ? InitialData[0].Position0Value
                : InitialData[0].Position180Value;

            for (int i = 1; i < InitialData.Count; i++)
            {
                if (InitialData[i].Position0Value > maxValue)
                    maxValue = InitialData[i].Position0Value;

                if (InitialData[i].Position180Value > maxValue)
                    maxValue = InitialData[i].Position180Value;
            }

            CalculatedData.MaxPositionValue = maxValue;
            outputInformationEstablishmentTime.Value = CalculatedData.MaxPositionValue;

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new List<OutputInformationEstablishmentTimeInitialData>
            {
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData(),
                new OutputInformationEstablishmentTimeInitialData()
            };

            CalculatedData = new OutputInformationEstablishmentTimeCalculatedData();

            if (!string.IsNullOrWhiteSpace(outputInformationEstablishmentTime.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{outputInformationEstablishmentTime.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<OutputInformationEstablishmentTimeInitialData> listInitData = JsonConvert.DeserializeObject<List<OutputInformationEstablishmentTimeInitialData>>(fileData[0]);
                OutputInformationEstablishmentTimeCalculatedData calcData = JsonConvert.DeserializeObject<OutputInformationEstablishmentTimeCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].Position0Value = listInitData[i].Position0Value;
                    InitialData[i].Position180Value = listInitData[i].Position180Value;
                }

                CalculatedData.MaxPositionValue = calcData.MaxPositionValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{outputInformationEstablishmentTime.Num}", false);
        }
    }

    internal class OutputInformationEstablishmentTimeInitialData : BaseModel, IProvData
    {
        private readonly int digits = 0;

        private double position0Value;
        private string position0ValueStr;

        private double position180Value;
        private string position180ValueStr;

        public double Position0Value
        {
            get => position0Value;
            set
            {
                position0Value = value;
                Position0ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Position0ValueStr
        {
            get => position0ValueStr;
            set
            {
                position0ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Position180Value
        {
            get => position180Value;
            set
            {
                position180Value = value;
                Position180ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Position180ValueStr
        {
            get => position180ValueStr;
            set
            {
                position180ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            Position0Value = default;
            Position0ValueStr = default;
            Position180Value = default;
            Position180ValueStr = default;
        }
    }

    internal class OutputInformationEstablishmentTimeCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 0;

        private double maxPositionValue;
        private string maxPositionValueStr;

        public double MaxPositionValue
        {
            get => maxPositionValue;
            set
            {
                maxPositionValue = value;
                MaxPositionValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MaxPositionValueStr
        {
            get => maxPositionValueStr;
            set
            {
                maxPositionValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            MaxPositionValue = default;
            MaxPositionValueStr = default;
        }
    }
}
