using Newtonsoft.Json;
using Parameters1903M.Service;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov3_Model : BaseModel, IProvModel
    {
        // Дрейф нуля ДУ
        private readonly Parameter angleSensorZeroDrift;

        // Крутизна характеристики ДУ
        private AngleSensorSlopeCalculatedData angleSensorSlopeCalculatedData;

        public AngleSensorZeroDriftInitialData InitialData { get; private set; }
        public AngleSensorZeroDriftCalculatedData CalculatedData { get; private set; }

        private List<double> tempData;

        public Prov3_Model(Parameter angleSensorZeroDrift)
        {
            this.angleSensorZeroDrift = angleSensorZeroDrift;
            GetAngleSensorSlopeCalculatedData();
            ReadData();
        }

        private async void GetAngleSensorSlopeCalculatedData()
        {
            angleSensorSlopeCalculatedData = await new Prov2_Model(new MainWindowService().GetParameterByName("Крутизна характеристики ДУ, Sду")).GetAngleSensorSlopeCalculatedData();
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
                InitialData.UdyMaxValue = tempData.Max();
                InitialData.UdyMinValue = tempData.Min();
                CalculatedData.DeltaUdyValue = InitialData.UdyMaxValue - InitialData.UdyMinValue;

                CalculatedData.AngleSensorZeroDriftValue = 60 * (InitialData.UdyMaxValue - InitialData.UdyMinValue) / 
                    angleSensorSlopeCalculatedData.SduValue;
            }
        }

        public void CalculateData()
        {
            angleSensorZeroDrift.Value = CalculatedData.AngleSensorZeroDriftValue;

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new AngleSensorZeroDriftInitialData();
            CalculatedData = new AngleSensorZeroDriftCalculatedData();
            tempData = new List<double>();

            if (!string.IsNullOrWhiteSpace(angleSensorZeroDrift.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{angleSensorZeroDrift.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                AngleSensorZeroDriftInitialData initData = JsonConvert.DeserializeObject<AngleSensorZeroDriftInitialData>(fileData[0]);
                AngleSensorZeroDriftCalculatedData calcData = JsonConvert.DeserializeObject<AngleSensorZeroDriftCalculatedData>(fileData[1]);

                InitialData.UdyValue = initData.UdyValue;
                InitialData.UdyMaxValue = initData.UdyMaxValue;
                InitialData.UdyMinValue = initData.UdyMinValue;

                CalculatedData.DeltaUdyValue = calcData.DeltaUdyValue;
                CalculatedData.AngleSensorZeroDriftValue = calcData.AngleSensorZeroDriftValue;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{angleSensorZeroDrift.Num}", false);
        }
    }

    internal class AngleSensorZeroDriftInitialData : BaseModel, IProvData
    {
        private readonly int digits = 2;

        private double udyValue;
        private string udyValueStr;

        private double udyMaxValue;
        private string udyMaxValueStr;

        private double udyMinValue;
        private string udyMinValueStr;

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

        public double UdyMaxValue
        {
            get => udyMaxValue;
            set
            {
                udyMaxValue = value;
                UdyMaxValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string UdyMaxValueStr
        {
            get => udyMaxValueStr;
            private set
            {
                udyMaxValueStr = value;
                OnPropertyChanged();
            }
        }

        public double UdyMinValue
        {
            get => udyMinValue;
            set
            {
                udyMinValue = value;
                UdyMinValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string UdyMinValueStr
        {
            get => udyMinValueStr;
            private set
            {
                udyMinValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            UdyValue = default;
            UdyValueStr = default;
            UdyMaxValue = default;
            UdyMaxValueStr = default;
            UdyMinValue = default;
            UdyMinValueStr = default;
        }
    }

    internal class AngleSensorZeroDriftCalculatedData : BaseModel, IProvData
    {
        private readonly int digits = 1;

        private double deltaUdyValue;
        private string deltaUdyValueStr;

        public double DeltaUdyValue
        {
            get => deltaUdyValue;
            set
            {
                deltaUdyValue = value;
                DeltaUdyValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string DeltaUdyValueStr
        {
            get => deltaUdyValueStr;
            private set
            {
                deltaUdyValueStr = value;
                OnPropertyChanged();
            }
        }

        private double angleSensorZeroDriftValue;
        private string angleSensorZeroDriftValueStr;

        public double AngleSensorZeroDriftValue
        {
            get => angleSensorZeroDriftValue;
            set
            {
                angleSensorZeroDriftValue = value;
                AngleSensorZeroDriftValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string AngleSensorZeroDriftValueStr
        {
            get => angleSensorZeroDriftValueStr;
            private set
            {
                angleSensorZeroDriftValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            DeltaUdyValue = default;
            DeltaUdyValueStr = default;
            AngleSensorZeroDriftValue = default;
            AngleSensorZeroDriftValueStr = default;
        }
    }
}
