using EasyEncryption;

namespace Parameters1903M.Util
{
    static class EncryptionDecryption
    {
        private static readonly string password = "96Q5faZNX3iDuusQe82Unk6C5zXSBMHL";
        private static readonly string authKey = "96Q5faZNX3iDuusQ";

        public static string Encrypt(string text) => AES.Encrypt(text, password, authKey);

        public static string Decrypt(string text) => AES.Decrypt(text, password, authKey);
    }
}
