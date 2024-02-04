using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScada.Commands
{
    public static class ButtonActions
    {
        public static Dictionary<string, ButtonAction> Actions = new Dictionary<string, ButtonAction>()
        {
            {"WriteSignalToDevice", new WriteSignalToDevice("WriteSignalToDevice")},
        };
    }
}
