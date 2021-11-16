using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer.RuleComponents
{
    public interface IRuleComponent
    {
        bool BeginMatch(List<ProcMonOperationInfo> infos);
    }
}
