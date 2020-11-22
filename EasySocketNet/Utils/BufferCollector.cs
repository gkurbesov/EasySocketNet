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
        /// Добавить данные в буфер
        /// </summary>
        /// <param name="value">массиф данных</param>
        /// <param name="size">количество байт, необходимое для считывания</param>
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
        /// очистить буфер данных
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
