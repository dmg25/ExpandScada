using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.SignalsGateway;
using Common.Gateway;
using ExpandScada.Communication;

namespace ExpandScada.Test.Gateway
{

    public class Loader
    {


        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void LoaderTest()
        {
            ExpandScada.SignalsGateway.SignalLoader.LoadAllSignals("..\\..\\Project\\test1.db");

            var num = SignalStorage.allSignals.Count;

            Assert.IsTrue(num > 0);

        }



    }
}



