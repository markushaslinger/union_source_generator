using System;

namespace UnionGen.InternalUtil;

public readonly struct StateByte
{
    private const int ShiftOffset = 4;
    private const int IndexMask = 0b0000_1111;
    private const int ActualTypeMask = 0b1111_0000;
    public const byte RefTypeIndex = IndexMask;
    
    private static readonly string outOfRangeMessage = $"Value exceeds the maximum value of {IndexMask} for the index.";

    private readonly byte _state;

    public StateByte(int index, int actualTypeIndex)
    {
        Index = (byte) index;
        ActualTypeIndex = (byte) actualTypeIndex;
    }

    public int Index
    {
        get => _state & IndexMask;
        private init
        {
            if (value > IndexMask)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, outOfRangeMessage);
            }

            var cleanState = _state & ActualTypeMask;
            _state = (byte) (cleanState | value);
        }
    }

    public int ActualTypeIndex
    {
        get => _state >> ShiftOffset;
        private init
        {
            if (value > IndexMask)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, outOfRangeMessage);
            }

            var shiftedValue = value << ShiftOffset;
            var cleanState = _state & IndexMask;
            _state = (byte) (cleanState | shiftedValue);
        }
    }
}
