using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static List<string> onlinePlayers = new List<string>();

    static void Main(string[] args)
    {
        Console.WriteLine("Server:");
        var ip = IPAddress.Parse("192.168.0.101");
        var port = 27001;
        var listener = new TcpListener(ip, port);
        listener.Start();
        Console.WriteLine("Server dinlənilir...");

        while (true)
        {
            using (TcpClient client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            {
                // Oyunçu adını al
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{playerName} qoşuldu.");

                // Oyunçu adını qeyd et
                onlinePlayers.Add(playerName);

                // Oyunçu siyahısını göndər
                SendOnlinePlayers(stream);

                // Oyunçunun simvolunu seçmesini isteyin
                char playerSymbol;
                while (true)
                {
                    SendMessage(stream, "X mi yoxsa O olmaq istəyirsiniz? (X/O): ");
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string symbolChoice = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim().ToUpper();

                    if (symbolChoice == "X" || symbolChoice == "O")
                    {
                        playerSymbol = symbolChoice[0];
                        break;
                    }
                    else
                    {
                        SendMessage(stream, "Keçərsiz giriş. Zəhmət olmasa 'X' veya 'O' daxil edin.");
                    }
                }

                char[,] board = new char[,] { { '1', '2', '3' }, { '4', '5', '6' }, { '7', '8', '9' } };
                int moves = 0;

                while (true)
                {
                    PrintBoard(board, stream);
                    int number = ReceiveMove(stream);

                    if (number >= 1 && number <= 9)
                    {
                        int row = (number - 1) / 3;
                        int col = (number - 1) % 3;

                        if (board[row, col] != 'X' && board[row, col] != 'O')
                        {
                            board[row, col] = playerSymbol; 
                            moves++;

                            if (CheckWin(board, playerSymbol))
                            {
                                PrintBoard(board, stream);
                                string winMessage = $"{playerName} ({playerSymbol}) qalib geldi!";
                                SendMessage(stream, winMessage);
                                Console.WriteLine(winMessage);
                                break;
                            }
                            else if (moves == 9)
                            {
                                PrintBoard(board, stream);
                                SendMessage(stream, "Oyun bərabərədir!");
                                Console.WriteLine("Oyun bərabərədir!");
                                break;
                            }
                            // Değiştirme simvolu
                            playerSymbol = (playerSymbol == 'X') ? 'O' : 'X';
                        }
                        else
                        {
                            SendMessage(stream, "Bu secim ugursuz, zəhmət olmasa başqa bir rəqəm seçin.");
                        }
                    }
                    else
                    {
                        SendMessage(stream, "Ugursuz rəqəm, zəhmət olmasa 1-9 arası bir rəqəm girin.");
                    }
                }

                // Bağlantını bağla
                onlinePlayers.Remove(playerName);
            }
        }
    }

    static void SendOnlinePlayers(NetworkStream stream)
    {
        string playersList = "Online Oyunçular: " + string.Join(", ", onlinePlayers);
        byte[] data = Encoding.UTF8.GetBytes(playersList);
        stream.Write(data, 0, data.Length);
    }

    static void PrintBoard(char[,] board, NetworkStream stream)
    {
        StringBuilder sb = new StringBuilder("Yenilənmiş taxta:\n");
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                sb.Append(board[i, j] + " ");
            }
            sb.AppendLine();
        }
        SendMessage(stream, sb.ToString());
    }

    static void SendMessage(NetworkStream stream, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    static int ReceiveMove(NetworkStream stream)
    {
        byte[] buffer = new byte[4];
        stream.Read(buffer, 0, buffer.Length);
        return BitConverter.ToInt32(buffer, 0);
    }

    static bool CheckWin(char[,] board, char player)
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                return true;
        }

        for (int j = 0; j < 3; j++)
        {
            if (board[0, j] == player && board[1, j] == player && board[2, j] == player)
                return true;
        }

        if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            return true;
        if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            return true;

        return false;
    }
}
