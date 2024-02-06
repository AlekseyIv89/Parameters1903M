namespace Parameters1903M.Model.TSE1903M
{
    public class InputDialogModel : BaseModel
    {
        public string Title { get; private set; }
        public string InfoMessage { get; private set; }

        public string InputValue { get; set; }

        public InputDialogModel(string title, string infoMessage)
        {
            Title = title;
            InfoMessage = infoMessage;
        }
    }
}
