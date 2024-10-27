using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        Console.WriteLine("Client:");
        var ip = IPAddress.Parse("192.168.0.101");
        var port = 27001;
        var client = new TcpClient();

        try
        {
            client.Connect(ip, port);
            using (var stream = client.GetStream())
            {
                Console.Write("Zehmet olmasa oyuncu adinizi daxil edin: ");
                string playerName = Console.ReadLine();

                byte[] nameData = Encoding.UTF8.GetBytes(playerName);
                stream.Write(nameData, 0, nameData.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string onlinePlayers = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(onlinePlayers);

                char playerSymbol = ' ';
                while (playerSymbol != 'X' && playerSymbol != 'O')
                {
                    Console.Write("X mi yoxsa O olmaq istəyirsiniz? (X/O): ");
                    var input = Console.ReadLine().ToUpper();

                    if (input == "X" || input == "O")
                    {
                        playerSymbol = input[0];
                        byte[] symbolData = Encoding.UTF8.GetBytes(playerSymbol.ToString());
                        stream.Write(symbolData, 0, symbolData.Length);
                    }
                    else
                    {
                        Console.WriteLine("Ugursuz secim. Zəhmət olmasa 'X' veya 'O' daxil edin.");
                    }
                }

                while (true)
                {
                    Console.Write("Rəqəmi daxil edin (1-9 arası): ");
                    var input = Console.ReadLine();

                    if (int.TryParse(input, out int num) && num >= 1 && num <= 9)
                    {
                        byte[] data = BitConverter.GetBytes(num);
                        stream.Write(data, 0, data.Length);

                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Serverdən gələn cavab: " + response);

                        if (response.Contains("qazandı") || response.Contains("bərabərə"))
                            break; 
                    }
                    else
                    {
                        Console.WriteLine("Ugursuz rəqəm daxil etdiniz. Zəhmət olmasa 1-9 arası bir rəqəm daxil edin.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Xəta: " + ex.Message);
        }
        finally
        {
            client.Close();
        }
    }
}

