using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.SignalsGateway;
using Common.Gateway;

namespace ExpandScada.Test.Gateway
{
    // TODO change are assert conditions
    /// <summary>
    /// !!!TIME LIMITS ARE VALID FOR MY PC ONLY!!!
    /// </summary>
    public class BoxingUnboxingPerformance
    {
        const double GETTING_LIMIT = 0.24d;
        const double SAME_SETTING_LIMIT = 0.15d;
        const double DIFF_SETTING_LIMIT = 0.15d;


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
                //signal.Value.PropertyChanged += Value_PropertyChanged;
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

            Assert.IsTrue(gettingResult.TotalMilliseconds < GETTING_LIMIT);
            Assert.IsTrue(sameSettingResult.TotalMilliseconds < SAME_SETTING_LIMIT);
            Assert.IsTrue(diffSettingResult.TotalMilliseconds < DIFF_SETTING_LIMIT);

            Assert.Pass();
        }

        //private void Value_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Value")
        //    {
        //        var tmp = ((Signal)sender).Value;
        //    }

            
        //}

        
    }
}


/*  First implementation
 *  
 *  Getting: 0,2225 ms
    Same setting: 0,1184 ms
    Diff setting: 0,1156 ms
 * **************************************
 * 
 * First implementation with Get function by property notification (every setting = setting + getting time) - good!
 *  Getting: 0,2329 ms
    Same setting: 0,3582 ms
    Diff setting: 0,2889 ms
 * 
 * 
 *  Only double signal without objects
  
 *  Getting: 0,1847 ms
    Same setting: 0,023 ms
    Diff setting: 0,0951 ms
 * **************************************
 * 
 * */


