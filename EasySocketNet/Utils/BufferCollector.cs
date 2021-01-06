using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasySocketNet.Utils
{
    public class BufferCollector
    {
        public byte[] Data { get; private set; } = new byte[0];
        private object locker = new object();

        /// <summary>
        /// Add data to buffer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        public void Append(byte[] value, int size)
        {
            lock(locker)
            {
                var tmp = new byte[Data.Length + size];
                for (int i = 0; i < Data.Length; i++)
                    tmp[i] = Data[i];
                for (int i = 0; i < size; i++)
                    tmp[i+Data.Length] = value[i];
                Data = tmp;
            }
        }
        /// <summary>
        /// Clear data buffer
        /// </summary>
        public void Clear()
        {
            lock (locker)
            {
                Data = new byte[0];
            }
        }
    }
}
