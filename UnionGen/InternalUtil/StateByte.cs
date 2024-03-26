using System;

namespace UnionGen.InternalUtil;

public readonly struct StateByte
{
    private const int IndexMask = 0b0000_1111;
    private const int ActualTypeMask = 0b1111_0000;
    public const byte RefTypeIndex = IndexMask;

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
                throw new ArgumentOutOfRangeException(nameof(value), value,
                                                      $"Value exceeds the maximum value of {IndexMask} for the index.");
            }

            var cleanState = _state & ActualTypeMask;
            _state = (byte) (cleanState | value);
        }
    }

    public int ActualTypeIndex
    {
        get => _state >> 4;
        private init
        {
            if (value > IndexMask)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                                                      $"Value exceeds the maximum value of {IndexMask} for the actual type index.");
            }

            var shiftedValue = value << 4;
            var cleanState = _state & IndexMask;
            _state = (byte) (cleanState | shiftedValue);
        }
    }
}
