using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScada.Communication;
using ExpandScada.SignalsGateway;
using Common.Gateway;

namespace ExpandScada.Commands
{
    public class WriteSignalToDevice : ButtonAction, ICloneable
    {
        public int deviceId = -1;
        public int communicationId = -1;
        public Signal sourceSignal = null;
        public Signal targetSignal = null;
        //Dictionary<string, CommandProperty> properties = new Dictionary<string, CommandProperty>()
        //{
        //     {"SourceSognal", new CommandProperty<string>("SourceSognal")},
        //     {"TargetSignal", new CommandProperty<string>("TargetSignal")},
        //};

        List<string> propertiesNames = new List<string>()
        {
            "SourceSognal",
            "TargetSignal"
        };

        public WriteSignalToDevice(string name) 
            : base(name) 
        { 
            // check if signals exist
            // check if source and target signals have equal type
            // check if target signal has communication settings
            // go through communications and try to find target signal
            // when found - get device or smth to make it possible to write
        }

        public override void Initialize(Dictionary<string, string> propertiesWithValues)
        {
            if (!propertiesWithValues.ContainsKey("SourceSognal") || !propertiesWithValues.ContainsKey("TargetSignal"))
            {
                throw new InvalidOperationException("Not all properties were found in XAML");
            }

            if (!SignalStorage.allNamedSignals.ContainsKey(propertiesWithValues["SourceSognal"]))
            {
                throw new InvalidOperationException($"Signal {propertiesWithValues["SourceSognal"]} not found in storage");
            }

            if (!SignalStorage.allNamedSignals.ContainsKey(propertiesWithValues["TargetSignal"]))
            {
                throw new InvalidOperationException($"Signal {propertiesWithValues["TargetSignal"]} not found in storage");
            }

            sourceSignal = SignalStorage.allNamedSignals[propertiesWithValues["SourceSognal"]];
            targetSignal = SignalStorage.allNamedSignals[propertiesWithValues["TargetSignal"]];

            if (sourceSignal.SignalType != targetSignal.SignalType)
            {
                throw new InvalidOperationException("Signals must have equal type");
            }

            foreach (var protocol in CommunicationManager.communicationProtocols)
            {
                int tmpInt = protocol.Value.GetDeviceIdBySignal(targetSignal);
                if (tmpInt >= 0)
                {
                    communicationId = protocol.Key;
                    deviceId = tmpInt;
                    break;
                }
            }

            if (deviceId < 0 || communicationId < 0)
            {
                throw new InvalidOperationException($"Target signal {targetSignal.name} has not found in communication channels");
            }

        }

        // on execution
        // go to communication/device and call "write signal" with certain name

        public override void Execute()
        {
            targetSignal.ValueToWrite = sourceSignal.Value;
            CommunicationManager.communicationProtocols[communicationId].WriteSignalToDevice(targetSignal, deviceId);
        }

    }
}
