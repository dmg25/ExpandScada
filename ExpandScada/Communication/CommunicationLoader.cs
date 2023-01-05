using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScada.Communication
{
    /*      How it works:
     *  - we are searching for dlls in the some folder. 
     *  - each found dll trying convert to abstract protocol class 
     *  - if there is some dll, but converts with error - write to log or smth
     *  - make some name for this protocol (like ID converted?)
     *  - add this protocol to the list of the protocols
     * */

    public class CommunicationLoader
    {
        public static void LoadAllProtocols()
        {

        }

    }
}
