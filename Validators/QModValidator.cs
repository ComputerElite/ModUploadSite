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
        public static ValidationResult ValidateQMod(string fileName)
        {
            try
            {
                QMod mod = QMod.ParseAsync(File.Open(fileName, FileMode.Open)).Result;
            } catch(Exception e)
            {
                return new ValidationResult(false, e.Message);
            }
            return new ValidationResult(true, "Qmod parsed successfully");
        }
    }
}
