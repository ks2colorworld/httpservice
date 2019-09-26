using System;
using System.Security.Cryptography;

public class AESCipher
{
    private System.Text.UTF8Encoding utf8Encoding = null;
    private RijndaelManaged rijndael = null;

    private XMLCommonUtil xmlCommonUtil;

    public AESCipher(string key)
    {
        xmlCommonUtil = new XMLCommonUtil();
        if (key == null || key == "")
        {
            try
            {
                throw new ArgumentException("The key is not null.", "key");
            }
            catch (ArgumentException ex)
            {
                xmlCommonUtil.ResponseWriteErrorMSG("AESCipher()", ex);
                return;
            }
        }
        this.utf8Encoding = new System.Text.UTF8Encoding();
        this.rijndael = new RijndaelManaged();
        this.rijndael.Mode = CipherMode.ECB;
        this.rijndael.Padding = PaddingMode.PKCS7;
        this.rijndael.KeySize = 128;
        this.rijndael.BlockSize = 128;

        this.rijndael.Key = hex2Byte(key);
    }

    public string Encrypt(string text)
        {
            byte[] cipherBytes = null;
            ICryptoTransform transform = null;
            if (text == null)
                text = "";
            try
            {
                cipherBytes = new byte[] {};
                transform = this.rijndael.CreateEncryptor();
                byte[] plainText = this.utf8Encoding.GetBytes(text);
                cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch
            {
                try
                {
                    throw new ArgumentException(
                       "text is not a valid string!(Encrypt)", "text");
                }
                catch (ArgumentException ex)
                {
                    xmlCommonUtil.ResponseWriteErrorMSG("Encrypt()", ex);
                    return string.Empty;
                }
            }
            finally
            {
            }
            return Convert.ToBase64String(cipherBytes);
        }

    public string Decrypt(string text)
    {
        byte[] plainText = null;
        ICryptoTransform transform = null;
        if (text == null || text == "")
        {
            try
            {
                throw new ArgumentException("text is not null.");
            }
            catch (ArgumentException ex)
            {
                xmlCommonUtil.ResponseWriteErrorMSG("Decrypt()", ex);
                return string.Empty;
            }
        }
        try
        {
            plainText = new byte[] { };
            transform = rijndael.CreateDecryptor();
            byte[] encryptedValue = Convert.FromBase64String(text);
            plainText = transform.TransformFinalBlock(encryptedValue, 0,
               encryptedValue.Length);
        }
        catch
        {
            try
            {
                throw new ArgumentException(
                   "text is not a valid string!(Decrypt)", "text");
            }
            catch (ArgumentException ex)
            {
                xmlCommonUtil.ResponseWriteErrorMSG("Decrypt()", ex);
                return string.Empty;
            }
        }
        finally
        {
        }

        return this.utf8Encoding.GetString(plainText);
    }

    public byte[] hex2Byte(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            try
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            catch
            {
                try
                {
                    throw new ArgumentException(
                    "hex is not a valid hex number!", "hex");
                }
                catch (ArgumentException ex)
                {
                    xmlCommonUtil.ResponseWriteErrorMSG("hex2Byte()", ex);
                    return null;
                }
            }
        }
        return bytes;
    }

    public string byte2Hex(byte[] bytes)
    {
        string hex = "";
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                hex += bytes[i].ToString("X2");
            }
        }
        return hex;
    }
}
