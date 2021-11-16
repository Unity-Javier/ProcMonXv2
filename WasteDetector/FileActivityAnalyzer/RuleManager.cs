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
            m_Rules.Add(new HasFileChangedComponent(opCodes));
            m_Rules.Add(new CreateFileThenSetAttributesComponent(opCodes));
            m_Rules.Add(new WriteToFileAndHashOnCloseComponent(opCodes));
            m_Rules.Add(new MoveTempFileToLibraryFolderComponent(opCodes));
            m_Rules.Add(new InvalidParameterComponent(opCodes));
            m_Rules.Add(new TextRegistryDoubleWriteASMDefComponent(opCodes));

            SanitizeRules();
        }

        private void SanitizeRules()
        {
            var names = new HashSet<string>();
            for(int i = 0; i < m_Rules.Count; ++i)
            {
                names.Add(m_Rules[i].GetName());
            }
        }

        public List<IRuleComponent> GetRuleComponents()
        {
            return m_Rules;
        }
    }
}
