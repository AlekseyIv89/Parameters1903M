using Parameters1903M.Model;
using System;

namespace Parameters1903M.Util.Exceptions
{
    internal class ProvCancelledByUserException : Exception
    {
        public Parameter Parameter { get; }

        public ProvCancelledByUserException(Parameter parameter)
        {
            Parameter = parameter;
        }
    }
}
