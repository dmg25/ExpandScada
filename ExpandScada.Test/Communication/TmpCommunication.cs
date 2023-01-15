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
            SignalLoader.LoadAllSignals("..\\..\\Project\\test1.db");
            CommunicationLoader.LoadAllProtocols("..\\..\\Protocols\\Debug\\net6.0-windows", "..\\..\\Project\\test1.db");
            var name = CommunicationManager.communicationProtocols[0].Name;
            var modbusTcp = CommunicationManager.communicationProtocols[0];

            modbusTcp.StartCommunication();

            Thread.Sleep(5000);

            var value = SignalStorage.allSignals[1].Value;




            Assert.Pass();

        }

     

    }
}



