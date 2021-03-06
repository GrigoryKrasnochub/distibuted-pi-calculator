﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientServerCSharp.Settings
{
    class Settings
    {
        public const string ipSettingsHolder = "-ip";
        public const string portSettingsHolder = "-p";
        public const string stepSettingsHolder = "-s";
        public const string clientsSettingsHolder = "-c";

        string host = "127.0.0.1";
        int port = 8888;
        long steps = 1000000000;
        int clients = 100;

        Regex IpRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

        public string Host { get => host; set => host = value; }
        public int Port { get => port; set => port = value; }
        public long Steps { get => steps; set => steps = value; }
        public int Clients { get => clients; set => clients = value; }

        public void parseSettings(string[] commands)
        {
            for (int i = 0; i < commands.Length - 1; i++)
            {
                switch (commands[i])
                {
                    case ipSettingsHolder:
                        if (IpRegex.IsMatch(commands[i+1]))
                        {
                            Console.WriteLine("set host ip " + commands[i + 1]);
                            Host = commands[i + 1];
                            i++;
                        }
                        break;

                    case portSettingsHolder:
                        if (int.TryParse(commands[i + 1], out int value))
                        {
                            Console.WriteLine("set host port " + commands[i + 1]);
                            Port = value;
                            i++;
                        }
                        break;

                    case stepSettingsHolder:
                        if (long.TryParse(commands[i + 1], out long lValue))
                        {
                            Console.WriteLine("set steps " + commands[i + 1]);
                            Steps = lValue;
                            i++;
                        }
                        break;

                    case clientsSettingsHolder:
                        if (int.TryParse(commands[i + 1], out int clients))
                        {
                            Console.WriteLine("set max clients " + commands[i + 1]);
                            Clients = clients;
                            i++;
                        }
                        break;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("ip - {0}, port - {1}, steps - {2}, clients - {3}", Host, Port, Steps, Clients);
        }
    }
}
