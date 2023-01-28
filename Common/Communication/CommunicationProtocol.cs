using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Gateway;

namespace Common.Communication
{
    /*      HOW IT WORKS
    *  - There is Start method - does any connections and starts loops if the protocol supports it
    *  - Stop method - more like pause. You can start the communication again in any moment
    *  - Abort/Dispose/Or smth like that - destroy and disconnect everything. Dispose if necessary
    *  - Initialization
    *      - takes also settings string for this protocol - parsing/checking/applying
    *      - takes list/dictionary of Signals with property string for each one. 
    *      - every string must be parsed to settings for this Signal
    *          - in the protocol you can do any logic with these parameters - address map, in/out type etc
    *      - each protocol can have special signals (connected/disconnected/speed/errors...)
    *        we just have to have list of names of this signals - name will be just one of parameters
    *      - check on empty list or not enough signals or smth
    *  - create parameter class for every setting
    *      - there must be datatype, name, description, validation rule (method)
    *      - must be checking of unique parameters. 
    *          - if parameter has attribute like "must be unique", check every time all other signals.
    *        
    *  - method - get all setting parameters
    *  
    *  - table in DB
    *      - each protocol implementation mest have ID and ID of the protocol itself.
    *      - in the main table of signals add fields "protocol ID" "Settings string for protocol"
    *      
    * 
    * */
    public abstract class CommunicationProtocol
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string ChannelName { get; set; }


        public virtual string Name { get; set; }
        
        /// <summary>
        /// First initialization, parse settings and check them. Check equipment and possibility for starting
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="protocolSettings"></param>
        public virtual void InitializeProtocol(List<(Signal signal, string settings)> signals, string protocolSettings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start loop(s) of communication protocol
        /// </summary>
        public virtual void StartCommunication()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stop loops of communication with possibility to start them again if necessary
        /// </summary>
        public virtual void StopCommunication()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release all resources/equipment, finish all threads, dispose objects if necessary
        /// </summary>
        public virtual void FinishAndDisposeCommunication()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parameters for this protocol (speed, equipment names, addresses ... )
        /// </summary>
        /// <returns></returns>
        public virtual List<SettingsParameter> GetProtocolSettings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parameters for each signal which is involved in this protocol
        /// </summary>
        /// <returns></returns>
        public virtual List<SettingsParameter> GetSignalSettings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// List of special signals for this protocol, like connected/disconnected/error/status...
        /// </summary>
        /// <returns></returns>
        public virtual List<SettingsParameter> GetSystemSignals()
        {
            throw new NotImplementedException();
        }
    }
}
