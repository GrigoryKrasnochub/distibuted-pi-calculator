using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientServerCSharp.Server
{
    class ClientObject : Messages.Messages
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        TcpClient client;
        ServerObject server; // объект сервера

        protected internal double Result { get; private set; }

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Result = 0;
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void ResetResults()
        {
            Result = 0;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                
                string message = this.Id + " conected";
                AddMessage(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message == "")
                        {
                            continue;
                        }
                        message = String.Format("{0}: {1}", this.Id, message);
                        AddMessage(message);
                    }
                    catch
                    {
                        message = String.Format("{0}: leave", this.Id);
                        AddMessage(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                AddMessage(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            string message = builder.ToString();
            string[] messageParts = message.Split(" ");
            double res;
            if (messageParts.Length > 1 && double.TryParse(messageParts[1], out res))
            {
                Result = res;
            } else
            {
                AddMessage("Result parse ERR");
            }
            return message;
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
