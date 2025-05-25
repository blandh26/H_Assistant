using System.Collections.Generic;

namespace H_Assistant.Framework.PhysicalDataModel
{
    public class Views : Dictionary<string, View>
    {
        public Views()
            : base()
        {
        }

        public Views(int capacity)
            : base(capacity)
        {
        }
    }
}
