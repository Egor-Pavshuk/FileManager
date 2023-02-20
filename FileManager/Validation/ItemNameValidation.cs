using System.Text.RegularExpressions;

namespace FileManager.Validation
{
    public static class ItemNameValidation
    {
        private static readonly Regex template = new Regex(@"^[a-zA-Z0-9 _.]+$");
        public static bool Validate(string name)
        {
            return template.IsMatch(name);
        }
    }
}
