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
        public Rules(Config config)
        {
            var rulesLines = File.ReadAllLines(config.RulesFile);
            ExtractRules(rulesLines);
        }

        private void ExtractRules(string[] rulesLines)
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
            Console.WriteLine("Done");
        }
    }

    public class Rule
    {
        public string ruleName;
        public string[] steps;
    }
}
