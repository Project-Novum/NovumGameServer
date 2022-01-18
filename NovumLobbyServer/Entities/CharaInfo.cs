using Common.Attributes;
using Database.Models;

namespace NovumLobbyServer.Entities;

public class CharaInfo
{
    public struct FaceInfo
    {
        [BitfieldLength(5)]
        public uint characteristics;
        [BitfieldLength(3)]
        public uint characteristicsColor;
        [BitfieldLength(6)]
        public uint type;
        [BitfieldLength(2)]
        public uint ears;
        [BitfieldLength(2)]
        public uint mouth;
        [BitfieldLength(2)]
        public uint features;
        [BitfieldLength(3)]
        public uint nose;
        [BitfieldLength(3)]
        public uint eyeShape;
        [BitfieldLength(1)]
        public uint irisSize;
        [BitfieldLength(3)]
        public uint eyebrows;
        [BitfieldLength(2)]
        public uint unknown;
    }
    
    public static String BuildForCharaList(Character chara, Appearance appearance)
        {
            byte[] data;
            
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    //Build faceinfo for later
                    FaceInfo faceInfo = new FaceInfo();
                    faceInfo.characteristics = appearance.Characteristics;
                    faceInfo.characteristicsColor = appearance.CharacteristicsColor;
                    faceInfo.type = appearance.FaceType;
                    faceInfo.ears = appearance.Ears;
                    faceInfo.features = appearance.FaceFeatures;
                    faceInfo.eyebrows = appearance.FaceEyebrows;
                    faceInfo.eyeShape = appearance.FaceEyeShape;
                    faceInfo.irisSize = appearance.FaceIrisSize;
                    faceInfo.mouth = appearance.FaceMouth;
                    faceInfo.nose = appearance.FaceNose;

                    string location1 = "prv0Inn01\0";
                    string location2 = "defaultTerritory\0";

                    writer.Write((UInt32)0x000004c0);
                    writer.Write((UInt32)0x232327ea);
                    writer.Write((UInt32)System.Text.Encoding.UTF8.GetBytes(chara.Name + '\0').Length);
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(chara.Name + '\0'));
                    writer.Write((UInt32)0x1c);
                    writer.Write((UInt32)0x04);
                    writer.Write((UInt32)GetTribeModel(chara.Tribe));
                    writer.Write((UInt32)appearance.Size);
                    uint colorVal = (ushort)appearance.SkinColor | (uint)(appearance.HairColor << 10) | (uint)(appearance.EyeColor << 20);
                    writer.Write((UInt32)colorVal);

                    var bitfield = PrimitiveConversion.ToUInt32(faceInfo);

                    writer.Write((UInt32)bitfield); //FACE, Figure this out!
                    uint hairVal = (ushort)appearance.HairHighlightColor | (uint)(appearance.HairVariation << 5) | (uint)(appearance.HairStyle << 10);
                    writer.Write((UInt32)hairVal);
                    writer.Write((UInt32)appearance.Voice);
                    writer.Write((UInt32)appearance.MainHand);
                    writer.Write((UInt32)appearance.OffHand);

                    writer.Write((UInt32)0);
                    writer.Write((UInt32)0);
                    writer.Write((UInt32)0);
                    writer.Write((UInt32)0);
                    writer.Write((UInt32)0);

                    writer.Write((UInt32)appearance.Head);
                    writer.Write((UInt32)appearance.Body);
                    writer.Write((UInt32)appearance.Legs);
                    writer.Write((UInt32)appearance.Hands);
                    writer.Write((UInt32)appearance.Feet);
                    writer.Write((UInt32)appearance.Waist);

                    writer.Write((UInt32)appearance.Neck);
                    writer.Write((UInt32)appearance.RightEar);
                    writer.Write((UInt32)appearance.LeftEar);
                    writer.Write((UInt32)appearance.RightIndex);
                    writer.Write((UInt32)appearance.LeftIndex);
                    writer.Write((UInt32)appearance.RightFinger);
                    writer.Write((UInt32)appearance.LeftFinger);

                    for (int i = 0; i < 0x8; i++)
                        writer.Write((byte)0);

                    writer.Write((UInt32)1);
                    writer.Write((UInt32)1);

                    writer.Write((byte)chara.CurrentClass);
                    writer.Write((UInt16)chara.CurrentLevel);
                    writer.Write((byte)chara.CurrentJob);
                    writer.Write((UInt16)1);
                    writer.Write((byte)chara.Tribe);

                    writer.Write((UInt32)0xe22222aa);

                    writer.Write((UInt32)System.Text.Encoding.UTF8.GetBytes(location1).Length);
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(location1));
                    writer.Write((UInt32)System.Text.Encoding.UTF8.GetBytes(location2).Length);
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(location2));

                    writer.Write((byte)chara.Guardian);
                    writer.Write((byte)chara.BirthMonth);
                    writer.Write((byte)chara.BirthDay);

                    writer.Write((UInt16)0x17);
                    writer.Write((UInt32)4);
                    writer.Write((UInt32)4);

                    writer.BaseStream.Seek(0x10, SeekOrigin.Current);

                    writer.Write((UInt32)chara.InitialTown);
                    writer.Write((UInt32)chara.InitialTown);
                }

                data = stream.GetBuffer();
            }

            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
        }
    
    public static UInt32 GetTribeModel(byte tribe)
    {
        switch (tribe)
        {
            //Hyur Midlander Male
            case 1:
            default:
                return 1;

            //Hyur Midlander Female
            case 2:
                return 2;

            //Elezen Male
            case 4:
            case 6:
                return 3;

            //Elezen Female
            case 5:
            case 7:
                return 4;

            //Lalafell Male
            case 8:
            case 10:
                return 5;

            //Lalafell Female
            case 9:
            case 11:
                return 6;

            //Miqo'te Female
            case 12:
            case 13:
                return 8;

            //Roegadyn Male
            case 14:
            case 15:
                return 7;

            //Hyur Highlander Male
            case 3:
                return 9;
        }
    }
}