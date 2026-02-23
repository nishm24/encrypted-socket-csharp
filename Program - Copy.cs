using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Collections.Generic;

class Program
{
    static byte[] key = Encoding.UTF8.GetBytes("1234567890123456");
    static byte[] iv  = Encoding.UTF8.GetBytes("6543210987654321");

    static Dictionary<string, Dictionary<string, int>> data =
        new Dictionary<string, Dictionary<string, int>>()
        {
            { "SetA", new Dictionary<string, int>{{"One",1},{"Two",2}} },
            { "SetB", new Dictionary<string, int>{{"Three",3},{"Four",4}} }
        };

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        server.Start();
        Console.WriteLine("Server Started...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            new Thread(() => HandleClient(client)).Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytes = stream.Read(buffer, 0, buffer.Length);

        string msg = Decrypt(buffer, bytes);
        Console.WriteLine("Received: " + msg);

        string[] parts = msg.Split('-');

        if (parts.Length == 2 &&
            data.ContainsKey(parts[0]) &&
            data[parts[0]].ContainsKey(parts[1]))
        {
            int n = data[parts[0]][parts[1]];
            for (int i = 0; i < n; i++)
            {
                string time = DateTime.Now.ToString();
                byte[] enc = Encrypt(time);
                stream.Write(enc, 0, enc.Length);
                Thread.Sleep(1000);
            }
        }
        else
        {
            byte[] enc = Encrypt("EMPTY");
            stream.Write(enc, 0, enc.Length);
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