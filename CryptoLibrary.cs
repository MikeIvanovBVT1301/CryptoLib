using System;
using System.IO;
using System.Text;

namespace CryptoLibrary
{
    public interface ICryptoAlgorithm
    {
        void Encrypt(Stream inp, out Stream outp);
        void Decrypt(Stream inp, out Stream outp);
    }

    public class DES : ICryptoAlgorithm
    {
        string encryptKey;
        string decryptKey;
        string data;

        string[] Blocks;
        private const int blockSize = 64;

        private const int charSize = 16;
        private const int keyShift = 2;
        private const int roundNumber = 16;

        public DES(Stream inpKey)
        {
            StreamReader reader = new StreamReader(inpKey);
            encryptKey = reader.ReadToEnd();
        }

        private string CompleteLength(string data)
        {
            while (((data.Length * charSize) % blockSize) != 0)
                data = "0" + data;

            return data;
        }

        private void SplitBinaryIntoBlocks(string data)
        {
            Blocks = new string[data.Length / blockSize];

            int lengthOfBlock = data.Length / Blocks.Length;

            for (int i = 0; i < Blocks.Length; i++)
                Blocks[i] = data.Substring(i * lengthOfBlock, lengthOfBlock);
        }

        private void SplitStringIntoBlocks(string data)
        {
            Blocks = new string[(data.Length * charSize) / blockSize];

            int lengthOfBlock = data.Length / Blocks.Length;

            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i] = data.Substring(i * lengthOfBlock, lengthOfBlock);
                Blocks[i] = StringToBinary(Blocks[i]);
            }
        }

        private string CompleteKeyLength(string key, int keyLength)
        {
            if (key.Length > keyLength)
                key = key.Substring(0, keyLength);
            else
                while (key.Length < keyLength)
                    key = "0" + key;

            return key;
        }

        private string EncodeOneRound(string data, string key)
        {
            string L = data.Substring(0, data.Length / 2);
            string R = data.Substring(data.Length / 2, data.Length / 2);

            return (R + XOR(L, AND(R, key)));
        }

        private string DecodeOneRound(string data, string key)
        {
            string L = data.Substring(0, data.Length / 2);
            string R = data.Substring(data.Length / 2, data.Length / 2);

            return (XOR(AND(L, key), R) + L);
        }

        private string AND(string s1, string s2)
        {
            string result = "";

            for (int i = 0; i < s1.Length; i++)
            {
                bool a = Convert.ToBoolean(Convert.ToInt32(s1[i].ToString()));
                bool b = Convert.ToBoolean(Convert.ToInt32(s2[i].ToString()));

                if (a & b)
                    result += "1";
                else
                    result += "0";
            }
            return result;
        }

        private string XOR(string s1, string s2)
        {
            string result = "";

            for (int i = 0; i < s1.Length; i++)
            {
                bool a = Convert.ToBoolean(Convert.ToInt32(s1[i].ToString()));
                bool b = Convert.ToBoolean(Convert.ToInt32(s2[i].ToString()));

                if (a ^ b)
                    result += "1";
                else
                    result += "0";
            }
            return result;
        }

        private string NextKey(string key)
        {
            for (int i = 0; i < keyShift; i++)
            {
                key = key[key.Length - 1] + key;
                key = key.Remove(key.Length - 1);
            }

            return key;
        }

        private string PrevKey(string key)
        {
            for (int i = 0; i < keyShift; i++)
            {
                key = key + key[0];
                key = key.Remove(0, 1);
            }

            return key;
        }

        private string StringToBinary(string data)
        {
            string result = "";

            for (int i = 0; i < data.Length; i++)
            {
                string binaryChar = Convert.ToString(data[i], 2);

                while (binaryChar.Length < charSize)
                    binaryChar = "0" + binaryChar;

                result += binaryChar;
            }

            return result;
        }

        private string BinaryToString(string data)
        {
            string result = "";

            while (data.Length > 0)
            {
                string char_binary = data.Substring(0, charSize);
                data = data.Remove(0, charSize);

                int a = 0;
                int degree = char_binary.Length - 1;

                foreach (char c in char_binary)
                    a += Convert.ToInt32(c.ToString()) * (int)Math.Pow(2, degree--);

                result += ((char)a).ToString();
            }

            return result;
        }

        public void Encrypt(Stream inp, out Stream outp)
        {
            StreamReader reader = new StreamReader(inp);
            data = reader.ReadToEnd();

            data = CompleteLength(data);
            SplitStringIntoBlocks(data);
            encryptKey = CompleteKeyLength(encryptKey, data.Length / (2 * Blocks.Length));
            encryptKey = StringToBinary(encryptKey);

            for (int j = 0; j < roundNumber; j++)
            {
                for (int i = 0; i < Blocks.Length; i++)
                    Blocks[i] = EncodeOneRound(Blocks[i], encryptKey);

                encryptKey = NextKey(encryptKey);
            }

            encryptKey = PrevKey(encryptKey);

            decryptKey = BinaryToString(encryptKey);

            String result = "";

            for (int i = 0; i < Blocks.Length; i++)
                result += Blocks[i];

            result = BinaryToString(result);

            byte[] byteArray = Encoding.ASCII.GetBytes(result);
            outp = new MemoryStream(byteArray);

        }

        public void Decrypt(Stream inp, out Stream outp)
        {
            StreamReader reader = new StreamReader(inp);
            data = reader.ReadToEnd();

            decryptKey = StringToBinary(decryptKey);
            data = StringToBinary(data);

            SplitBinaryIntoBlocks(data);

            for (int j = 0; j < roundNumber; j++)
            {
                for (int i = 0; i < Blocks.Length; i++)
                    Blocks[i] = DecodeOneRound(Blocks[i], decryptKey);

                decryptKey = PrevKey(decryptKey);
            }

            decryptKey = NextKey(decryptKey);
            string result = "";

            for (int i = 0; i < Blocks.Length; i++)
                result += Blocks[i];
            result = BinaryToString(result);
            result = result.TrimStart('0');

            byte[] byteArray = Encoding.ASCII.GetBytes(result);
            outp = new MemoryStream(byteArray);

        }

    }


    //
    public interface ElGamalInterface
    {
        void EncryptStream(Stream input, Stream output);
        void DecryptStream(Stream input, Stream output);
    }




    public class CypherElGamal : ElGamalInterface
    {
        public void EncryptStream(Stream input, Stream output)
        {
            ElGamal elGamal = new ElGamal();
            elGamal.EncryptStream(input, output);
        }




        public void DecryptStream(Stream input, Stream output)
        {
            ElGamal elGamal = new ElGamal();
            elGamal.DecryptStream(input, output);
        }
    }



    public class ElGamal
    {
        static int p = 157;
        static int x = 10;
        static int g = 3;
        static int y;

        int Pow(int a, int b, int m)
        {
            int tmp = a;
            int sum = tmp;
            for (int i = 1; i < b; i++)
            {
                for (int j = 1; j < a; j++)
                {
                    sum += tmp;
                    if (sum >= m)
                    {
                        sum -= m;
                    }
                }
                tmp = sum;
            }
            return tmp;
        }

        int Mul(int a, int b, int m)
        {
            int sum = 0;
            for (int i = 0; i < b; i++)
            {
                sum += a;
                if (sum >= m)
                {
                    sum -= m;
                }
            }
            return sum;
        }




        public void EncryptStream(Stream input, Stream output)
        {
            Random random = new Random();
            byte byteToCrypt;

            y = Pow(g, x, p);

            input.Position = 0;
            for (; input.Position < input.Length;)
            {
                byteToCrypt = (byte)input.ReadByte();

                int k = random.Next() % (p - 2) + 1;
                int left = Pow(g, k, p);
                int right = Mul(Pow(y, k, p), byteToCrypt, p);

                output.WriteByte((byte)left);
                output.WriteByte((byte)right);
            }
        }

        public void DecryptStream(Stream input, Stream output)
        {
            byte left, right;
            byte[] bytesToDecrypt = new byte[2];

            input.Position = 0;
            for (; input.Position < input.Length;)
            {
                input.Read(bytesToDecrypt, 0, 2);

                left = bytesToDecrypt[0];
                right = bytesToDecrypt[1];

                int m = Mul(right, Pow(left, p - 1 - x, p), p);
                output.WriteByte((byte)m);
            }
        }
    }

   
    
    //

    public interface MD5Interface
    {
        void HashStream(Stream input, Stream output);
    }

    public class CypherMD5 : MD5Interface
    {
        public void HashStream(Stream input, Stream output)
        {
            MD5 md5 = new MD5();
            md5.HashStream(input, output);
        }
    }

    public static class Md5Extensions
    {
        public static uint RotateLeft(this uint val, int count)
        {
            return (val << count) | (val >> (32 - count));
        }

        public static uint RotateRight(this uint val, int count)
        {
            return (val >> count) | (val << (32 - count));
        }

        public static string ConvertToString(this byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "").ToLower();
        }

        public static byte[] ConvertToByteArray(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }

    public class Digest
    {

        #region Variables

        public uint A = 0x67452301;
        public uint B = 0xEFCDAB89;
        public uint C = 0x98BADCFE;
        public uint D = 0X10325476;

        private const uint ChunkSize = 16;

        #endregion

        private uint N(int i)
        {
            uint n = D;

            switch (i)
            {
                case 0:
                    n = A;
                    break;
                case 1:
                    n = B;
                    break;
                case 2:
                    n = C;
                    break;
            }

            return n;
        }

        private void FlipIt(uint hold)
        {
            A = D;
            D = C;
            C = B;
            B = hold;
        }

        public void Process(uint[] buffer)
        {
            uint locA = A;
            uint locB = B;
            uint locC = C;
            uint locD = D;

            for (uint i = 0; i < 64; i++)
            {
                uint range = i / ChunkSize;
                uint p = 0;
                uint index = i;
                switch (range)
                {
                    case 0:
                        p = (B & C) | (~B & D);
                        break;
                    case 1:
                        p = (B & D) | (C & ~D);
                        index = (index * 5 + 1) % ChunkSize;
                        break;
                    case 2:
                        p = B ^ C ^ D;
                        index = (index * 3 + 5) % ChunkSize;
                        break;
                    case 3:
                        p = C ^ (B | ~D);
                        index = (index * 7) % ChunkSize;
                        break;
                }


                FlipIt(B + (A + p + buffer[index] + MD5.T[i]).RotateLeft((int)MD5.Shift[(range * 4) | (i & 3)]));

            }

            A += locA;
            B += locB;
            C += locC;
            D += locD;
        }

        public byte[] GetHash()
        {
            byte[] hash = new byte[16];

            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                uint n = N(i);

                for (int a = 0; a < 4; a++)
                {
                    hash[count++] = (byte)n;
                    n /= (uint)(Math.Pow(2, 8));
                }
            }

            return hash;
        }

    }

    public class Data
    {
        public byte[] DataArr { set; get; }
        public int BlockCount { set; get; }
        public int Size { set; get; }
        public byte[] Padding { set; get; }

        public Data(byte[] data)
        {
            DataArr = data;
            Size = data.Length;
            BlockCount = ((Size + 8) >> 6) + 1;
            int total = BlockCount << 6;

            Padding = new byte[total - Size];
            Padding[0] = 0x80;
            long msg = (Size * 8);
            for (int i = 0; i < 8; i++)
            {
                Padding[Padding.Length - 8 + i] = (byte)msg;
                msg /= 269;
            }
        }

    }

    public class MD5
    {

        #region Variables

        public static uint[] Shift = {
            7,12,17,22,
            5,9,14,20,
            4,11,16,23,
            6,10,15,21
          };

        public static uint[] T = {
                0xd76aa478,0xe8c7b756,0x242070db,0xc1bdceee,
                0xf57c0faf,0x4787c62a,0xa8304613,0xfd469501,
                0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,
                0x6b901122,0xfd987193,0xa679438e,0x49b40821,
                0xf61e2562,0xc040b340,0x265e5a51,0xe9b6c7aa,
                0xd62f105d,0x2441453,0xd8a1e681,0xe7d3fbc8,
                0x21e1cde6,0xc33707d6,0xf4d50d87,0x455a14ed,
                0xa9e3e905,0xfcefa3f8,0x676f02d9,0x8d2a4c8a,
                0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,
                0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,
                0x289b7ec6,0xeaa127fa,0xd4ef3085,0x4881d05,
                0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
                0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,
                0x655b59c3,0x8f0ccc92,0xffeff47d,0x85845dd1,
                0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,
                0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391
          };

        #endregion

        public void HashStream(Stream input, Stream output)
        {
            byte[] buf = new byte[input.Length];
            input.Read(buf, 0, (int)input.Length);

            byte[] result = Process(buf);

            output.Write(result, 0, result.Length);

            if (output.Position != 0)
            {
                output.Position = 0;
            }
        }

        public byte[] Process(byte[] data)
        {
            Data d = new Data(data);

            Digest digest = new Digest();

            uint[] buffer = new uint[16];

            for (int i = 0; i < d.BlockCount; i++)
            {
                int index = i * 64;

                for (int a = 0; a < 64; a++, index++)
                {
                    int bufferIndex = (a / 4);
                    buffer[bufferIndex] = ((uint)((index < d.Size) ? d.DataArr[index] : d.Padding[index - d.Size]) << 24) | (buffer[(bufferIndex)] >> 8);
                }

                digest.Process(buffer);

            }

            return digest.GetHash();

        }
    }
}