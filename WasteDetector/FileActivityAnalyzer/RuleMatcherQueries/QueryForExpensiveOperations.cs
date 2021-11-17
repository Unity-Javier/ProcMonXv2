using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleMatcherQueries
{
    public class QueryForExpensiveOperations
    {
        private Dictionary<string, OperationAndDuration> m_OperationToDurations;
        private OperationAndDuration[] m_SortedOperationAndDurations;
        public QueryForExpensiveOperations(Parser parser)
        {
            m_OperationToDurations = new Dictionary<string, OperationAndDuration>();
            GatherOperationDurations(parser);

            var comparer = new OperationDurationComparer();
            m_SortedOperationAndDurations = m_OperationToDurations.Values.ToArray();
            Array.Sort(m_SortedOperationAndDurations, comparer);

            var comparer2 = new OperationAverageTimeComparer();
            Array.Sort(m_SortedOperationAndDurations, comparer2);
        }

        public override string ToString()
        {
            return $"Most expensive operation: {m_SortedOperationAndDurations[0].Operation}:{m_SortedOperationAndDurations[0].Duration}";
        }

        private class OperationAverageTimeComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                OperationAndDuration matchA = (OperationAndDuration)a;
                OperationAndDuration matchB = (OperationAndDuration)b;
                return matchA.AverageTimePerCall > matchB.AverageTimePerCall ? -1 : (matchA.AverageTimePerCall < matchB.AverageTimePerCall ? 1 : 0);
            }
        }

        private class OperationDurationComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                OperationAndDuration matchA = (OperationAndDuration)a;
                OperationAndDuration matchB = (OperationAndDuration)b;
                return matchA.Duration > matchB.Duration ? -1 : (matchA.Duration < matchB.Duration ? 1 : 0);
            }
        }

        private void GatherOperationDurations(Parser parser)
        {
            var infos = parser.GetOperationInfos();
            foreach (var curInfo in infos)
            {
                var list = curInfo.Value;
                for (int i = 0; i < list.Count; ++i)
                {
                    var curEntry = list[i];
                    var curOperation = curEntry.operation;

                    m_OperationToDurations.TryGetValue(curOperation, out var operationAndDuration);

                    if (operationAndDuration == null)
                    {
                        m_OperationToDurations.Add(curOperation, new OperationAndDuration()
                        {
                            Operation = curOperation,
                            Duration = curEntry.details.Duration,
                            Instances = 1
                        }) ;
                        continue;
                    }

                    operationAndDuration.Duration = operationAndDuration.Duration + curEntry.details.Duration;
                    operationAndDuration.Instances++;
                    operationAndDuration.AverageTimePerCall = operationAndDuration.Duration / (float) operationAndDuration.Instances;

                }
            }
        }

        public class OperationAndDuration
        {
            public string Operation;
            public float Duration;
            public int Instances;
            public float AverageTimePerCall;
        }
    }
}
