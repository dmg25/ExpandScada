using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.SignalsGateway;

namespace ExpandScada.Test.Gateway
{
    public class BoxingUnboxingPerformance
    {
        /*
         *  - each step calculate time and write it somewhere as result
         *  - create N num of different signals with different simple types
         *  - get all values in local array/list
         *  - set all SAME values to each signal M times
         *  - set all DIFFERENT values, which requires type conversion
         *  
         * 
         * */

        [SetUp]
        public void Setup()
        {
            // set 1000 int/uint/float/double each

            int indexCounter = 0;

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals.TryAdd(indexCounter, new Signal<int>(indexCounter, indexCounter.ToString(), ""));
                indexCounter++;
            }

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals.TryAdd(indexCounter, new Signal<uint>(indexCounter, indexCounter.ToString(), ""));
                indexCounter++;
            }

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals.TryAdd(indexCounter, new Signal<float>(indexCounter, indexCounter.ToString(), ""));
                indexCounter++;
            }

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals.TryAdd(indexCounter, new Signal<double>(indexCounter, indexCounter.ToString(), ""));
                indexCounter++;
            }

        }

        [Test]
        public void BoxingUnboxing()
        {
            // warming 
            foreach (var signal in SignalStorage.allSignals)
            {
                object tmp = signal.Value.Value;
                signal.Value.Value = tmp;
            }


            // get all values to local vars and calculate time
            int tmpInt = 0;
            uint tmpUInt = 0;
            float tmpFloat = 0;
            double tmpDouble = 0;
            DateTime beforeGetting = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                tmpInt = (int)SignalStorage.allSignals[i].Value;
            }

            for (int i = 1000; i < 2000; i++)
            {
                tmpUInt = (uint)SignalStorage.allSignals[i].Value;
            }

            for (int i = 2000; i < 3000; i++)
            {
                tmpFloat = (float)SignalStorage.allSignals[i].Value;
            }

            for (int i = 3000; i < 4000; i++)
            {
                tmpDouble = (double)SignalStorage.allSignals[i].Value;
            }

            DateTime afterGetting = DateTime.Now;

            TimeSpan gettingResult = afterGetting - beforeGetting; //.0006669

            // set all same values and calculate time 
            tmpInt = 1;
            tmpUInt = 2;
            tmpFloat = 3f;
            tmpDouble = 4d;
            DateTime beforeSameSetting = DateTime.Now; 

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals[i].Value = tmpInt;
            }

            for (int i = 1000; i < 2000; i++)
            {
                SignalStorage.allSignals[i].Value = tmpUInt;
            }

            for (int i = 2000; i < 3000; i++)
            {
                SignalStorage.allSignals[i].Value = tmpFloat;
            }

            for (int i = 3000; i < 4000; i++)
            {
                SignalStorage.allSignals[i].Value = tmpDouble;
            }

            DateTime afterSameSetting = DateTime.Now;
            TimeSpan sameSettingResult = afterSameSetting - beforeSameSetting; //.0007457

            // set different values
            DateTime beforeDiffSetting = DateTime.Now;

            for (int i = 0; i < 1000; i++)
            {
                SignalStorage.allSignals[i].Value = (int)tmpDouble;
            }

            for (int i = 1000; i < 2000; i++)
            {
                SignalStorage.allSignals[i].Value = (uint)tmpFloat;
            }

            for (int i = 2000; i < 3000; i++)
            {
                SignalStorage.allSignals[i].Value = (float)tmpDouble;
            }

            for (int i = 3000; i < 4000; i++)
            {
                SignalStorage.allSignals[i].Value = (double)tmpUInt;
            }

            DateTime afterDiffSetting = DateTime.Now;
            TimeSpan diffSettingResult = afterDiffSetting - beforeDiffSetting;


            TestContext.WriteLine($"Getting: {gettingResult.TotalMilliseconds} ms");
            TestContext.WriteLine($"Same setting: {sameSettingResult.TotalMilliseconds} ms");
            TestContext.WriteLine($"Diff setting: {diffSettingResult.TotalMilliseconds} ms");

            Assert.Pass();
        }
    }
}


/*  First implementation
 *  
 *  Getting: 0,2225 ms
    Same setting: 0,1184 ms
    Diff setting: 0,1156 ms
 * 
 * 
 * 
 * 
 * */


