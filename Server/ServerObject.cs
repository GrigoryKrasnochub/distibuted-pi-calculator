using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServerCSharp.Server
{
    class ServerObject : Messages.Messages
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        Settings.Settings Settings;
        
        public ServerObject(ref Settings.Settings settings)
        {
            Settings = settings;
        }

        private void ResetClients()
        {
            foreach (ClientObject client in clients)
            {
                client.ResetResults();
            }
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Settings.Port);
                tcpListener.Start();
                AddMessage("Start server. Port " + Settings.Port);

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
                Disconnect();
            }
        }
        
        public double DoCalculations()
        {
            ResetClients();
            int maxClientsCount = Settings.Clients;
            int count = clients.Count;
            count = count > maxClientsCount ? maxClientsCount : count;

            double stepsPerClient = Settings.Steps / count;

            long start = 0;
            long stop = 0;
            int activeClients = count;
            foreach(ClientObject client in clients)
            {
                activeClients--;
                if (activeClients == 0)
                {
                    stop = Settings.Steps;
                } else
                {
                    stop += (long)stepsPerClient;
                }
                sendMessage(String.Format("calc {0} {1} {2}", start, stop, Settings.Steps), client.Id);
                start = stop;
                if (activeClients == 0)
                {
                    break;
                }
            }

            int clientsCount = 0;
            double result = 0;
            while(true)
            {
                foreach (ClientObject client in clients)
                {
                    if (client.Result != 0)
                    {
                        result += client.Result;
                        clientsCount++;
                    }
                }
                if (count == clientsCount)
                {
                    break;
                }
                result = 0;
                clientsCount = 0;
            }

            return result/Settings.Steps;
        }

        // трансляция сообщения подключенным клиентам
        protected internal void sendMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                    break;
                }
            }
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }

        public override string[]  GetMessages()
        {
            string[] servMessages = base.GetMessages();
            List<string> Messages = new List<string>();

            foreach (string messages in servMessages)
            {
                Messages.Add(messages);
            }

            foreach (ClientObject client in clients)
            {
                if (client == null)
                {
                    continue;
                }
                foreach ( string messages in client.GetMessages())
                {
                    Messages.Add(messages);
                }
            }

            return Messages.ToArray();
        }
    }
}
