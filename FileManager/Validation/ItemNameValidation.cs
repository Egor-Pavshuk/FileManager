using System.Text.RegularExpressions;

namespace FileManager.Validation
{
    public static class ItemNameValidation
    {
        private static readonly Regex template = new Regex(@"^[a-zA-Z0-9а-яА-Я _.їЇ']+$");
        public static bool Validate(string name)
        {
             return name != null && template.IsMatch(name);
        }
    }
}
