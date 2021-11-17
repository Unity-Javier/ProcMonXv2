using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleMatcherQueries
{
    public class QueryForExpensivePaths
    {
        private MatchSummary[] m_Matches;
        public QueryForExpensivePaths(Dictionary<string, MatchSummary> matchSummary)
        {
            //Sort match summary
            var m_Matches = matchSummary.Values.ToArray();
            var comparer = new TotalDurationComparer();
            Array.Sort(m_Matches, comparer);
        }

        public override string ToString()
        {
            return $"Most expensive path: {m_Matches[0].TotalDuration}, {m_Matches[0].UnmatchedOperations[0].details.Path}";
        }

        private class TotalDurationComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                MatchSummary matchA = (MatchSummary)a;
                MatchSummary matchB = (MatchSummary)b;
                return matchA.TotalDuration > matchB.TotalDuration ? -1 : (matchA.TotalDuration < matchB.TotalDuration ? 1 : 0);
            }
        }
    }
}
