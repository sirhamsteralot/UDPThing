using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.EndPoint.Replication
{
    internal class Replicable<T> : INotifyPropertyChanged
        where T : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Replicable(T observable)
        {
            observable.PropertyChanged += Observable_PropertyChanged;
        }

        private void Observable_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
    }
}
