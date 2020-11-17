using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ClientServerCSharp.Client
{
    class Client : Messages.Messages
    {
        TcpClient client;
        NetworkStream stream;
        Settings.Settings Settings;

        public Client(ref Settings.Settings settings)
        {
            ModuleName = "client";
            Settings = settings;
        }

        private void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public void Connect()
        {
            client = new TcpClient();
            try
            {
                client.Connect(Settings.Host, Settings.Port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока
                AddMessage("Client connected. Waiting for Server commands...");
                while (true){};
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    AddMessage("get command");
                    AddMessage(message);

                    if (message.Length > 0)
                    {
                        string[] messageParts = message.Split(" ");
                        if (messageParts.Length > 0)
                        {
                            switch (messageParts[0])
                            {
                                case "calc":
                                    if (messageParts.Length < 4)
                                    {
                                        break;
                                    }

                                    long[] calcParams = new long[3];
                                    int paramsCounter = 0;
                                    foreach (string part in messageParts)
                                    {
                                        long value;
                                        if (long.TryParse(part, out value))
                                        {
                                            calcParams[paramsCounter] = value;
                                            paramsCounter++;
                                        }
                                        if (paramsCounter > 2)
                                        {
                                            DoCalc(calcParams[0], calcParams[1], calcParams[2]);
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    AddMessage("loose connection");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        public void DoCalc(long startStep, long stopStep, long steps)
        {
            AddMessage("Start calculations startStep/stopStep:" + startStep.ToString() + "/" + stopStep.ToString());

            double x;
            double result = 0;
            for (long i = startStep; i < stopStep; i++)
            {
                x = (i + 0.5) / steps;
                result += 4.0 / (1.0 + x * x);
            }
            AddMessage("Calculation finished! Result: " + result.ToString());
            SendMessage("200 " + result);
        }

        public void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
    }
}
