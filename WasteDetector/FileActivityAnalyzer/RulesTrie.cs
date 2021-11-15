using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class RulesTrie
    {
        private Dictionary<int, RulesNode> m_Rules;

        public RulesTrie(Rules rules)
        {
            m_Rules = new Dictionary<int, RulesNode>();
            RulesNode curNode = null;
            var allRules = rules.GetRules();
            for (int i = 0; i < allRules.Length; ++i)
            {
                var curRule = allRules[i];

                m_Rules.TryGetValue(curRule.opCodes[0], out curNode);

                if(curNode == null)
                {
                    curNode = new RulesNode()
                    {
                        m_IsRule = curRule.opCodes.Length == 1,
                        m_Siblings = new Dictionary<int, RulesNode>()
                    };

                    if (curNode.m_IsRule)
                        curNode.m_Rule = curRule;

                    m_Rules.Add(curRule.opCodes[0], curNode);
                }

                PopulateTrie(curNode, curRule, 1);
            }
        }

        public Dictionary<int, RulesNode> GetRules()
        {
            return m_Rules;
        }

        private void PopulateTrie(RulesNode curNode, Rule rule, int ruleIndex)
        {
            var curOpCode = rule.opCodes[ruleIndex];
            if(curNode.m_Siblings.ContainsKey(curOpCode))
            {
                curNode = curNode.m_Siblings[curOpCode];
                PopulateTrie(curNode, rule, ++ruleIndex);
            }
            else
            {
                var newNode = new RulesNode()
                {
                    m_IsRule = (rule.opCodes.Length-1) == ruleIndex,
                    m_Siblings = new Dictionary<int, RulesNode>()
                };

                curNode.m_Siblings.Add(curOpCode, newNode);
                curNode = curNode.m_Siblings[curOpCode];

                if (curNode.m_IsRule)
                {
                    curNode.m_Rule = rule;
                }
                else
                {
                    
                    PopulateTrie(curNode, rule, ++ruleIndex);
                }
            }
        }
    }

    public class RulesNode
    {
        public Dictionary<int, RulesNode> m_Siblings;
        public bool m_IsRule;
        public Rule m_Rule;
    }

}
