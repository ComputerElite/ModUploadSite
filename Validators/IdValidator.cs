using ComputerUtils.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Validators
{
    public class IdValidator
    {
        public static string idValidationChars = "1234567890abcdefABCDEF";
        public static string idChars = "1234567890abcdef";
        public static bool IsIdValid(string id)
        {
            foreach(char c in id)
            {
                if (!idValidationChars.Contains(c)) return false;
            }
            return true;
        }

        public static string GetNewId()
        {
            return Hasher.GetSHA256OfString(DateTime.UtcNow.Ticks.ToString());
        }
    }
}
