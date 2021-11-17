using System;

namespace ProcMonUtils
{
    public interface IAddressRange
    {
        ref AddressRange AddressRef { get; }
    }
    
    public struct AddressRange
    {
        public ulong Base;
        public uint Size;
        public ulong End => Base + Size;
        
        public AddressRange(ulong bace, uint size)
        {
            Base = bace;
            Size = size;
        }
    }
    
    public static class AddressRangeExtensions
    {
        public static T? FindAddressIn<T>(this T[] items, ulong address)
            where T : class, IAddressRange
        {
            if (items.Length == 0 || address < items[0].AddressRef.Base || address >= items[items.Length - 1].AddressRef.End)
                return null;

            for (ulong l = 0, h = (ulong)(items.Length - 1); l <= h; )
            {
                var i = l + (h - l) / 2;
                var test = items[i];

                if (test.AddressRef.End <= address)
                    l = i + 1;
                else if (test.AddressRef.Base > address)
                    h = i - 1;
                else
                    return test;
            }
            return null;
        }
    }
}
