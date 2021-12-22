using System;
using System.Collections.Generic;
using System.Threading;

namespace PIDFinder
{
    public static class RandUtil
    {
        // Multithread safe rand, ha
        public static Random Rand => _local.Value;

        private static readonly ThreadLocal<Random> _local = new(() => new Random());

        public static uint Rand32() => Rand32(Rand);
        public static uint Rand32(Random rnd) => (uint)rnd.Next(1 << 30) << 2 | (uint)rnd.Next(1 << 2);
    }
}
