using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MEMOEDIT
{
    class MemoData
    {
        public MemoData(string FileName, string TableFile)
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

        byte[] FileData;

        UInt32[] MemoPointers;
        UInt32[] RubiPointers;
        List<string> MemoStrings;
        List<string> RubiStrings;

        long RubiOffset;
        long MstrOffset;

        public int PageCount
        {
            get { return MemoPointers.Length / 5; }
        }

        //long MstrOffset;

        void LoadData(string FileName)
        {
            FileData = File.ReadAllBytes(FileName);

            using (MemoryStream ms = new MemoryStream(FileData))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    for (int b = 0; b < 3; b++) //Read each one of the three blocks in any order they could be
                    {
                        UInt32 BlockHeader = br.ReadUInt32();
                        UInt32 BlockLength;

                        switch (BlockHeader)
                        {
                            case 0x4f4d454d: //MEMO

                                BlockLength = br.ReadUInt32();

                                MemoPointers = new UInt32[(BlockLength - 12) >> 2];
                                for (int n = 0; n < MemoPointers.Length; n++)
                                {
                                    MemoPointers[n] = br.ReadUInt32();
                                }

                                ms.Position += 4; //Skip the last 4 zeroes in the block

                                break;

                            case 0x49425552: //RUBI

                                RubiOffset = ms.Position - 4;

                                BlockLength = br.ReadUInt32();

                                RubiPointers = new UInt32[(BlockLength - 12) >> 2];
                                for (int n = 0; n < RubiPointers.Length; n++)
                                {
                                    RubiPointers[n] = br.ReadUInt32();
                                }

                                ms.Position += 4; //Skip the last 4 zeroes in the block

                                break;

                            case 0x5254534d: //MSTR

                                MstrOffset = ms.Position - 4;

                                //Read MemoStrings
                                MemoStrings = new List<string>();

                                for (int n = 0; n < MemoPointers.Length; n++)
                                {
                                    if (MemoPointers[n] == 0xffffffff)
                                    {
                                        MemoStrings.Add("");
                                    }
                                    else
                                    {
                                        MemoStrings.Add(ReadText(ms, MstrOffset + 8 + MemoPointers[n]));
                                    }
                                }

                                //Read RubiStrings
                                RubiStrings = new List<string>();

                                for (int n = 0; n < RubiPointers.Length; n++)
                                {
                                    if (RubiPointers[n] == 0xffffffff)
                                    {
                                        RubiStrings.Add("");
                                    }
                                    else
                                    {
                                        RubiStrings.Add(ReadText(ms, MstrOffset + 8 + RubiPointers[n]));
                                    }
                                }

                                break;
                        }
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
                    //Reset Memo pointers
                    for (int n = 0; n < MemoPointers.Length; n++)
                    {
                        MemoPointers[n] = 0xfffffffe;
                    }

                    uint CurrentPosition = 0;

                    //Regenerate Memo pointers and write texts to MSTR block
                    for (int n = 0; n < MemoPointers.Length; n++)
                    {
                        if (MemoStrings[n] == "")
                        {
                            MemoPointers[n] = 0xffffffff;
                        }
                        else
                        {
                            if (MemoPointers[n] == 0xfffffffe)
                            {
                                byte[] TextArray = Table.StringToBin(MemoStrings[n]);

                                MemoPointers[n] = CurrentPosition;

                                //Find duplicates
                                for (int d = n + 1; d < MemoPointers.Length; d++)
                                {
                                    if (MemoStrings[n] == MemoStrings[d]) MemoPointers[d] = CurrentPosition;
                                }

                                //Write text to the stream
                                ms.Position = MstrOffset + 8 + CurrentPosition;
                                ms.Write(TextArray, 0, TextArray.Length);

                                CurrentPosition += (uint)(TextArray.Length + 1);
                            }
                        }
                    }

                    //Reset Rubi pointers
                    for (int n = 0; n < RubiPointers.Length; n++)
                    {
                        RubiPointers[n] = 0xfffffffe;
                    }

                    //Regenerate Rubi pointers and write texts to MSTR block
                    for (int n = 0; n < RubiPointers.Length; n++)
                    {
                        if (RubiStrings[n] == "")
                        {
                            RubiPointers[n] = 0xffffffff;
                        }
                        else
                        {
                            if (RubiPointers[n] == 0xfffffffe)
                            {
                                byte[] TextArray = Table.StringToBin(RubiStrings[n]);

                                RubiPointers[n] = CurrentPosition;

                                //Find duplicates
                                for (int d = n + 1; d < RubiPointers.Length; d++)
                                {
                                    if (RubiStrings[n] == RubiStrings[d]) RubiPointers[d] = CurrentPosition;
                                }

                                //Write text to the stream
                                ms.Position = MstrOffset + 8 + CurrentPosition;
                                ms.Write(TextArray, 0, TextArray.Length);

                                CurrentPosition += (uint)(TextArray.Length + 1);
                            }
                        }
                    }

                    //Write final zeroes of the file
                    ms.Position = MstrOffset + 8 + CurrentPosition;
                    uint Padding = 4 - (CurrentPosition % 4);
                    if (Padding == 4) Padding = 0;
                    for (int n = 0; n < Padding; n++) bw.Write((byte)0);
                    bw.Write((uint)0);
                    bw.Write((uint)0);

                    //Write MEMO block header
                    ms.Position = 0;

                    bw.Write((uint)0x4f4d454d);
                    bw.Write((MemoPointers.Length << 2) + 12);

                    //Write MemoPointers
                    for (int n = 0; n < MemoPointers.Length; n++)
                    {
                        bw.Write(MemoPointers[n]);
                    }

                    //Write RUBI block
                    ms.Position = RubiOffset;

                    bw.Write((uint)0x49425552);
                    bw.Write((RubiPointers.Length << 2) + 12);

                    for (int n = 0; n < RubiPointers.Length; n++)
                    {
                        bw.Write(RubiPointers[n]);
                    }

                    //Write MSTR block header
                    ms.Position = MstrOffset;

                    bw.Write((uint)0x5254534d);
                    bw.Write(CurrentPosition + 8 + 1);


                    //Write stream to file
                    File.WriteAllBytes(FileName, ms.ToArray());
                }
            }
        }

        public string[] GetPageText(int PageIndex)
        {
            string[] PageText = new string[10];

            PageText[0] = MemoStrings[PageIndex * 5];
            PageText[1] = MemoStrings[(PageIndex * 5) + 1];
            PageText[2] = MemoStrings[(PageIndex * 5) + 2];
            PageText[3] = MemoStrings[(PageIndex * 5) + 3];
            PageText[4] = MemoStrings[(PageIndex * 5) + 4];
            PageText[5] = MemoStrings[(PageIndex * 5) + 5];
            PageText[6] = MemoStrings[(PageIndex * 5) + 6];
            PageText[7] = MemoStrings[(PageIndex * 5) + 7];
            PageText[8] = MemoStrings[(PageIndex * 5) + 8];
            PageText[9] = MemoStrings[(PageIndex * 5) + 9];

            return PageText;
        }

        public void SetPageText(string[] Texts, int PageIndex)
        {
            MemoStrings[PageIndex * 5] = Texts[0];
            MemoStrings[(PageIndex * 5) + 1] = Texts[1];
            MemoStrings[(PageIndex * 5) + 2] = Texts[2];
            MemoStrings[(PageIndex * 5) + 3] = Texts[3];
            MemoStrings[(PageIndex * 5) + 4] = Texts[4];
            MemoStrings[(PageIndex * 5) + 5] = Texts[5];
            MemoStrings[(PageIndex * 5) + 6] = Texts[6];
            MemoStrings[(PageIndex * 5) + 7] = Texts[7];
            MemoStrings[(PageIndex * 5) + 8] = Texts[8];
            MemoStrings[(PageIndex * 5) + 9] = Texts[9];
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
