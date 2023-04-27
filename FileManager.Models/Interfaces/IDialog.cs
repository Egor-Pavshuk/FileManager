using FileManager.Models.Dialogs;
using System.Threading.Tasks;

namespace FileManager.Models.Interfaces
{
    public interface IDialog<T> : IDialog
    {
        Task<DialogResult> ShowAsync(T parameter);
    }

    public interface IDialog
    {
        void Dismiss();

        void OnEnterKeyUp();
    }
}