using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleComponents
{
    public class FlushBuffersFile : IRuleComponent
    {
        private Rule m_Rule;
        private bool[] m_CanBeOptimized;
        public FlushBuffersFile(OpCodes opCodes)
        {
            InitRule(opCodes);
        }

        private void InitRule(OpCodes opCodes)
        {
            m_Rule = new Rule()
            {
                ruleName = GetName(),
                steps = new string[]
                {
                    "FlushBuffersFile "
                }
            };

            m_CanBeOptimized = new bool[]
            {
                true//"FlushBuffersFile ",
            };

            m_Rule.opCodes = new int[m_Rule.steps.Length];

            for(int i = 0; i < m_Rule.steps.Length; ++i)
            {
                 m_Rule.opCodes[i] = opCodes.GetOpCode(m_Rule.steps[i]);                
            }
        }

        public bool BeginMatch(List<ProcMonOperationInfo> infos)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                var curInfo = infos[i];

                if (curInfo.matchedRule != null)
                    continue;

                Match(infos, i);
            }

            return false;
        }

        //This is custom match logic for each IRuleComponent
        private void Match(List<ProcMonOperationInfo> infos, int index)
        {
            for (int i = 0; i < m_Rule.opCodes.Length; ++i)
            {
                if (index + i >= infos.Count || infos[index + i].opCode != m_Rule.opCodes[i])
                {
                    //No Match
                    return;
                }
            }

            var end = index + m_Rule.opCodes.Length;
            for (int i = 0; i < m_Rule.opCodes.Length; ++i)
            {
                //Match & fill up all the infos so we don't try and match them again
                infos[index + i].matchedRule = m_Rule;
                if (CanBeOptimized(i))
                {
                    m_Rule.estimatedOptimizationTime += infos[index + i].details.Duration;
                }
            }
        }

        public string GetName()
        {
            return "FlushBuffersFile rule";
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
