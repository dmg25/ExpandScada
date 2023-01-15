using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.SignalsGateway;
using Common.Gateway;
using ExpandScada.Communication;


namespace ExpandScada.Test.Communication
{

    public class TmpCommunication
    {


        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TmpCommunicationTest()
        {
            // TODO #########################################################################################
            // - Cut dependency to nuget from this project, try make it like this
            // - maybe make protocol more ready, because did not find there using of signals dictionary...
            // - set modbus slave on the localhost and make it working
            // ##############################################################################################


            SignalLoader.LoadAllSignals("..\\Project\\test1.db");
            CommunicationLoader.LoadAllProtocols("..\\Protocols\\Debug", "..\\Project\\test1.db");
            var name = CommunicationManager.communicationProtocols[1].Name;
            var modbusTcp = CommunicationManager.communicationProtocols[1];

            modbusTcp.StartCommunication();

            Thread.Sleep(5000);

            var value = SignalStorage.allSignals[1].Value;

            Assert.Pass();

        }


    }
}



