using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScada.Commands
{
    public abstract class ButtonAction : ICloneable
    {

        public string Name { get; protected set; }

        //public Dictionary<string, CommandProperty> Properties { get; protected set; }

        public ButtonAction(string name)
        {
            Name = name;
            //Properties = properties;
        }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(Dictionary<string, string> propertiesWithValues)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
