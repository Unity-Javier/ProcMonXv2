using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleComponents
{
    public class ReadAfterWriteWithNoOtherOperations : IRuleComponent
    {
        private enum States
        {
            State_Idle,
            State_WriteDetected,
            State_ReadDetected
        };

        private Rule m_Rule;
        private States m_State;

        public ReadAfterWriteWithNoOtherOperations(OpCodes opCodes)
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
                    "WriteFile",
                    "ReadFile"
                }
            };

            m_Rule.opCodes = new int[m_Rule.steps.Length];

            for (int i = 0; i < m_Rule.steps.Length; ++i)
            {
                m_Rule.opCodes[i] = opCodes.GetOpCode(m_Rule.steps[i]);
            }

            m_State = States.State_Idle;
        }

        public bool BeginMatch(List<ProcMonOperationInfo> infos)
        {
            int matchStartIndex = 0;
            int matchEndIndex = 0;
            bool didMatch = false;
            for (int i = 0; i < infos.Count; ++i)
            {
                var curInfo = infos[i];
                if (curInfo.matchedRule != null)
                    continue;

                switch (m_State)
                {
                    case States.State_Idle:
                    {
                        if (curInfo.opCode == m_Rule.opCodes[0])
                        {
                            m_State = States.State_WriteDetected;
                            matchStartIndex = i;
                        }
                        break;
                    }
                    case States.State_WriteDetected:
                    {
                        //Ignore writing and then reading
                        if (curInfo.opCode == m_Rule.opCodes[1])
                        {
                            matchEndIndex = i;
                            m_State = States.State_ReadDetected;
                        }
                        else/* if(curInfo.opCode == m_Rule.opCodes[0])*/
                        {
                            matchStartIndex = 0;
                            matchEndIndex = 0;
                            m_State = States.State_Idle;
                        }
                        break;
                    }
                    case States.State_ReadDetected:
                    {
                        //Handle multiple Reads in a row
                        if (curInfo.opCode != m_Rule.opCodes[1])
                        {
                            for (int j = matchStartIndex; j < matchEndIndex; ++j)
                            {
                                infos[j].matchedRule = m_Rule;
                                m_Rule.estimatedOptimizationTime += infos[j].details.Duration;
                            }

                            didMatch = true;
                            m_State = States.State_Idle;                                        
                        }
                        else
                            matchEndIndex = i;
                        break;
                    }
                }
            }

            return didMatch;
        }

        public string GetName()
        {
            return "ReadAfterWriteWithNoOtherOperations rule";
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
