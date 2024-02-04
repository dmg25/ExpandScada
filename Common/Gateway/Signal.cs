using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Common.Communication;

namespace Common.Gateway
{
    /*      Plan
     *  - create abstract class for Signal. What must be there? 
     *      - set/get objects OR
     *      - try to implement direct Value property
     *  - create class Signal based on abstract class.
     *      - Do it generic
     *      - convertor with packing/unpacking
     *      - add notification also
     *  - Create concurrent dictionary for abstract signals 
     *  - think out some test scenarios for checking how is it working
     *      - smth like: add some signals, some threads and use it with some algorithm. Check sequency and time of working
     *      - try to provocate referenced types multi accessing problem
     * 
     * 
     * 
     * 
     * */


    public abstract class Signal : INotifyPropertyChanged
    {
        public int id;
        public string name;
        public string description;
        /// <summary>
        /// if this field is true - every new value will be checked on equality and if this value is not equal, will work event PropertyChangedNotEqual
        /// </summary>
        public bool checkEquality = false;

        public virtual object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Use it to write values to communication channel. Because Value can be updated in the middle, we can not use just Value
        /// </summary>
        public virtual object ValueToWrite
        {
            get;
            set;
        }

        public virtual Type SignalType
        {
            get;
        }

        public CommunicationProtocol AttachedCommunicationProtocol { get; set; } = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChangedNotEqual;
        /// <summary>
        /// Will be invoked only if the new value is not equal.
        /// </summary>
        /// <param name="prop"></param>
        public void OnPropertyChangedNotEqual([CallerMemberName] string prop = "")
        {
            PropertyChangedNotEqual?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class Signal<T> : Signal
    {
        Type signalType;
        private T _value;
        private T _valueToWrite;
        public override Type SignalType
        {
            get
            {
                return signalType;
            }
        }

        public override object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (checkEquality && !EqualityComparer<T>.Default.Equals(_value, (T)value))
                {
                    OnPropertyChangedNotEqual();
                }

                TypedValue = (T)value; // TESTS ONLY
                this.OnPropertyChanged();
            }
        }

        public override object ValueToWrite
        {
            get
            {
                return this._valueToWrite;
            }
            set
            {
                _valueToWrite = (T)value;
                this.OnPropertyChanged();
            }
        }

        // Test for a while 
        public T TypedValue
        {
            get
            {
                return this._value;
            }
            set
            {
                if (checkEquality && !EqualityComparer<T>.Default.Equals(_value, value))
                {
                    OnPropertyChangedNotEqual();
                }

                this._value = value;
                this.OnPropertyChanged();
            }
        }


        public Signal()
        {
            signalType = typeof(T);
        }

        public Signal(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            signalType = typeof(T);
        }


    }

}

