using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

using Virtual.Interface.Golf.Models;

namespace Virtual.Interface.Golf.ViewModels
{

    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {

        #region Private Members
        
        #endregion

        #region Properties

        #endregion

        #region Private Methods

        

        #endregion

        #region Protected Methods
                      
        #endregion

        #region Public Methods

        #endregion

        #region INotifyPropertyChanges

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDisposable Members

        //Implement IDisposable.
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
