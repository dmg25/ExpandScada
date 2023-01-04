using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpandScada.SignalsGateway
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
    }







    public class Signal<T> : Signal
    {
        Type signalType;
        private T _value;
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
                this._value = (T)value;
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




    public class SignalDouble : INotifyPropertyChanged
    {
        public int id;
        public string name;
        public string description;
        Type signalType;
        private double _value;
        public Type SignalType
        {
            get
            {
                return signalType;
            }
        }

        public double Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this.OnPropertyChanged();
            }
        }


        public SignalDouble()
        {
            signalType = typeof(double);
        }

        public SignalDouble(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            signalType = typeof(double);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
