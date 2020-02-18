using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DLG.ToolBox.Enc
{
    public class DES
    {
        // Fields
        private SymmetricAlgorithm _mSymmetric;

        // Methods

        public void DESCrypt(string key)
        {
            initDes(key, key);
        }

        public void DESCrypt(string key, string iv)
        {
            initDes(key, iv);
        }

        private void initDes(string key, string iv)
        {
            _mSymmetric = new DESCryptoServiceProvider();
            setKey(key);
            setIV(iv);
        }

        public string Encrypt(string str)
        {
            return Convert.ToBase64String(EncryptWithByte(str));
        }

        public string Decrypt(string strEncrypt)
        {
            return decrypt(Convert.FromBase64String(strEncrypt));
        }

        private byte[] EncryptWithByte(string str)
        {
            var stream = new MemoryStream();
            var transform = _mSymmetric.CreateEncryptor();
            var stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            var writer = new StreamWriter(stream2);
            writer.Write(str);
            writer.Flush();
            stream2.FlushFinalBlock();
            var buffer = new byte[stream.Length];
            stream.Position = 0L;
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        
        private string decrypt(byte[] bytEncrypt)
        {
            var stream = new MemoryStream(bytEncrypt);
            var stream2 = new CryptoStream(stream, _mSymmetric.CreateDecryptor(), CryptoStreamMode.Read);
            var reader = new StreamReader(stream2);
            return reader.ReadToEnd();
        }

        private static byte[] getValidIv(string initVector, int validLength)
        {
            return Encoding.ASCII.GetBytes(initVector.Length > validLength ? initVector.Substring(0, validLength) : initVector.PadRight(validLength, ' '));
        }

        private byte[] getValidKey(string key)
        {
            var minSize = _mSymmetric.LegalKeySizes[0].MinSize;
            var str = (key.Length*8) > minSize ? key.Substring(0, minSize/8) : key.PadRight(minSize/8, ' ');
            return Encoding.ASCII.GetBytes(str);
        }

        private void setIV(string iv)
        {
            _mSymmetric.IV = getValidIv(iv, _mSymmetric.IV.Length);
        }

        private void setKey(string key)
        {
            _mSymmetric.Key = getValidKey(key);
        }
    }
}
