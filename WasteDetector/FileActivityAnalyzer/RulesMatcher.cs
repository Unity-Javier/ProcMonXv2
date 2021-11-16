using FileActivityAnalyzer.RuleComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class RulesMatcher
    {
        public Dictionary<string, MatchSummary> m_MatchSummary;
        
        public RulesMatcher(Parser parser, RulesTrie rulesTrie)
        {
            m_MatchSummary = new Dictionary<string, MatchSummary>();

            var infos = parser.GetOperationInfos();

            foreach(var curOperationInfo in infos)
            {
                GenerateMatchesForOperation(curOperationInfo.Value, rulesTrie);

                CreateMatchSummaryForPath(curOperationInfo.Key, curOperationInfo.Value);

            }

            Console.WriteLine("Complete");
        }

        public RulesMatcher(Parser parser, List<IRuleComponent> rules)
        {
            m_MatchSummary = new Dictionary<string, MatchSummary>();

            var infos = parser.GetOperationInfos();

            foreach (var curOperationInfo in infos)
            {
                foreach (var curRule in rules)
                {
                    curRule.BeginMatch(curOperationInfo.Value);
                }

                CreateMatchSummaryForPath(curOperationInfo.Key, curOperationInfo.Value);
            }
        }

        private void CreateMatchSummaryForPath(string key, List<ProcMonOperationInfo> infos)
        {
            var matchSummary = new MatchSummary() {
                MatchedRules = new HashSet<Rule>(),
                UnmatchedOperations = new List<ProcMonOperationInfo>()
            };

            for(int i = 0; i < infos.Count; ++i)
            {
                var matchedRule = infos[i].matchedRule;
                if (matchedRule == null)
                {
                    matchSummary.UnmatchedOperations.Add(infos[i]);
                }
                else if(!matchSummary.MatchedRules.Contains(matchedRule))
                {
                    matchSummary.MatchedRules.Add(matchedRule);
                }
            }

            m_MatchSummary.Add(key, matchSummary);
        }

        private void GenerateMatchesForOperation(List<ProcMonOperationInfo> infos, RulesTrie rulesTrie)
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                var curInfo = infos[i];

                if (curInfo.matchedRule != null)
                    continue;

                var rules = rulesTrie.GetRules();
                var match = rules.ContainsKey(curInfo.opCode);

                if (match && i+1 < infos.Count)
                {
                    var matchedRule = ContinueMatch(infos, i+1, rules[curInfo.opCode]);
                    infos[i].matchedRule = matchedRule;

                    if(matchedRule != null)
                    {
                        var duration = 0.0f;
                        for(int j = 0; j < infos.Count; ++j)
                        {
                            if (infos[j].matchedRule != null)
                            {
                                float.TryParse(infos[j].details.Duration, out var floatDuration);
                                duration += floatDuration;
                            }
                        }
                        Console.WriteLine("Duration: " + duration);
                    }
                }
            }
        }

        private Rule ContinueMatch(List<ProcMonOperationInfo> infos, int index, RulesNode node)
        {
            var curOpCode = infos[index].opCode;
            
            if (node.m_IsRule)
            {
                infos[index].matchedRule = node.m_Rule;
                return node.m_Rule;
            }
            else if (node.m_Siblings.ContainsKey(curOpCode))
            {
                node = node.m_Siblings[curOpCode];

                //Recursively assign the matched rule
                if (index + 1 < infos.Count)
                {
                    var matchedRule = ContinueMatch(infos, index + 1, node);
                    infos[index].matchedRule = matchedRule;
                    return matchedRule;
                }
            }

            return null;
        }
    }

    public class MatchSummary
    {
        public HashSet<Rule> MatchedRules;
        public List<ProcMonOperationInfo> UnmatchedOperations;
    }
}
