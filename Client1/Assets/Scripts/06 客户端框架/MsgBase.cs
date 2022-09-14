using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class MsgBase
    {
        // 协议名称
        public abstract string protoName { get; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="msgBase">协议</param>
        /// <returns></returns>
        public static byte[] Encode(MsgBase msgBase)
        {
            var str = JsonUtility.ToJson(msgBase);
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="protoName">协议名称</param>
        /// <param name="bytes">协议数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">长度</param>
        /// <returns></returns>
        public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
        {
            var str = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            return (MsgBase) JsonUtility.FromJson(str, Type.GetType($"Framework.{protoName},Assembly-CSharp"));
        }
        
        /// <summary>
        /// 编码协议名(2字节长度+字符串)
        /// </summary>
        /// <param name="msgBase">协议</param>
        /// <returns></returns>
        public static byte[] EncodeName(MsgBase msgBase)
        {
            // 名字bytes和长度
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
            var len = (Int16) nameBytes.Length;
            // 申请bytes数值
            var bytes = new byte[2 + len];
            // 组装2字节的长度信息（小端）
            bytes[0] = (byte) (len % 256);
            bytes[1] = (byte) (len / 256);
            // 组装名字bytes
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }

        /// <summary>
        /// 解码协议名(2字节长度+字符串)
        /// </summary>
        /// <param name="bytes">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">长度</param>
        /// <returns></returns>
        public static string DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            // 必须大于2字节
            if (offset + 2 > bytes.Length)
                return "";
            // 读取长度
            Int16 len = (Int16) ((bytes[offset + 1] << 8) | bytes[offset]);
            // 长度必须足够
            if (offset + 2 + len > bytes.Length)
                return "";
            // 解析
            count = 2 + len;
            return System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        }
    }
}