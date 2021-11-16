using NUnit.Framework;
using PmlConversion;
using Shouldly;

class Tests
{
    [Test]
    public void Basics()
    {
        var mono = new MonoSymbolReader(@"C:\work\_forks\ProcMonXv2\pmlconv\procmonlibtests\testdata\pmip_38624_1.txt");
        foreach (var symbol in mono.Symbols)
        {
            mono.FindSymbol(symbol.BaseAddress + symbol.Size / 2).ShouldBe(symbol);
            mono.FindSymbol(symbol.BaseAddress).ShouldBe(symbol);
            mono.FindSymbol(symbol.EndAddress - 1).ShouldBe(symbol);
        }
        
        mono.FindSymbol(mono.Symbols[0].BaseAddress - 1).ShouldBeNull();
        mono.FindSymbol(mono.Symbols[^1].EndAddress).ShouldBeNull();
    }
}
