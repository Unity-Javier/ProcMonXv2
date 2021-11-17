using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleComponents
{
    public class NotifyChangeDirectoryComponent : IRuleComponent
    {
        private Rule m_Rule;
        int NotifyChangeDirectoryOperation;
        public NotifyChangeDirectoryComponent(OpCodes opCodes)
        {
            InitRule(opCodes);
        }

        private void InitRule(OpCodes opCodes)
        {
            m_Rule = new Rule()
            {
                ruleName = GetName()
            };

            NotifyChangeDirectoryOperation = opCodes.GetOpCode("NotifyChangeDirectory");
        }

        public bool BeginMatch(List<ProcMonOperationInfo> infos)
        {   
            for (int i = 0; i < infos.Count; ++i)
            {
                var curInfo = infos[i];

                if (curInfo.matchedRule != null)
                    continue;

                if (curInfo.opCode == NotifyChangeDirectoryOperation && curInfo.details.Duration > 1.0f)
                {
                    curInfo.matchedRule = m_Rule;
                    m_Rule.estimatedOptimizationTime += curInfo.details.Duration;
                }
            }

            return false;
        }

        public string GetName()
        {
            return "NotifyChangeDirectoryComponent rule";
        }

        public bool CanBeOptimized(int index)
        {
            return true;
        }

        public float GetEstimatedRuleOptimizationTime()
        {
            return m_Rule.estimatedOptimizationTime;
        }
    }
}
