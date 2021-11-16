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
    }

    public class MatchSummary
    {
        public HashSet<Rule> MatchedRules;
        public List<ProcMonOperationInfo> UnmatchedOperations;
    }
}
