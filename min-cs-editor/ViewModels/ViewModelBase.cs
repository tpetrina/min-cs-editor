using System.Runtime.CompilerServices;

namespace min_cs_editor.ViewModels
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }
    }
}
