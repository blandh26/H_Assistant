using System.Collections.Generic;

namespace H_Assistant.Framework.PhysicalDataModel
{
    public class Tables : Dictionary<string, Table>
    {
        public Tables()
            : base()
        {
        }

        public Tables(int capacity)
            : base(capacity)
        {
        }
    }
}
