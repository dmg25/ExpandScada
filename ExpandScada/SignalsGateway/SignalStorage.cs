using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ExpandScada.SignalsGateway
{
    public class SignalStorage
    {
        public static ConcurrentDictionary<int, Signal> allSignals = new ConcurrentDictionary<int, Signal>();
    }
}
