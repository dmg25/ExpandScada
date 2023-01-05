using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpandScada.Communication
{
    /// <summary>
    /// Represents one parameter for a settings string for each signal or protocol of communication
    /// </summary>
    public abstract class SettingsParameter : INotifyPropertyChanged
    {
        public virtual object Value
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


    }

    public class SettingsParameter<T> : SettingsParameter, IDataErrorInfo
    {
        private T _value;
        public override object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = (T)value;
                this.OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool MustBeUnique { get; set; }

        public readonly Func<T, string> validation = null;


        public SettingsParameter() { }

        public SettingsParameter(
            T value,
            string name,
            string description,
            Func<T, string> validation = null,
            bool mustBeUnique = false)
        {
            Value = value;
            Name = name;
            Description = description;
            this.validation = validation;
            MustBeUnique = mustBeUnique;
        }

        // TODO Maybe here will be the problem, check carefully during configurator implementation
        public string this[string columnName]
        {
            get
            {
                return validation(_value);
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }



    }



}
