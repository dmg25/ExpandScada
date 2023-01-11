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
            CommunicationLoader.LoadAllProtocols("..\\..\\Protocols\\Debug\\net6.0", "..\\..\\Project\\test1.db");
            var name = CommunicationManager.communicationProtocols[0].Name;
            var modbusTcp = CommunicationManager.communicationProtocols[0];

            // check if this protocol exists in the table in DB - in scada

            // load signals - just here

            // load communication signals with settings 





            //modbusTcp.InitializeProtocol()



            Assert.Pass();

        }

     

    }
}



