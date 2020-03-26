using System;

namespace RequestService
{
    internal class CustomException : Exception
    {
        public string Data { get; set; }
    }
}
