namespace Parameters1903M.Model
{
    public class TableDeviceData : BaseModel
    {
        private string name;
        private string deviceData;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string DeviceData
        {
            get => deviceData;
            set
            {
                deviceData = value;
                OnPropertyChanged();
            }
        }
    }
}
