﻿using PlcCommon.Logs;
using System;

namespace PlcCommon.S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Real from S7 plc to C# float.
    /// </summary>
    public static class Single
    {
        /// <summary>
        /// Converts a S7 Real (4 bytes) to float
        /// </summary>
        public static float FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                Logger.E($"Wrong number of bytes. Bytes array must contain 4 bytes.");
                //throw new ArgumentException("Wrong number of bytes. Bytes array must contain 4 bytes.");
            }

            // sps uses bigending so we have to reverse if platform needs
            if (BitConverter.IsLittleEndian)
            {
                // create deep copy of the array and reverse
                bytes = new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] };
            }

            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Converts a S7 DInt to float
        /// </summary>
        public static float FromDWord(Int32 value)
        {
            byte[] b = DInt.ToByteArray(value);
            float d = FromByteArray(b);
            return d;
        }

        /// <summary>
        /// Converts a S7 DWord to float
        /// </summary>
        public static float FromDWord(UInt32 value)
        {
            byte[] b = DWord.ToByteArray(value);
            float d = FromByteArray(b);
            return d;
        }


        /// <summary>
        /// Converts a double to S7 Real (4 bytes)
        /// </summary>
        public static byte[] ToByteArray(float value)
        {
            byte[] bytes = BitConverter.GetBytes((float)(value));

            // sps uses bigending so we have to check if platform is same
            if (!BitConverter.IsLittleEndian) return bytes;
            
            // create deep copy of the array and reverse
            return new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] };
        }

        /// <summary>
        /// Converts an array of float to an array of bytes 
        /// </summary>
        public static byte[] ToByteArray(float[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (float val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of S7 Real to an array of float
        /// </summary>
        public static float[] ToArray(byte[] bytes)
        {
            float[] values = new float[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

            return values;
        }
        
    }
}