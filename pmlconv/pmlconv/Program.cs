// See https://aka.ms/new-console-template for more information

using PmlConversion;

PmlSymbolicator.Symbolicate(
    @"C:\work\_forks\ProcMonXv2\pmlconv\procmonlibtests\testdata\basic.PML",
    @"C:\work\_forks\ProcMonXv2\pmlconv\procmonlibtests\testdata\pmip_38624_1.txt",
    @"c:\temp\frames.txt",
    @"c:\temp\map.txt");
