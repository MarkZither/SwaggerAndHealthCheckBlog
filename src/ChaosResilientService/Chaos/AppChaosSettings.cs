using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChaosResilientService
{
    public class AppChaosSettings
    {
        public List<OperationChaosSetting> OperationChaosSettings { get; set; }

        public OperationChaosSetting GetSettingsFor(string operationKey) => OperationChaosSettings?.SingleOrDefault(i => i.OperationKey == operationKey);
    }
}
