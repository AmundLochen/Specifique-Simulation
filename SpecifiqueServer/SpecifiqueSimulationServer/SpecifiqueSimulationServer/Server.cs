using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BLL;
using Model;

namespace SpecifiqueSimulationServer
{
    /// <summary>
    ///     Contains server logic and values
    /// </summary>
    public static class Server
    {
        public static readonly GameClock Clock = new GameClock();
        public static bool Pause = true;
        public static DateTime Time;
        public static DateTime EndTime;
        private static readonly TimeSpan Speed = TimeSpan.FromHours(5);
        private static TimeModel _timeValues;
        private static HandleClinet _client = new HandleClinet();

        /// <summary>
        ///     Establishes server connection
        /// </summary>
        public static void ServerConnection()
        {
            //  Starts the application and starts listening for clients.
            var serverSocket = new TcpListener(IPAddress.Any, 8888);
            var newClient = new HandleClinet();

            int counter = 0;

            serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            _timeValues = ValueLogic.GetTimeValues();

            Time = _timeValues.CurrentTime;
            EndTime = _timeValues.EndTime;
            Clock.SetDate(Time, Speed);

            var serverInputThread = new Thread(ServerInput);
            serverInputThread.Start();

            //  Detects client and opens socket. Makes a thread for the client and keeps listening for other clients afterwards.
            while (true)
            {
                //  Listens for clients.
                TcpClient clientSocket = serverSocket.AcceptTcpClient();

                // Checks if no other clients has connected.
                if (counter == 0)
                {
                    newClient.NextClient = newClient;

                    _client = newClient;

                    Console.WriteLine(" >> " + "Client connected!");
                    counter++;
                    _client.StartClient(clientSocket);
                }

                else
                {
                    //  Checks if a client has disconnected.
                    HandleClinet dcClient = _client.NextClient;
                    while (dcClient != _client)
                    {
                        if (dcClient.Disconnected)
                            break;

                        dcClient = dcClient.NextClient;
                    }

                    //  Reconnects client if it has disconnected.
                    if (dcClient.Disconnected)
                    {
                        dcClient.Disconnected = false;
                        HandleClinet tempClients = _client.NextClient;
                        while (tempClients.NextClient != dcClient)
                            tempClients = tempClients.NextClient;
                        tempClients.NextClient = dcClient;
                        _client = tempClients.NextClient;
                        Console.WriteLine(" >> " + "Client reconnected!");
                        _client.StartClient(clientSocket);
                    }

                        // Adds new client.
                    else
                    {
                        newClient = new HandleClinet {NextClient = _client};

                        HandleClinet temp = newClient.NextClient;

                        while (temp.NextClient != newClient.NextClient)
                            temp = temp.NextClient;

                        temp.NextClient = newClient;

                        _client = newClient;

                        Console.WriteLine(" >> " + "Client connected!");
                        counter++;
                        _client.StartClient(clientSocket);
                    }
                }
            }
        }

        /// <summary>
        ///     Listens for input from admin
        /// </summary>
        private static void ServerInput()
        {
            while (true)
            {
                Console.WriteLine("Enter command to interact with the game: ");
                string s = Console.ReadLine();

                if (s != null && s.StartsWith("notific"))
                {
                    var newNotification = new NotificationModel(0, 0, "", "", DateTime.Now);

                    Console.WriteLine(
                        "Enter recipient for new Notification(Group number for one specific, 0 if send to all): ");
                    s = Console.ReadLine();
                    int toId;
                    if (!int.TryParse(s, out toId))
                    {
                        Console.WriteLine("Invalid input!");
                    }
                    else if (!_client.GroupNumberTaken(toId) && toId == 0)
                    {
                        Console.WriteLine("Group " + toId + " does not exist!");
                    }
                    else
                    {
                        newNotification.NotificTo = toId;
                        Console.WriteLine("Enter notification subject: ");
                        newNotification.Subject = Console.ReadLine();
                        Console.WriteLine("Enter notification text: ");
                        newNotification.Text = Console.ReadLine();
                        newNotification.Time = Clock.GameTime;

                        var listOfNotifications = new List<NotificationModel>();

                        if (newNotification.NotificTo == 0)
                        {
                            foreach (HandleClinet recipient in _client.GetClients())
                            {
                                newNotification.NotificTo = recipient.Groupnumber;
                                listOfNotifications.Add(NotificationLogic.NewNotific(newNotification));
                            }
                        }
                        else
                        {
                            listOfNotifications.Add(NotificationLogic.NewNotific(newNotification));
                        }
                        foreach (HandleClinet recipient in _client.GetClients())
                        {
                            foreach (NotificationModel n in listOfNotifications)
                            {
                                if (n.NotificTo == recipient.Groupnumber)
                                {
                                    recipient.SendNotification(n);
                                }
                            }
                        }
                    }
                }

                else if (s != null && s.StartsWith("pause"))
                {
                    Pause = true;
                    Console.WriteLine(" >> " + "Game is paused!");
                    Time = Clock.GameTime;
                    foreach (HandleClinet recipient in _client.GetClients())
                    {
                        recipient.SendPauseState(Pause);
                    }
                }
                else if (s != null && s.StartsWith("start"))
                {
                    Pause = false;
                    Console.WriteLine(" >> " + "Game is started!");
                    Clock.SetDate(Time, Speed);
                    foreach (HandleClinet recipient in _client.GetClients())
                    {
                        recipient.SendPauseState(Pause);
                    }
                }
                else if (s != null && s.StartsWith("set date"))
                {
                    Console.WriteLine("Enter the new date: (dd.MM.yyyy)");
                    try
                    {
                        DateTime date = DateTime.Parse(Console.ReadLine());
                        Clock.SetDate(date, Speed);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Not a valid date, enter a new date in this format(dd.MM.yyyy)");
                    }
                }
                else if (s != null && s.StartsWith("set item price"))
                {
                    Console.WriteLine("Enter item id:");
                    s = Console.ReadLine();
                    int itemId;
                    double price;
                    ItemModel item;
                    if (!int.TryParse(s, out itemId))
                    {
                        Console.WriteLine("Invalid input!");
                    }
                    else
                    {
                        item = InventoryLogic.GetItemById(itemId);
                        if (item == null)
                        {
                            Console.WriteLine("Invalid item id!");
                        }
                        else
                        {
                            Console.WriteLine("Enter new price:");
                            s = Console.ReadLine();
                            if (!double.TryParse(s, out price))
                            {
                                Console.WriteLine("Invalid input!");
                            }
                            else
                            {
                                ValueLogic.EditItemPrice(itemId, price);
                                var notification = new NotificationModel
                                    {
                                        Subject = item.Name + " price changed!",
                                        Text = "The price is now " + price + ", previously " +
                                               item.Price + ".",
                                        Time = Clock.GameTime
                                    };

                                item = InventoryLogic.GetItemById(itemId);

                                foreach (HandleClinet cl in _client.GetClients())
                                {
                                    cl.SendItem(item);
                                    notification.NotificTo = cl.Groupnumber;
                                    notification = NotificationLogic.NewNotific(notification);
                                    cl.SendNotification(notification);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not a valid command, enter another command");
                }
            }
        }
    }
}