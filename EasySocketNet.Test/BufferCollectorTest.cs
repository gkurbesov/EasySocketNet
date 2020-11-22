using EasySocketNet.Utils;
using System;
using Xunit;

namespace EasySocketNet.Test
{
    public class BufferCollectorTest
    {
        [Fact]
        public void Test1()
        {
            var collecor = new BufferCollector();
            var arr1 = new byte[] { 1, 2, 3, 4 };
            var arr2 = new byte[] { 5, 6, 7, 8, 9 };

            collecor.Append(arr1, arr1.Length);
            collecor.Append(arr2, arr2.Length);

            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, collecor.Data);
        }

        [Fact]
        public void Test2()
        {
            var collecor = new BufferCollector();
            var arr1 = new byte[] { 1, 2, 3, 4 };

            collecor.Append(arr1, arr1.Length);
            collecor.Clear();

            Assert.Empty(collecor.Data);
        }
    }
}
