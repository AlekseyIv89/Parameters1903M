namespace Parameters1903M.Model
{
    internal class TimerModel : BaseModel
    {
        private string time;

        /// <summary>
        /// Время обратного отсчета для отображения в компоненте формы
        /// </summary>
        public string Time
        {
            get => time;
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }
    }

    public enum TimerEvent
    {
        Finished,
        Cancelled,
        UserChangedSystemTime
    }
}
