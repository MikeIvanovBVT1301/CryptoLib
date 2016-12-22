using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using CryptoLibrary;


namespace DESTests
{
    [TestClass]
    public class DESTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        [TestMethod]
        public void TestMethod1()
        {
            string data = "MIKEMIKEMIKE";
            string key = "77rrropl8";

            byte[] inpByteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream inpStream = new MemoryStream(inpByteArray);
            byte[] keyByteArray = Encoding.ASCII.GetBytes(key);
            MemoryStream keyStream = new MemoryStream(keyByteArray);

            Stream encrStream = new MemoryStream();
            Stream decrStream = new MemoryStream();

            DES target = new DES(keyStream);

            target.Encrypt(inpStream, out encrStream);
            target.Decrypt(encrStream, out decrStream);

            StreamReader reader = new StreamReader(decrStream);
            string decrypted = reader.ReadToEnd();

            Assert.AreEqual(decrypted, data);
        }
    }

}

