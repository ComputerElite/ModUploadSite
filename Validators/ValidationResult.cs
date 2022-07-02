using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Validators
{
    public class ValidationResult
    {
        public ValidationResult(bool valid, string msg)
        {
            this.valid = valid;
            this.msg = msg;
        }

        public bool valid { get; set; } = false;
        public string msg { get; set; } = "";
    }
}
