using System;
using System.IO;

namespace MEMOEDIT
{
    class MemoFlag
    {
        public MemoFlag(string FileName)
        {
            LoadData(FileName);
        }

        UInt16[] _Flags;

        void LoadData(string FileName)
        {
            byte[] FileData = File.ReadAllBytes(FileName);

            _Flags = new UInt16[FileData.Length >> 1];

            for (int n = 0; n < _Flags.Length; n++)
            {
                _Flags[n] = (UInt16)((FileData[(n * 2) + 1] << 8) | FileData[n * 2]);
            }
        }

        public UInt16[] GetFlags(int PageIndex)
        {
            UInt16[] Flags = new UInt16[10];
            Array.Copy(_Flags, PageIndex * 5, Flags, 0, 10);

            return Flags;
        }

        public void SetFlags(UInt16[] Flags, int PageIndex)
        {
            Array.Copy(Flags, 0, _Flags, PageIndex * 5, 10);
        }

        public void SaveData(string FileName)
        {
            byte[] FileData = new byte[_Flags.Length << 1];

            for (int n = 0; n < _Flags.Length; n++)
            {
                FileData[n * 2] = (byte)(_Flags[n] & 0xff);
                FileData[(n * 2) + 1] = (byte)(_Flags[n] >> 8);
            }

            File.WriteAllBytes(FileName, FileData);
        }
    }
}
