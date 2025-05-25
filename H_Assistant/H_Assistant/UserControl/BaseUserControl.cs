using H_Assistant.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H_Assistant.UserControl
{
    public abstract class BaseUserControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
