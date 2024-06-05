using Parameters1903M.Model;
using Parameters1903M.Model.TSE1903M;
using Parameters1903M.Service.Command;
using Parameters1903M.Service.TSE1903M;
using Parameters1903M.Util;
using Parameters1903M.Util.Exceptions;
using Parameters1903M.View.TSE1903M;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Parameters1903M.ViewModel.TSE1903M
{
    internal class InputDialogViewModel : BaseViewModel
    {
        public Parameter Parameter { get; }

        private readonly InputDialogWindowService inputDialogWindowService;

        public ICommand ButtonOkCommand { get; }
        public ICommand ButtonCancelCommand { get; }

        public InputDialogModel InputModel { get; set; }
        private InputDialogWindow InputWindow { get => inputDialogWindowService.GetProvWindow(); }

        public InputDialogViewModel(Parameter parameter)
        {
            Parameter = parameter;

            inputDialogWindowService = new InputDialogWindowService();

            InputModel = GetInputDialogModel(parameter);

            ButtonOkCommand = new RelayCommand(param => ParameterMeasure(), x => true);
            ButtonCancelCommand = new RelayCommand(param => inputDialogWindowService.Close(param), x => true);
        }

        public InputDialogViewModel(Parameter parameter, string title, string message)
        {
            Parameter = parameter;

            inputDialogWindowService = new InputDialogWindowService();

            InputModel = new InputDialogModel(title, message);

            ButtonOkCommand = new RelayCommand(param => ParameterInputValue(), x => true);
            ButtonCancelCommand = new RelayCommand(param => InputWindowClose(param), x => true);
        }

        private InputDialogModel GetInputDialogModel(Parameter parameter)
        {
            string title = parameter.Name;
            string infoMessage = $"Введите измеренную величину параметра \"{title}\"\nв ед. изм. \"{parameter.Unit}\"";
            return new InputDialogModel(title, infoMessage);
        }

        private void ParameterMeasure()
        {
            try
            {
                string inputValueStr = InputModel.InputValue.Replace(',', '.');
                double inputValue = double.Parse(inputValueStr, NumberStyles.Number, CultureInfo.InvariantCulture);
                Parameter.Value = inputValue;
                ButtonCancelCommand.Execute(true);
            }
            catch
            {
                string message = $"Неверно введено значение параметра \"{Parameter.Name}\"" + Environment.NewLine;
                message += "Повторите ввод";
                MessageBox.Show(InputWindow, message, Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ParameterInputValue()
        {
            string inputValueStr = InputModel.InputValue.Replace(',', '.');
            if (double.TryParse(inputValueStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double inputValue))
            {
                GlobalVars.InputDialogValue = inputValue;
                ButtonCancelCommand.Execute(true);
            }
            else
            {
                string message = $"Неверно введено измеренное значение." + Environment.NewLine;
                message += "Повторите ввод.";
                MessageBox.Show(InputWindow, message, Parameter.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InputWindowClose(object param)
        {
            inputDialogWindowService.Close(param);
            if (!(param is bool)) throw new ProvCancelledByUserException(Parameter);
        }
    }
}
