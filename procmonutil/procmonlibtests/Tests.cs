using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProcMonUtils;
using Shouldly;

class Tests
{
    // crap tests just to get something going..
    
    [Test]
    public void PmipBasics()
    {
        var mono = new MonoSymbolReader(@"C:\work\_forks\ProcMonXv2\procmonutil\procmonlibtests\testdata\pmip_38624_1.txt");
        foreach (var symbol in mono.Symbols)
        {
            mono.FindSymbol(symbol.Address.Base + symbol.Address.Size / 2).ShouldBe(symbol);
            mono.FindSymbol(symbol.Address.Base).ShouldBe(symbol);
            mono.FindSymbol(symbol.Address.End - 1).ShouldBe(symbol);
        }
        
        mono.FindSymbol(mono.Symbols[0].Address.Base - 1).ShouldBeNull();
        mono.FindSymbol(mono.Symbols[mono.Symbols.Length - 1].Address.End).ShouldBeNull();
    }
    
    [Test]
    public void WriteAndParse()
    {
        PmlUtils.Symbolicate(
            @"C:\work\_forks\ProcMonXv2\procmonutil\procmonlibtests\testdata\basic.PML",
            @"C:\work\_forks\ProcMonXv2\procmonutil\procmonlibtests\testdata\pmip_38624_1.txt",
            @"c:\temp\frames.txt");
        
        var pmlQuery = new PmlQuery(@"c:\temp\frames.txt");
        
        var frame = pmlQuery.GetRecordBySequence(36).Frames[2];
        frame.Module.ShouldBe("FLTMGR.SYS");
        frame.Type.ShouldBe(FrameType.Kernel);
        frame.Symbol.ShouldBe("FltGetFileNameInformation");
        frame.Offset.ShouldBe(0x992);
    }
    
    [Test]
    public void Match()
    {
        var pmlQuery = new PmlQuery(@"c:\temp\frames.txt");
        
        // find all events where someone is calling a dotnet generic
        var matches = pmlQuery
            .MatchRecordsBySymbol(new Regex("`"))
            .OrderBy(seq => seq)
            .Select(seq => pmlQuery.GetRecordBySequence(seq))
            .ToList();
        
        matches.First().Sequence.ShouldBe(3);
        matches.Last().Sequence.ShouldBe(311);
    }
}
