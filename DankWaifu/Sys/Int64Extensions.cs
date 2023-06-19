﻿using System;

namespace DankWaifu.Sys
{
    /// <summary>
    /// Represents extension methods for System.Int64 class
    /// </summary>
    public static class Int64Extensions
    {
        ///// <summary>
        ///// Get bit representation of the current value
        ///// </summary>
        ///// <param name="target"></param>
        ///// <returns></returns>
        //public static Bit[] Bits(this Int64 target)
        //{
        //    Int64 flag = 0x01;
        //    var result = new Bit[sizeof(Int64) * 8];
        //    for (var i = 0; i < result.Length; i++)
        //    {
        //        result[i] = (target & flag) > 0 ? Bit.One : Bit.Zero;
        //        flag <<= 1;
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Convert the current value into bytes
        ///// </summary>
        ///// <param name="value">Value to convert</param>
        ///// <param name="order">Byte order in which bytes must be placed</param>
        ///// <returns>Bytes, which represent the current value</returns>
        //public static byte[] GetBytes(this Int64 value, ByteOrder order)
        //{
        //    var result = BitConverter.GetBytes(value);
        //    if (order != RuntimeServices.CurrentByteOrder)
        //        Array.Reverse(result);
        //    return result;
        //}

        /// <summary>
        /// Convert the current value into bytes
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Bytes, which represent the current value</returns>
        public static byte[] GetBytes(this Int64 value)
        {
            var result = BitConverter.GetBytes(value);
            return result;
            //return value.GetBytes(RuntimeServices.CurrentByteOrder);
        }

        /// <summary>
        /// Get byte in the specified position
        /// </summary>
        /// <param name="value">Target value</param>
        /// <param name="index">Byte position</param>
        /// <returns>Byte in the specified position</returns>
        public static byte GetByte(this Int64 value, byte index)
        {
            if (index >= sizeof(Int64))
                throw new ArgumentOutOfRangeException(nameof(index));
            index <<= 3; //index *= 3;
            return (byte)(value >> index);
        }

        /// <summary>
        /// Get lo byte of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Lo byte of the current value</returns>
        public static byte LoByte(this Int64 value)
        {
            return value.GetByte(0);
        }

        /// <summary>
        /// Get hi byte of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Hi byte of the current value</returns>
        public static byte HiByte(this Int64 value)
        {
            return value.GetByte(sizeof(Int64) - 1);
        }

        /// <summary>
        /// Convert the current value into words
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Words, which represent the current value</returns>
        public static short[] GetWords(this Int64 value)
        {
            var result = new short[sizeof(UInt64) / sizeof(short)];
            for (byte i = 0; i < result.Length; i++)
                result[i] = value.GetWord(i);
            return result;
        }

        /// <summary>
        /// Get word in the specified position
        /// </summary>
        /// <param name="value">Target value</param>
        /// <param name="index">Word position</param>
        /// <returns>Word in the specified position</returns>
        public static short GetWord(this Int64 value, byte index)
        {
            const byte wordSize = sizeof(short) * 8; //Word size, in bits
            unchecked
            {
                return (short)(value >> (index * wordSize));
            }
        }

        /// <summary>
        /// Get double word in the specified position
        /// </summary>
        /// <param name="value">Target value</param>
        /// <param name="index">Double word position</param>
        /// <returns>Double word in the specified position</returns>
        public static uint GetDword(this Int64 value, byte index)
        {
            const byte dwordSize = sizeof(uint) * 8; //Double word size, in bits
            unchecked
            {
                return (ushort)(value >> (index * dwordSize));
            }
        }

        /// <summary>
        /// Convert the current value into double words
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Double words, which represent the current value</returns>
        public static uint[] GetDwords(this Int64 value)
        {
            var result = new uint[sizeof(Int64) / sizeof(uint)];
            for (byte i = 0; i < result.Length; i++)
                result[i] = value.GetDword(i);
            return result;
        }

        /// <summary>
        /// Get lo word of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Lo word of the current value</returns>
        public static short LoWord(this Int64 value)
        {
            return value.GetWord(0);
        }

        /// <summary>
        /// Get hi word of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Hi word of the current value</returns>
        public static short HiWord(this Int64 value)
        {
            return value.GetWord(sizeof(Int64) / sizeof(ushort) - 1);
        }

        /// <summary>
        /// Get lo double word of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Lo double word of the current value</returns>
        public static uint LoDword(this Int64 value)
        {
            return value.GetDword(0);
        }

        /// <summary>
        /// Get hi double word of the current value
        /// </summary>
        /// <param name="value">Target value</param>
        /// <returns>Hi double word of the current value</returns>
        public static uint HiDword(this Int64 value)
        {
            return value.GetDword(sizeof(Int64) / sizeof(uint) - 1);
        }
    }
}