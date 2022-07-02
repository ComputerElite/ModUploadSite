using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite
{
    public class GenericRequestResponse
    {
        public GenericRequestResponse(int statusCode, string msg, string contentType = "text/plain")
        {
            this.statusCode = statusCode;
            this.message = msg;
            this.contentType = contentType;
        }
        public int statusCode = 200;
        public string message = "";
        public string contentType = "text/plain";
    }
}
