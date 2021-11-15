using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class Rules
    {
        private Rule[] m_Rules;
        public Rules(Config config, OpCodes opCodes)
        {
            var rulesLines = File.ReadAllLines(config.RulesFile);
            ExtractRules(rulesLines, opCodes);
        }

        private void ExtractRules(string[] rulesLines, OpCodes opCodes)
        {
            var rules = new List<Rule>();
            var curSteps = new List<string>();
            Rule curRule = null;
            for(int i = 0; i < rulesLines.Length; ++i)
            {
                if(rulesLines[i].StartsWith("=", StringComparison.OrdinalIgnoreCase))   
                {
                    if (string.CompareOrdinal(rulesLines[i], "=End=") == 0)
                    {
                        //Finalize rule
                        curRule.steps = curSteps.ToArray();
                        curRule.PopulateOpCodes(opCodes);
                        
                        curSteps.Clear();
                        rules.Add(curRule);
                    }
                    else
                    {
                        //starting rule
                        var end = rulesLines[i].LastIndexOf("=");
                        var name = rulesLines[i].Substring(1, end-1);
                        curRule = new Rule()
                        {
                            ruleName = name
                        };
                    }
                }
                else
                {
                    curSteps.Add(rulesLines[i]);
                }
            }

            m_Rules = rules.ToArray();
        }

        public Rule[] GetRules()
        {
            return m_Rules;
        }
    }

    public class Rule
    {
        public string ruleName;
        public string[] steps;
        public int[] opCodes;

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
