using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response
{
    public class Response
    {
        public string Message { get; set; }

        public int Success { get; set; }

        public object Data { get; set; }
    }
}
