using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Common.Communication
{
    /// <summary>
    /// Represents one parameter for a settings string for each signal or protocol of communication
    /// </summary>
    public abstract class SettingsParameter : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual object Value
        {
            get;
            set;
        }

        public virtual Type SignalType
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public virtual object GetTypedValueByString(string val)
        {
            throw new NotImplementedException();
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

        public override Type SignalType
        {
            get
            {
                return typeof(T);
            }
        }

        
        public bool MustBeUnique { get; set; }
        public readonly Func<T, string> validation = null;
        public T DefaultValue { get; set; }

        public SettingsParameter() { }

        public SettingsParameter(
            T defaultValue,
            string name,
            string description,
            Func<T, string> validation = null,
            bool mustBeUnique = false)
        {
            DefaultValue = defaultValue;
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

        public override object GetTypedValueByString(string val)
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }

    }



}
