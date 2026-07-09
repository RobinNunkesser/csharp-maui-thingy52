namespace Thingy52;

internal sealed class ThingyAdpcmDecoder
{
    private const int FrameSize = 131;

    private static readonly int[] IndexTable =
    {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8
    };

    private static readonly short[] StepTable =
    {
         7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28,
         31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143,
         157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544,
         598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878,
         2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894,
         6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818,
         18500, 20350, 22385, 24623, 27086, 29794, 32767
    };

    private readonly List<byte> _frameBuffer = new();

    public void Reset() => _frameBuffer.Clear();

    public short[] Decode(byte[] data)
    {
        if (data.Length == 0)
            return Array.Empty<short>();

        _frameBuffer.AddRange(data);
        if (_frameBuffer.Count < FrameSize)
            return Array.Empty<short>();

        var frame = _frameBuffer.GetRange(0, FrameSize).ToArray();
        _frameBuffer.Clear();

        var valuePredicted = (short)((frame[0] << 8) | frame[1]);
        var index = Math.Clamp((int)frame[2], 0, 88);
        var step = StepTable[index];

        var output = new short[(FrameSize - 3) * 2];
        var outputIndex = 0;
        byte nextValue = 0;
        var readLowNibble = false;

        for (var i = 0; i < output.Length; i++)
        {
            byte delta;
            if (readLowNibble)
            {
                delta = (byte)(nextValue & 0x0F);
            }
            else
            {
                nextValue = frame[3 + (i / 2)];
                delta = (byte)((nextValue >> 4) & 0x0F);
            }
            readLowNibble = !readLowNibble;

            index += IndexTable[delta];
            index = Math.Clamp(index, 0, 88);

            var sign = (delta & 0x08) != 0;
            var mag = (byte)(delta & 0x07);

            var diff = step >> 3;
            if ((mag & 0x04) != 0) diff += step;
            if ((mag & 0x02) != 0) diff += step >> 1;
            if ((mag & 0x01) != 0) diff += step >> 2;

            var predicted = valuePredicted + (sign ? -diff : diff);
            valuePredicted = (short)Math.Clamp(predicted, short.MinValue, short.MaxValue);

            step = StepTable[index];
            output[outputIndex++] = valuePredicted;
        }

        return output;
    }
}
