using System;
using System.Collections.Generic;
using System.Text;

namespace ImageUpWpf.Core.Upload
{
    public class UploadException : Exception
    {
        public new string Message { get; private set; }
        public UploadException(string message)
        {
            this.Message = message;
        }
    }
}
