using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;

namespace ExpandScada.Communication
{
    /// <summary>
    /// Main class for managing of all communication protocols
    /// </summary>
    public class CommunicationManager
    {
        public static Dictionary<int, CommunicationProtocol> communicationProtocols = new Dictionary<int, CommunicationProtocol>();

        public static void RunAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.Value.StartCommunication();
            }
        }

        public static void StopAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.Value.StopCommunication();
            }
        }

        public static void FinishAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.Value.FinishAndDisposeCommunication();
            }
        }
    }
}
