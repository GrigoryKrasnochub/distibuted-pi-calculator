using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ClientServerCSharp.Server
{
    class Server : Messages.Messages
    {
        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания

        public Server()
        {
            ModuleName = "Server";
        }

        public void StartServer(Settings.Settings settings)
        {
            try
            {
                server = new ServerObject(ref settings);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        public double DoCalculations()
        {
            DateTime startDate = DateTime.Now;
            double calcResult = server.DoCalculations();
            DateTime stopDate = DateTime.Now;
            Console.WriteLine("Calculation time " + (stopDate - startDate).ToString(@"hh\:mm\:ss\.fff"));
            return calcResult;
        }

        public override string[] GetMessages()
        {
            if (server != null) {
                return server.GetMessages();
            } else {
                return new string[0];
            }
        }
    }
}
