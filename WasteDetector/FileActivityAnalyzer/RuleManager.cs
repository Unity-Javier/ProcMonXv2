using FileActivityAnalyzer.RuleComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class RuleManager
    {
        private List<IRuleComponent> m_Rules;

        public RuleManager(OpCodes opCodes)
        {
            m_Rules = new List<IRuleComponent>();
            m_Rules.Add(new FileHasherComponent(opCodes));
        }

        public List<IRuleComponent> GetRuleComponents()
        {
            return m_Rules;
        }
    }
}
