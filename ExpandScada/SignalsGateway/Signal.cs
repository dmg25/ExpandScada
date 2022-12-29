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

        private T _value;
        public override Type SignalType
        {
            get
            {
                return typeof(T);
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

        }

        public Signal(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }

        
    }
}
