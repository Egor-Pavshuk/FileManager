using FileManager.Models.Enums;

namespace FileManager.Models.Dialogs
{
    public class DialogResult
    {
        public object Value { get; set; }

        public OperationResult OperationResult { get; set; }
    }
}