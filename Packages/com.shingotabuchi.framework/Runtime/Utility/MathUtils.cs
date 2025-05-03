using System.Runtime.CompilerServices;

public static class MathUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Square(float x) => x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Square(double x) => x * x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Square(int x) => x * x;
}
