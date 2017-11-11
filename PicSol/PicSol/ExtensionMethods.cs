using System.Collections;

namespace PicSol
{
    internal static class ExtensionMethods
    {

        internal static void SetBits(this BitArray ba, int startIx, int length, bool value)
        {
            for (int i = startIx; i < (startIx + length); i++)
            {
                ba.Set(i, value);
            }
        }
    }
}
