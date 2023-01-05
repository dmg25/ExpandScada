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
        public static List<CommunicationProtocol> communicationProtocols = new List<CommunicationProtocol>();

        public static void RunAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.StartCommunication();
            }
        }

        public static void StopAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.StopCommunication();
            }
        }

        public static void FinishAllCommunication()
        {
            foreach (var protocol in communicationProtocols)
            {
                protocol.FinishAndDisposeCommunication();
            }
        }
    }
}
