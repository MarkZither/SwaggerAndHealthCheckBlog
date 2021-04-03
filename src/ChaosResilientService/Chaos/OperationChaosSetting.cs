using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChaosResilientService
{
    public class OperationChaosSetting
    {
        public string OperationKey { get; set; }

        public bool Enabled { get; set; }

        public double InjectionRate { get; set; }

        public int StatusCode { get; set; }

        public int LatencyMs { get; set; }

        public string Exception { get; set; }
    }
}
