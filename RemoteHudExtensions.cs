using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GLV.Shared.Common;

namespace DiegoG.RemoteHud;

public static class RemoteHudExtensions
{
    extension (Type type){
        public long NetworkCode
        {
            get
            {
                Span<byte> buffer = stackalloc byte[SHA256.HashSizeInBytes];
                type.FullName!.TryHashToSHA256(buffer);
                Span<long> longs = MemoryMarshal.Cast<byte, long>(buffer);
                return longs[0] ^ longs[1] ^ longs[2] ^ longs[3];
            }
        }
    }
}
