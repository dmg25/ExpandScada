using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScada.Commands
{
    public abstract class CommandProperty
    {
        public string Name { get; set; }

        public virtual object ValueObj
        {
            get;
            set;
        }

        public virtual Type PropertyType
        {
            get;
        }
    }

    public class CommandProperty<T> : CommandProperty
    {
        private T _value;
        public override Type PropertyType
        {
            get
            {
                return typeof(T);
            }
        }

        public T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                _value = value;
            }
        }

        public override object ValueObj
        {
            get
            {
                return this._value;
            }
            set
            {
                _value = (T)value;
            }
        }

        public CommandProperty(string name)
        {
            Name = name;
        }
    }
}
