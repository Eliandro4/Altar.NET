using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class ILHacks
{
    public static TEnum CombineEnums<TEnum>(TEnum a, TEnum b) where TEnum : struct, Enum
    {
        var aVal = Unsafe.As<TEnum, int>(ref a);
        var bVal = Unsafe.As<TEnum, int>(ref b);
        int result = aVal | bVal;
        return Unsafe.As<int, TEnum>(ref result);
    }

    public static int SizeOf<T>() where T : struct
    {
        return Unsafe.SizeOf<T>();
    }

    public static byte[] ToByteArray<T>(ref T v) where T : struct
    {
        int size = Unsafe.SizeOf<T>();
        byte[] result = new byte[size];
        Unsafe.CopyBlockUnaligned(ref result[0], ref Unsafe.As<T, byte>(ref v), (uint)size);
        return result;
    }

    public static byte[] ToByteArray<T>(T v) where T : struct
    {
        return ToByteArray(ref v);
    }

    public unsafe static void Cpblk<T>(ref T source, void* target) where T : struct
    {
        Unsafe.CopyBlockUnaligned(
            destination: target,
            source: Unsafe.AsPointer(ref source),
            byteCount: (uint)Unsafe.SizeOf<T>()
        );
    }


    public unsafe static void Cpblk<T>(void* source, ref T target) where T : struct
    {
        Unsafe.CopyBlockUnaligned(
            destination: Unsafe.AsPointer(ref target),
            source: source,
            byteCount: (uint)Unsafe.SizeOf<T>()
        );
    }

    public unsafe static void Cpblk<T>(T[] source, void* target, int index, int size)
    {
        fixed (T* p = &source[index])
        {
            Unsafe.CopyBlockUnaligned(target, p, (uint)size);
        }
    }

    public unsafe static void Cpblk<T>(void* source, T[] target, int index, int size)
    {
        fixed (T* p = &target[index])
        {
            Unsafe.CopyBlockUnaligned(p, source, (uint)size);
        }
    }

    public static void Cpblk<T1, T2>(T1[] source, ref T2 target, int index, int size)
    {
        ref byte src = ref Unsafe.As<T1, byte>(ref source[index]);
        ref byte dst = ref Unsafe.As<T2, byte>(ref target);
        Unsafe.CopyBlockUnaligned(ref dst, ref src, (uint)size);
    }

    public static void Cpblk<T1, T2>(ref T1 source, T2[] target, int index, int size)
    {
        ref byte src = ref Unsafe.As<T1, byte>(ref source);
        ref byte dst = ref Unsafe.As<T2, byte>(ref target[index]);
        Unsafe.CopyBlockUnaligned(ref dst, ref src, (uint)size);
    }

    public static void Cpblk<T1, T2>(T1[] source, T2[] target, int length)
    {
        if (length > source.Length || length > target.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        ref byte src = ref Unsafe.As<T1, byte>(ref source[0]);
        ref byte dst = ref Unsafe.As<T2, byte>(ref target[0]);
        Unsafe.CopyBlockUnaligned(ref dst, ref src, (uint)(Unsafe.SizeOf<T1>() * length));
    }

    public static void Cpblk(IntPtr source, IntPtr target, int size)
    {
        Unsafe.CopyBlockUnaligned((void*)target, (void*)source, (uint)size);
    }

    public unsafe static void Cpblk(void* source, void* target, int size)
    {
        Unsafe.CopyBlockUnaligned(target, source, (uint)size);
    }

    public unsafe static void Cpblk<T>(ref T source, IntPtr target) where T : struct
    {
        Cpblk(ref source, (void*)target);
    }

    public unsafe static void Cpblk<T>(T[] source, IntPtr target, int index, int size)
    {
        Cpblk(source, (void*)target, index, size);
    }

    public unsafe static void Cpblk<T>(IntPtr source, T[] target, int index, int size)
    {
        Cpblk((void*)source, target, index, size);
    }

    public static void Cpblk<T1, T2>(T1[] source, T2 target, int index, int size)
    {
        Cpblk(source, ref target, index, size);
    }

    public static void Cpblk<T1, T2>(T1 source, T2[] target, int index, int size)
    {
        Cpblk(ref source, target, index, size);
    }

    public unsafe static void Cpblk<T>(IntPtr source, ref T target) where T : struct
    {
        Cpblk((void*)source, ref target);
    }

    public static void Cpblk<T1, T2>(T1[] source, T2[] target)
    {
        Cpblk(source, target, source.Length);
    }

    public static void Cpobj<T>(ref T source, ref T target) where T : struct
    {
        target = source;
    }

    public static void Cpobj<T1, T2>(ref T1 source, ref T2 target) where T1 : struct where T2 : struct
    {
        if (Unsafe.SizeOf<T1>() != Unsafe.SizeOf<T2>())
            throw new ArgumentException("Type parameters must be of the same size.", "T1, T2");

        source = Unsafe.As<T2, T1>(ref target);
    }

    public unsafe static void Initblk(IntPtr addr, byte val, uint size)
    {
        Unsafe.InitBlockUnaligned((void*)addr, val, size);
    }

    public static void Initblk<T>(T[] arr, byte val) where T : struct
    {
        ref byte ptr = ref Unsafe.As<T, byte>(ref arr[0]);
        Unsafe.InitBlockUnaligned(ref ptr, val, (uint)(arr.LongLength * Unsafe.SizeOf<T>()));
    }

    public static void Initblk<T>(ref T mref, byte val) where T : struct
    {
        Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref mref), val, (uint)Unsafe.SizeOf<T>());
    }

    public unsafe static void Initblk<T>(T* ptr, byte val, uint size) where T : struct
    {
        Unsafe.InitBlockUnaligned(ptr, val, size);
    }

    public static void Initobj<T>(ref T mref)
    {
        mref = default;
    }

    public unsafe static void Initobj<T>(IntPtr ptr)
    {
        Unsafe.Write((void*)ptr, default(T));
    }

    public unsafe static void Initobj<T>(T* ptr) where T : struct
    {
        Unsafe.Write(ptr, default(T));
    }
}
