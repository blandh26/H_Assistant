﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
