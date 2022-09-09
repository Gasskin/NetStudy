using System;

namespace Framework
{
    public class ByteArray
    {
    #region 字段

        //默认大小
        private const int DEFAULT_SIZE = 1024;

        // 初始大小
        private int initSize = 0;

    #endregion

    #region 属性

        // 缓冲区
        public byte[] Bytes { get; private set; }

        // 读写位置
        public int ReadIndex { get; private set; }

        public int WriteIndex { get; private set; }

        // 容量
        public int Capacity { get; private set; }

        // 剩余空间
        public int Remain => Capacity - WriteIndex;

        // 数据长度
        public int Length => WriteIndex - ReadIndex;

    #endregion

    #region 构造

        // 构造函数1
        public ByteArray(int size = DEFAULT_SIZE)
        {
            Bytes = new byte[size];
            Capacity = size;
            initSize = size;
            ReadIndex = 0;
            WriteIndex = 0;
        }

        // 构造函数2
        public ByteArray(byte[] defaultBytes)
        {
            Bytes = defaultBytes;
            Capacity = defaultBytes.Length;
            initSize = defaultBytes.Length;
            ReadIndex = 0;
            WriteIndex = defaultBytes.Length;
        }

        // 重设尺寸
        private void ReSize(int size)
        {
            if (size < Length)
                return;
            if (size < initSize)
                return;
            int n = 1;
            while (n < size) n *= 2;
            Capacity = n;
            byte[] newBytes = new byte[Capacity];
            Array.Copy(Bytes, ReadIndex, newBytes, 0, WriteIndex - ReadIndex);
            Bytes = newBytes;
            WriteIndex = Length;
            ReadIndex = 0;
        }

    #endregion

    #region 读写

        // 写入数据
        public int Write(byte[] bs, int offset, int count)
        {
            if (Remain < count)
            {
                ReSize(Length + count);
            }

            Array.Copy(bs, offset, Bytes, WriteIndex, count);
            WriteIndex += count;
            return count;
        }

        // 读取数据
        public int Read(byte[] bs, int offset, int count)
        {
            count = Math.Min(count, Length);
            Array.Copy(Bytes, 0, bs, offset, count);
            ReadIndex += count;
            CheckAndMoveBytes();
            return count;
        }

        // 检查并移动数据
        private void CheckAndMoveBytes()
        {
            if (Length < 8)
            {
                MoveBytes();
            }
        }

        // 移动数据
        private void MoveBytes()
        {
            Array.Copy(Bytes, ReadIndex, Bytes, 0, Length);
            WriteIndex = Length;
            ReadIndex = 0;
        }

        // 读取Int16,16位=2字节
        public Int16 ReadInt16()
        {
            if (Length < 2)
                return 0;
            Int16 ret = (Int16) ((Bytes[1] << 8) | Bytes[0]);
            ReadIndex += 2;
            CheckAndMoveBytes();
            return ret;
        }

        // 读取Int32
        public Int32 ReadInt32()
        {
            if (Length < 4) return 0;
            Int32 ret = (Int32) ((Bytes[3] << 24) |
                                 (Bytes[2] << 16) |
                                 (Bytes[1] << 8) |
                                 Bytes[0]);
            ReadIndex += 4;
            CheckAndMoveBytes();
            return ret;
        }

    #endregion
    }
}