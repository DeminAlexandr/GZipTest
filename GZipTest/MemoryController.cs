using System;
using Microsoft.VisualBasic.Devices;

namespace GZipArchiver
{
    public static class MemoryController
    {
        public const int ArchiverBlockSize = 1048576; // 1 MB
        public const double AvailableMemoryLimitPercents = 0.2;

        public static bool CanTakeMemory()
        {
            var info = new ComputerInfo();
            return ((1.0 - (double)info.AvailablePhysicalMemory / info.TotalPhysicalMemory) > AvailableMemoryLimitPercents);
        }

        public static int GetWorkerThreadCount()
        {
            return Environment.ProcessorCount * 20;
        }
    }
}
