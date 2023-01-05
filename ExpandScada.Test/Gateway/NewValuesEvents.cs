using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.SignalsGateway;

namespace ExpandScada.Test.Gateway
{
    // TODO change are assert conditions
    /// <summary>
    /// !!!TIME LIMITS ARE VALID FOR MY PC ONLY!!!
    /// </summary>
    public class NewValuesEvents
    {
        public int valueChangedCounter = 0;
        public int valueChangedNotEqualCounter = 0;
        DateTime beforeComparing;
        DateTime afterComparing;
        TimeSpan result;

        [SetUp]
        public void Setup()
        {
            int indexCounter = 0;


            SignalStorage.allSignals.TryAdd(indexCounter, new Signal<int>(indexCounter, indexCounter.ToString(), ""));
            indexCounter++;
            SignalStorage.allSignals.TryAdd(indexCounter, new Signal<double>(indexCounter, indexCounter.ToString(), ""));
            indexCounter++;
            SignalStorage.allSignals.TryAdd(indexCounter, new Signal<string>(indexCounter, indexCounter.ToString(), ""));
            indexCounter++;
            SignalStorage.allSignals.TryAdd(indexCounter, new Signal<double[]>(indexCounter, indexCounter.ToString(), ""));
        }

        [Test]
        public void NewValuesEventsTest()
        {
            int tmpValueChangedCounter = 0;
            int tmpValueChangedNotEqualCounter = 0;

            // warming 
            foreach (var signal in SignalStorage.allSignals)
            {
                signal.Value.checkEquality = true;
                signal.Value.PropertyChanged += Value_PropertyChanged;
                signal.Value.PropertyChangedNotEqual += Value_PropertyNotEqualChanged;
            }

            // int
            // equal - equal - not equal - equal ...
            beforeComparing = DateTime.Now;
            int tmpCounter = 0;
            int tmpValueI = 0;
            for (int i = 0; i < 1000; i++)
            {
                tmpCounter++;
                if (tmpCounter >= 3)
                {
                    tmpCounter = 0;
                    tmpValueChangedNotEqualCounter++;
                    SignalStorage.allSignals[0].Value = i;
                    tmpValueI = i;
                }
                else
                {
                    SignalStorage.allSignals[0].Value = tmpValueI;
                }
                tmpValueChangedCounter++;
            }

            afterComparing = DateTime.Now;
            result = afterComparing - beforeComparing;

            TestContext.WriteLine($"Compared int in: {result.TotalMilliseconds} ms");
            Assert.AreEqual(valueChangedCounter, tmpValueChangedCounter);
            Assert.AreEqual(valueChangedNotEqualCounter, tmpValueChangedNotEqualCounter);

            // double
            // equal - equal - not equal - equal ...
            valueChangedCounter = 0;
            valueChangedNotEqualCounter = 0;
            tmpValueChangedCounter = 0;
            tmpValueChangedNotEqualCounter = 0;
            beforeComparing = DateTime.Now;
            tmpCounter = 0;
            double tmpValueD = 0;
            for (int i = 0; i < 1000; i++)
            {
                tmpCounter++;
                if (tmpCounter >= 3)
                {
                    tmpCounter = 0;
                    tmpValueChangedNotEqualCounter++;
                    SignalStorage.allSignals[1].Value = (double)i / 2d;
                    tmpValueD = (double)i / 2d;
                }
                else
                {
                    SignalStorage.allSignals[1].Value = tmpValueD;
                }
                tmpValueChangedCounter++;
            }

            afterComparing = DateTime.Now;
            result = afterComparing - beforeComparing;

            TestContext.WriteLine($"Compared double in: {result.TotalMilliseconds} ms");
            Assert.AreEqual(valueChangedCounter, tmpValueChangedCounter);
            Assert.AreEqual(valueChangedNotEqualCounter, tmpValueChangedNotEqualCounter);


            // string
            // equal - equal - not equal - equal ...
            valueChangedCounter = 0;
            valueChangedNotEqualCounter = 0;
            tmpValueChangedCounter = 0;
            tmpValueChangedNotEqualCounter = 0;
            beforeComparing = DateTime.Now;
            tmpCounter = 0;
            string tmpValueS = null;
            for (int i = 0; i < 1000; i++)
            {
                tmpCounter++;
                if (tmpCounter >= 3)
                {
                    tmpCounter = 0;
                    tmpValueChangedNotEqualCounter++;
                    SignalStorage.allSignals[2].Value = i.ToString();
                    tmpValueS = i.ToString();
                }
                else
                {
                    SignalStorage.allSignals[2].Value = tmpValueS;
                }
                tmpValueChangedCounter++;
            }

            afterComparing = DateTime.Now;
            result = afterComparing - beforeComparing;

            TestContext.WriteLine($"Compared string in: {result.TotalMilliseconds} ms");
            Assert.AreEqual(valueChangedCounter, tmpValueChangedCounter);
            Assert.AreEqual(valueChangedNotEqualCounter, tmpValueChangedNotEqualCounter);

            // array
            // TODO arrays do not work good. Compares only link on the object. Even the arrays will be equal but with different link - not equal!
            // equal - equal - not equal - equal ...
            //valueChangedCounter = 0;
            //valueChangedNotEqualCounter = 0;
            //tmpValueChangedCounter = 0;
            //tmpValueChangedNotEqualCounter = 0;
            //beforeComparing = DateTime.Now;
            //tmpCounter = 0;
            //double[] tmpValueArr = null;
            //for (int i = 0; i < 1000; i++)
            //{
            //    tmpCounter++;
            //    if (tmpCounter >= 3)
            //    {
            //        tmpCounter = 0;
            //        tmpValueChangedNotEqualCounter++;
            //        tmpValueArr = new double[i];
            //        SignalStorage.allSignals[3].Value = new double[i];
                    
            //    }
            //    else
            //    {
            //        SignalStorage.allSignals[3].Value = tmpValueArr;
            //    }
            //    tmpValueChangedCounter++;
            //}

            //afterComparing = DateTime.Now;
            //result = afterComparing - beforeComparing;

            //TestContext.WriteLine($"Compared array in: {result.TotalMilliseconds} ms");
            //Assert.AreEqual(valueChangedCounter, tmpValueChangedCounter);
            //Assert.AreEqual(valueChangedNotEqualCounter, tmpValueChangedNotEqualCounter);

        }

        private void Value_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                valueChangedCounter++;
            }
        }

        private void Value_PropertyNotEqualChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                valueChangedNotEqualCounter++;
            }
        }


    }
}



