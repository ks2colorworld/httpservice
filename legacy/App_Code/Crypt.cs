using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class Crypt
{
    // 암호화 키
    private const string desKey = "kesso.kr";

    public Crypt()
    {
    }

    //------------------------------------------------------------------------
    #region MD5 Hash

    public static string MD5HashCrypt(string val)
    {
        byte[] data = Convert.FromBase64String(val);
        // This is one implementation of the abstract class MD5.
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(data);

        return Convert.ToBase64String(result);
    }

    #endregion //MD5 Hash

    //------------------------------------------------------------------------
    #region DES암복호화

    // Public Function
    public static string DESEncrypt(string inStr)
    {
        return DesEncrypt(inStr, desKey);
    }

    //문자열 암호화
    private static string DesEncrypt(string str, string key)
    {
        //키 유효성 검사
        byte[] btKey = ConvertStringToByteArrayA(key);

        //키가 8Byte가 아니면 예외발생
        if (btKey.Length != 8)
        {
            throw (new Exception("Invalid key. Key length must be 8 byte."));
        }

        //소스 문자열
        byte[] btSrc = ConvertStringToByteArray(str);
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        des.Key = btKey;
        des.IV = btKey;

        ICryptoTransform desencrypt = des.CreateEncryptor();

        MemoryStream ms = new MemoryStream();

        CryptoStream cs = new CryptoStream(ms, desencrypt,
         CryptoStreamMode.Write);

        cs.Write(btSrc, 0, btSrc.Length);
        cs.FlushFinalBlock();


        byte[] btEncData = ms.ToArray();

        return (ConvertByteArrayToStringB(btEncData));
    }//end of func DesEncrypt

    // Public Function
    public static string DESDecrypt(string inStr) // 복호화
    {
        return DesDecrypt(inStr, desKey);
    }

    //문자열 복호화
    private static string DesDecrypt(string str, string key)
    {
        //키 유효성 검사
        byte[] btKey = ConvertStringToByteArrayA(key);

        //키가 8Byte가 아니면 예외발생
        if (btKey.Length != 8)
        {
            throw (new Exception("Invalid key. Key length must be 8 byte."));
        }


        byte[] btEncData = ConvertStringToByteArrayB(str);
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        des.Key = btKey;
        des.IV = btKey;

        ICryptoTransform desdecrypt = des.CreateDecryptor();

        MemoryStream ms = new MemoryStream();

        CryptoStream cs = new CryptoStream(ms, desdecrypt,
         CryptoStreamMode.Write);

        cs.Write(btEncData, 0, btEncData.Length);

        cs.FlushFinalBlock();

        byte[] btSrc = ms.ToArray();


        return (ConvertByteArrayToString(btSrc));

    }//end of func DesDecrypt

    //문자열->유니코드 바이트 배열
    private static Byte[] ConvertStringToByteArray(String s)
    {
        return (new UnicodeEncoding()).GetBytes(s);
    }

    //유니코드 바이트 배열->문자열
    private static string ConvertByteArrayToString(byte[] b)
    {
        return (new UnicodeEncoding()).GetString(b, 0, b.Length);
    }

    //문자열->안시 바이트 배열
    private static Byte[] ConvertStringToByteArrayA(String s)
    {
        return (new ASCIIEncoding()).GetBytes(s);
    }

    //안시 바이트 배열->문자열
    private static string ConvertByteArrayToStringA(byte[] b)
    {
        return (new ASCIIEncoding()).GetString(b, 0, b.Length);
    }

    //문자열->Base64 바이트 배열
    private static Byte[] ConvertStringToByteArrayB(String s)
    {
        return Convert.FromBase64String(s);
    }

    //Base64 바이트 배열->문자열
    private static string ConvertByteArrayToStringB(byte[] b)
    {
        return Convert.ToBase64String(b);
    }

    #endregion //DES암복호화

    //------------------------------------------------------------------------
    #region RSA암복호화
    //RSA 암호화
    public static string RSAEncrypt(string sValue, string sPubKey)
    {
        //공개키 생성
        byte[] keybuf = Convert.FromBase64String(sPubKey);
        sPubKey = (new UTF8Encoding()).GetString(keybuf);
        
        System.Security.Cryptography.RSACryptoServiceProvider oEnc = new RSACryptoServiceProvider(); //암호화


        oEnc.FromXmlString(sPubKey);

        //암호화할 문자열을 UFT8인코딩
        byte[] inbuf = (new UTF8Encoding()).GetBytes(sValue);
        //암호화
        byte[] encbuf = oEnc.Encrypt(inbuf, false);

        //암호화된 문자열 Base64인코딩
        return Convert.ToBase64String(encbuf);
    }
    //RSA 복호화
    public static string RSADecrypt(string sValue, string sPrvKey)
    {
        //개인키 생성
        byte[] inbuf = Convert.FromBase64String(sPrvKey);
        sPrvKey = (new UTF8Encoding()).GetString(inbuf);

        //RSA객체생성
        System.Security.Cryptography.RSACryptoServiceProvider oDec = new RSACryptoServiceProvider(); //복호화
        //개인키로 활성화
        oDec.FromXmlString(sPrvKey);

        //sValue문자열을 바이트배열로 변환
        byte[] srcbuf = Convert.FromBase64String(sValue);

        //바이트배열 복호화
        byte[] decbuf = oDec.Decrypt(srcbuf, false);

        //복호화 바이트배열을 문자열로 변환
        string sDec = (new UTF8Encoding()).GetString(decbuf, 0, decbuf.Length);
        return sDec;
    }

    #endregion

}
