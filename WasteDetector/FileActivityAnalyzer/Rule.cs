using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class Rule
    {
        public string ruleName;
        public string[] steps;
        public int[] opCodes;
        public float estimatedOptimizationTime = 0.0f;

        internal void PopulateOpCodes(OpCodes codes)
        {
            if (steps == null || steps.Length == 0)
                throw new Exception("Steps have not been initialized yet");
            opCodes = new int[steps.Length];

            for(int i = 0; i < opCodes.Length; ++i)
            {
                opCodes[i] = codes.GetOpCode(steps[i]);
            }
        }
    }
}
