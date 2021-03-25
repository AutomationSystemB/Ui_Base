using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using HamburgerMenu.Views;

namespace HamburgerMenuApp
{
    public class _cDigitalIO: INotifyPropertyChanged
    {
        public short    index    { get; set; }
        private bool    state; 
        public string   name  { get; set; }

        public bool State  {
                            get { return this.state; }
                            set
                            {
                                if (this.state != value)
                                {
                                    this.state = value;
                                    this.NotifyPropertyChanged("State");
                                }
}
                            }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string  valor)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(valor));
        }

    }

}
