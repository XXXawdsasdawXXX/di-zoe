using System;
using System.Security.Cryptography;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Device;

namespace Code.Tools
{
    public static class JsonExtensions
    {
            public static string ToJson(this object obj)
        {
            string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            //return EncryptData(json);
            return json;
        }


        public static T ToDeserialized<T>(this string json)
        {
            /*try
            {
                var t = JsonConvert.DeserializeObject<T>(DecryptData(json));
                return t;
            }
            catch (Exception e)
            {
                Debugging.Instance.ErrorLog($"e");*/
            return JsonConvert.DeserializeObject<T>(json);
            //}
        }

        private static string GenerateUniqueKey()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            byte[] deviceIdBytes = Encoding.UTF8.GetBytes(deviceId);
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(deviceIdBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private static string EncryptData(string data)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Convert.FromBase64String(GenerateUniqueKey());
            aesAlg.IV = new byte[16];
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using System.IO.MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (System.IO.StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(data);
                }
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        private static string DecryptData(string encryptedData)
        {
            if (encryptedData == "")
            {
                return encryptedData;
            }

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Convert.FromBase64String(GenerateUniqueKey());
            aesAlg.IV = new byte[16];
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using System.IO.MemoryStream
                msDecrypt = new(Convert.FromBase64String(encryptedData));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using System.IO.StreamReader srDecrypt = new(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
}