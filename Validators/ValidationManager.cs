using ComputerUtils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Validators
{
    public class ValidationManager
    {
        public static Dictionary<string, Func<string, ValidationResult>> validators { get; set; } = new Dictionary<string, Func<string, ValidationResult>>();

        public static bool AddValidator(string extensionIncludingDot, Func<string, ValidationResult> validationFunction, bool overrideExisting = true)
        {
            extensionIncludingDot = extensionIncludingDot.ToLower();
            if (!overrideExisting && validators.ContainsKey(extensionIncludingDot)) return false;
            validators[extensionIncludingDot] = validationFunction;
            return true;
        }

        public static ValidationResult ValidateFile(string filename, string filePath)
        {
            string extension = Path.GetExtension(filename).ToLower();
            if (!validators.ContainsKey(extension)) return new ValidationResult(true, "No validation possible. Assuming it's correct");
            return validators[extension].Invoke(filePath);
        }

        public static void AddDefaultValidators()
        {
            AddValidator(".qmod", new Func<string, ValidationResult>(x =>
            {
                return QModValidator.ValidateQMod(x);
            }));
        }
    }
}
