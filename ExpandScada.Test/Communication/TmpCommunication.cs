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

        // !!!Start Modbus Slave on licalhost before!!!
        [Test]
        public void TmpCommunicationTest()
        {
            SignalLoader.LoadAllSignals("..\\Project\\test1.db");
            CommunicationLoader.LoadAllProtocols("..\\Protocols\\Debug", "..\\Project\\test1.db");
            var name = CommunicationManager.communicationProtocols[1].Name;
            var modbusTcp = CommunicationManager.communicationProtocols[1];


            //modbusTcp.Logger.Info("tetetetst");
            //modbusTcp.Logger.Trace("tetetetst");
            //modbusTcp.Logger.Debug("tetetetst");
            //modbusTcp.Logger.Error("tetetetst");


            modbusTcp.StartCommunication();

            


            Thread.Sleep(5000);

            var value = SignalStorage.allSignals[1].Value;

            Assert.Pass();

        }


    }
}



