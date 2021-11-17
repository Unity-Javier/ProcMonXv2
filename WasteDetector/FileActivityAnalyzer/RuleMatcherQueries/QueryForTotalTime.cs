using FileActivityAnalyzer.RuleComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleMatcherQueries
{
    public class QueryForTotalTime
    {
        public float m_TotalDuration;
        public float TotalEstimatedOptimizations;

        public QueryForTotalTime(Parser parser, List<IRuleComponent> rules)
        {
            GatherTotalTime(parser);
            GatherTotalOptimizationEstimate(rules);
        }

        private void GatherTotalOptimizationEstimate(List<IRuleComponent> rules)
        {
            for (int i = 0; i < rules.Count; ++i)
                TotalEstimatedOptimizations += rules[i].GetEstimatedRuleOptimizationTime();
        }

        private void GatherTotalTime(Parser parser)
        {
            var infos = parser.GetOperationInfos();
            foreach(var curInfo in infos)
            {
                AddToTotalDuration(curInfo.Value);
            }
        }

        private void AddToTotalDuration(List<ProcMonOperationInfo> value)
        {
            foreach (var curValue in value)
                m_TotalDuration += curValue.details.Duration;
        }

        public override string ToString()
        {
            return $"Estimated time saved by optimizations: {TotalEstimatedOptimizations}s out of {m_TotalDuration}s";
        }
    }
}
