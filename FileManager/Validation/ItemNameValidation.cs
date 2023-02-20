using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileManager.Validation
{
    public static class ItemNameValidation
    {
        private static readonly Regex template = new Regex(@"^[a-zA-Z0-9 ]+$");
        public static bool Validate(string name)
        {
            return template.IsMatch(name);
        }
    }
}
