using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

class Program
{
    static byte[] key = Encoding.UTF8.GetBytes("1234567890123456");
    static byte[] iv  = Encoding.UTF8.GetBytes("6543210987654321");

    static void Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();

        Console.Write("Enter string (Example: SetA-Two): ");
        string msg = Console.ReadLine();

        byte[] enc = Encrypt(msg);
        stream.Write(enc, 0, enc.Length);

        byte[] buffer = new byte[1024];
        int bytes;
        while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            Console.WriteLine("Server: " + Decrypt(buffer, bytes));
        }

        client.Close();
    }

    static byte[] Encrypt(string text)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        return aes.CreateEncryptor()
                  .TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
    }

    static string Decrypt(byte[] data, int len)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        return Encoding.UTF8.GetString(
            aes.CreateDecryptor().TransformFinalBlock(data, 0, len));
    }
}
