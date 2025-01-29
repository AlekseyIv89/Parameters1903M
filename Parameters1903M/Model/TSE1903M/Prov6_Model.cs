using Newtonsoft.Json;
using Parameters1903M.Util.Data;
using Parameters1903M.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov6_Model : BaseModel, IProvModel
    {
        // Стабильность положения ОС за 120 минут 
        private readonly Parameter osDriftFluctuationAndInstability;

        public OsDriftFluctuationInitialData InitialData { get; private set; }
        public OsDriftFluctuationCalculatedData CalculatedData { get; private set; }

        public Prov6_Model(Parameter osDriftFluctuation)
        {
            this.osDriftFluctuationAndInstability = osDriftFluctuation;
            ReadData();
        }

        public void ClearAllData()
        {
            InitialData.Clear();
            CalculatedData.Clear();
        }

        public void CalculateData()
        {
            // TODO

            WriteData();
        }

        private async void ReadData()
        {
            InitialData = new OsDriftFluctuationInitialData();
            CalculatedData = new OsDriftFluctuationCalculatedData();

            if (!string.IsNullOrWhiteSpace(osDriftFluctuationAndInstability.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{osDriftFluctuationAndInstability.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                OsDriftFluctuationInitialData initData = JsonConvert.DeserializeObject<OsDriftFluctuationInitialData>(fileData[0]);
                OsDriftFluctuationCalculatedData calcData = JsonConvert.DeserializeObject<OsDriftFluctuationCalculatedData>(fileData[1]);

                // TODO
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{osDriftFluctuationAndInstability.Num}", false);
        }
    }

    internal class OsDriftFluctuationInitialData : BaseModel, IProvData
    {
        // TODO

        public void Clear()
        {
            // TODO
        }
    }

    internal class OsDriftFluctuationCalculatedData : BaseModel, IProvData
    {
        // TODO

        public void Clear()
        {
            // TODO
        }
    }
}
