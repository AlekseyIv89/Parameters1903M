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
            double maxValue = InitialData[0].Postion0Value >= InitialData[0].Postion180Value
                ? InitialData[0].Postion0Value
                : InitialData[0].Postion180Value;

            for (int i = 1; i < InitialData.Count; i++)
            {
                if (InitialData[i].Postion0Value > maxValue)
                    maxValue = InitialData[i].Postion0Value;

                if (InitialData[i].Postion180Value > maxValue)
                    maxValue = InitialData[i].Postion180Value;
            }

            CalculatedData.MaxPostionValue = maxValue;
            outputInformationEstablishmentTime.Value = CalculatedData.MaxPostionValue;

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
                    InitialData[i].Postion0Value = listInitData[i].Postion0Value;
                    InitialData[i].Postion180Value = listInitData[i].Postion180Value;
                }

                CalculatedData.MaxPostionValue = calcData.MaxPostionValue;
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

        private double postion0Value;
        private string postion0ValueStr;

        private double postion180Value;
        private string postion180ValueStr;

        public double Postion0Value
        {
            get => postion0Value;
            set
            {
                postion0Value = value;
                Postion0ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Postion0ValueStr
        {
            get => postion0ValueStr;
            set
            {
                postion0ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Postion180Value
        {
            get => postion180Value;
            set
            {
                postion180Value = value;
                Postion180ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Postion180ValueStr
        {
            get => postion180ValueStr;
            set
            {
                postion180ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }

    internal class OutputInformationEstablishmentTimeCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 0;

        private double maxPostionValue;
        private string maxPostionValueStr;

        public double MaxPostionValue
        {
            get => maxPostionValue;
            set
            {
                maxPostionValue = value;
                MaxPostionValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string MaxPostionValueStr
        {
            get => maxPostionValueStr;
            set
            {
                maxPostionValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            MaxPostionValue = default;
            MaxPostionValueStr = default;
        }
    }
}
