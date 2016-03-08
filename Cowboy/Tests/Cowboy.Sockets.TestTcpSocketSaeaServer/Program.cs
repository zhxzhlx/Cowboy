﻿using System;
using System.Text;
using System.Threading.Tasks;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;

namespace Cowboy.Sockets.TestTcpSocketSaeaServer
{
    class Program
    {
        static TcpSocketSaeaServer _server;

        static void Main(string[] args)
        {
            NLogLogger.Use();

            try
            {
                var config = new TcpSocketSaeaServerConfiguration();

                _server = new TcpSocketSaeaServer(22222, new SimpleMessageDispatcher(), config);
                _server.Listen();

                Console.WriteLine("TCP server has been started on [{0}].", _server.ListenedEndPoint);
                Console.WriteLine("Type something to send to clients...");
                while (true)
                {
                    try
                    {
                        string text = Console.ReadLine();
                        if (text == "quit")
                            break;
                        Task.Run(async () =>
                        {
                            if (text == "many")
                            {
                                text = new string('x', 8192);
                                for (int i = 0; i < 1000000; i++)
                                {
                                    await _server.BroadcastAsync(Encoding.UTF8.GetBytes(text));
                                    Console.WriteLine("Server [{0}] broadcasts text -> [{1}].", _server.ListenedEndPoint, text);
                                }
                            }
                            else if (text == "big")
                            {
                                text = new string('x', 1024 * 1024 * 100);
                                await _server.BroadcastAsync(Encoding.UTF8.GetBytes(text));
                                Console.WriteLine("Server [{0}] broadcasts text -> [{1}].", _server.ListenedEndPoint, text);
                            }
                            else
                            {
                                await _server.BroadcastAsync(Encoding.UTF8.GetBytes(text));
                                Console.WriteLine("Server [{0}] broadcasts text -> [{1}].", _server.ListenedEndPoint, text);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                _server.Shutdown();
                Console.WriteLine("TCP server has been stopped on [{0}].", _server.ListenedEndPoint);
            }
            catch (Exception ex)
            {
                Logger.Get<Program>().Error(ex.Message, ex);
            }

            Console.ReadKey();
        }
    }
}
