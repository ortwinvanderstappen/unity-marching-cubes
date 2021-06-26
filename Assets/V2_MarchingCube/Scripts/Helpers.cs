using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V2
{
    class Helper
    {
        public static int GetAppendBufferSize(ComputeBuffer appendBuffer)
        {
            // Copy the count
            ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);

            // Store count in array
            int[] countArr = { 0 };
            countBuffer.GetData(countArr);

            // Release the buffer
            countBuffer.Release();

            // Return the count (in first index)
            return countArr[0];
        }
    }
}
