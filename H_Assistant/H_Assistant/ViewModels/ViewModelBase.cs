using H_Assistant.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H_Assistant.ViewModels
{
    class ViewModelBase : INotifyPropertyChanged
    {

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
