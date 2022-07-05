using QuestPatcher.QMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Validators
{
    public class QModValidator
    {
        public static ValidationResult ValidateQMod(string filePath)
        {
            try
            {
                Stream s = File.Open(filePath, FileMode.Open);
                QMod qmod = QMod.ParseAsync(s).Result;
                s.Close();
            } catch(Exception e)
            {
                return new ValidationResult(false, e.Message);
            }
            return new ValidationResult(true, "Qmod parsed successfully");
        }
    }
}
