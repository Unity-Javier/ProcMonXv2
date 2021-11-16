using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleComponents
{
    public class InvalidParameterComponent : IRuleComponent
    {
        private Rule m_Rule;
        private bool[] m_CanBeOptimized;
        public InvalidParameterComponent(OpCodes opCodes)
        {
            InitRule(opCodes);
        }

        private void InitRule(OpCodes opCodes)
        {
            m_Rule = new Rule()
            {
                ruleName = GetName()
            };
        }

        public bool BeginMatch(List<ProcMonOperationInfo> infos)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                var curInfo = infos[i];

                if (curInfo.matchedRule != null)
                    continue;

                if (string.CompareOrdinal(curInfo.details.Result, "INVALID PARAMETER") == 0)
                {
                    curInfo.matchedRule = m_Rule;
                    m_Rule.estimatedOptimizationTime += curInfo.details.Duration;
                }
            }

            return false;
        }

        public string GetName()
        {
            return "InvalidParameterComponent rule";
        }

        public bool CanBeOptimized(int index)
        {
            return m_CanBeOptimized[index];
        }

        public float GetEstimatedRuleOptimizationTime()
        {
            return m_Rule.estimatedOptimizationTime;
        }
    }
}
