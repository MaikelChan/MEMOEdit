using System;
using System.Collections.Generic;
using System.IO;

namespace MEMOEDIT
{
    class Bunki
    {
        public Bunki(string FileName, string TableFile)
        {
            //Load table
            if (!Table.TableLoaded)
            {
                if (File.Exists(TableFile))
                {
                    if (!Table.LoadTable(TableFile))
                    {
                        throw new Exception("There was an error while loading the table file.");
                    }
                }
                else
                    throw new Exception("Unable to find \"table.txt\" file.");
            }

            LoadData(FileName);
        }

        public int TextCount
        {
            get;
            set;
        }

        byte[] HeaderBytes;

        uint[] Flag1;
        uint[] Flag2;
        uint[] Pointer1;
        uint[] Pointer2;

        string[] Texts;

        void LoadData(string FileName)
        {
            byte[] FileData = File.ReadAllBytes(FileName);

            HeaderBytes = new byte[0xc];
            Array.Copy(FileData, HeaderBytes, 0xc);

            using (MemoryStream ms = new MemoryStream(FileData))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    uint PointersOffset;

                    //Read pointers offset
                    br.BaseStream.Position = 0xc;
                    PointersOffset = br.ReadUInt32();

                    //Initialize variables
                    TextCount = (int)(br.BaseStream.Length - PointersOffset) >> 4;

                    Flag1 = new uint[TextCount];
                    Flag2 = new uint[TextCount];
                    Pointer1 = new uint[TextCount];
                    Pointer2 = new uint[TextCount];

                    Texts = new string[TextCount << 1];

                    //Start reading and storing pointers
                    br.BaseStream.Position = PointersOffset;

                    for (int n = 0; n < TextCount; n++)
                    {
                        Flag1[n] = br.ReadUInt32();
                        Flag2[n] = br.ReadUInt32();
                        Pointer1[n] = br.ReadUInt32();
                        Pointer2[n] = br.ReadUInt32();
                    }

                    //Start reading texts
                    for (int n = 0; n < TextCount; n++)
                    {
                        if (Pointer1[n] == 0)
                            Texts[2 * n] = String.Empty;
                        else
                            Texts[2 * n] = ReadText(br.BaseStream, Pointer1[n]);

                        if (Pointer2[n] == 0)
                            Texts[(2 * n) + 1] = String.Empty;
                        else
                            Texts[(2 * n) + 1] = ReadText(br.BaseStream, Pointer2[n]);
                    }
                }
            }
        }

        public void SaveData(string FileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.BaseStream.Write(HeaderBytes, 0, HeaderBytes.Length);

                    bw.BaseStream.Position = 0x10;

                    //Start writing texts
                    for (int n = 0; n < Texts.Length; n++)
                    {
                        bool Empty = Texts[n] == String.Empty;

                        if ((n & 1) == 0)
                        {
                            if (Empty)
                                Pointer1[n >> 1] = 0;
                            else
                                Pointer1[n >> 1] = (uint)bw.BaseStream.Position;
                        }
                        else
                        {
                            if (Empty)
                                Pointer2[n >> 1] = 0;
                            else
                                Pointer2[(n - 1) >> 1] = (uint)bw.BaseStream.Position;
                        }

                        if (!Empty)
                        {
                            byte[] TextArray = Table.StringToBin(Texts[n]);
                            bw.BaseStream.Write(TextArray, 0, TextArray.Length);

                            //Padding
                            bw.BaseStream.Position++;
                            long Padding = 4 - (bw.BaseStream.Position % 4);
                            bw.BaseStream.Position += Padding == 4 ? 0 : Padding;
                        }
                    }

                    uint PointersOffset = (uint)bw.BaseStream.Position;

                    for (int n = 0; n < TextCount; n++)
                    {
                        bw.Write(Flag1[n]);
                        bw.Write(Flag2[n]);
                        bw.Write(Pointer1[n]);
                        bw.Write(Pointer2[n]);
                    }

                    bw.BaseStream.Position = 0xc;

                    bw.Write(PointersOffset);
                }
                File.WriteAllBytes(FileName, ms.ToArray());
            }
        }

        public string[] GetTexts()
        {
            return Texts;
        }

        public void SetTexts(string[] BunkiTexts)
        {
            Array.Copy(BunkiTexts, Texts, Texts.Length);
        }

        string ReadText(Stream s, long Offset)
        {
            List<byte> Temp = new List<byte>();

            s.Position = Offset;

            for (; ; )
            {
                byte TempByte = (byte)s.ReadByte();

                if (TempByte == 0)
                {
                    if (Temp.Count > 0)
                    {
                        return Table.BinToString(Temp.ToArray());
                    }
                }
                else
                {
                    Temp.Add(TempByte);
                }
            }
        }
    }
}
