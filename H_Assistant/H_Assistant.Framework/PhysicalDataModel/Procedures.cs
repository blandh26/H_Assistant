using System.Collections.Generic;

namespace H_Assistant.Framework.PhysicalDataModel
{
    public class Procedures : Dictionary<string, Procedure>
    {
        public Procedures()
            : base()
        {
        }

        public Procedures(int capacity)
            : base(capacity)
        {
        }
    }
}
