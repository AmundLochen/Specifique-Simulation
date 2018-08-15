using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using BLL;
using Model;

namespace SpecifiqueSimulationServer
{
    /// <summary>
    ///     Contains client logic and values
    /// </summary>
    public class HandleClinet
    {
        public Boolean Disconnected;
        public int Groupnumber;
        public HandleClinet NextClient;
        private TcpClient _clientSocket;
        private Thread _ctThread;
        private DateTime _oldTimeStamp;
        private DateTime _timeStamp;

        /// <summary>
        ///     Starts communicating with client
        /// </summary>
        /// <param name="inClientSocket">TcpClient socket</param>
        public void StartClient(TcpClient inClientSocket)
        {
            _clientSocket = inClientSocket;
            _ctThread = new Thread(DoChat);
            _ctThread.Start();

            SendTeamList(MessagingLogic.GetTeams());
        }

        /// <summary>
        ///     Gets client
        /// </summary>
        /// <param name="clNr">Groupnumber</param>
        /// <returns></returns>
        private HandleClinet GetClient(int clNr)
        {
            HandleClinet client = this;

            while (client.Groupnumber != clNr)
            {
                client = client.NextClient;
            }

            return client;
        }

        /// <summary>
        ///     Returns all clients.
        /// </summary>
        public List<HandleClinet> GetClients()
        {
            var clients = new List<HandleClinet>();

            HandleClinet client = this;
            clients.Add(client);
            HandleClinet next = client.NextClient;


            while (client != next)
            {
                clients.Add(next);
                next = next.NextClient;
            }

            return clients;
        }

        /// <summary>
        ///     Checks if group number is taken.
        /// </summary>
        /// <param name="gNr">Groupnumber</param>
        /// <returns>If a client has taken the groupnumber</returns>
        public bool GroupNumberTaken(int gNr)
        {
            return GetClients().Any(client => client.Groupnumber == gNr);
        }

        /// <summary>
        ///     Communicates between server and client.
        /// </summary>
        private void DoChat()
        {
            while (!Disconnected)
            {
                Thread.Sleep(10);
                DateTime now = Server.Clock.GameTime;
                _timeStamp = now.Date;

                //  Group 1 sends new time and cash to all clients
                if (_timeStamp != _oldTimeStamp && Server.Pause != true && Groupnumber == 1)
                {
                    ValueLogic.UpdateCurrentTime(_timeStamp);
                    List<CashPerTeamModel> s = ValueLogic.UpdateCash();
                    foreach (HandleClinet client in GetClients())
                    {
                        client.SendGroupCash(s);
                        client.SendTimeStamp(_timeStamp);
                    }
                    _oldTimeStamp = _timeStamp;

                    if (_timeStamp >= Server.EndTime.Date)
                    {
                        Server.Pause = true;
                        Console.WriteLine(" >> " + "Game is over!");
                        Server.Time = Server.Clock.GameTime;
                        foreach (HandleClinet recipient in GetClients())
                        {
                            recipient.SendPauseState(Server.Pause);
                        }
                    }
                }

                //  Checks if data is available and if it is, determines what kind of data it is and responds appropriately
                if (_clientSocket.Available > 0)
                {
                    TextReader reader = new StreamReader(_clientSocket.GetStream());
                    var doc = new XmlDocument();
                    string incomeData = reader.ReadLine();
                    //Console.WriteLine(incomeData);

                    if (incomeData != null && (incomeData.StartsWith("<") && incomeData.EndsWith(">")))
                    {
                        doc.LoadXml(incomeData);
                        XmlElement root = doc.DocumentElement;
                        if (root != null)
                        {
                            string rootText = root.Name;

                            if (rootText == "Message")
                            {
                                MessageModel incoming = RecieveMessage(doc);
                                incoming.Time = _timeStamp;

                                //  
                                if (incoming.ParentId != incoming.Id)
                                {
                                    MessageModel parent = MessagingLogic.GetMessageByParentId(incoming.ParentId);

                                    if (parent.FromId == incoming.FromId)
                                    {
                                        incoming.ToId = parent.ToId;
                                        incoming.ToName = parent.ToName;
                                    }

                                    else
                                    {
                                        incoming.ToId = parent.FromId;
                                        incoming.ToName = parent.FromName;
                                    }

                                    incoming.Subject = parent.Subject;
                                }

                                incoming = MessagingLogic.ForwardMessage(incoming);

                                HandleClinet reciever = GetClient(incoming.ToId);
                                reciever.SendMessage(incoming);
                                reciever = GetClient(incoming.FromId);
                                reciever.SendMessage(incoming);
                            }
                            else if (rootText == "Notification")
                            {
                                NotificationModel incoming = RecieveNotification(doc);
                                NotificationLogic.NewNotific(incoming);
                            }
                            else if (rootText == "Asset")
                            {
                            }

                            else if (rootText == "Item")
                            {
                            }

                            else if (rootText == "Trading")
                            {
                                TradingModel incoming = RecieveTrading(doc);

                                //  Trade is unread.
                                if (incoming.Accept == 0)
                                {
                                    incoming.Time = _timeStamp.Add(new TimeSpan(30, 0, 0, 0, 0));
                                    incoming = ConditionalTradingLogic.NewTrade(incoming);
                                    GetClient(incoming.Owner).SendTrading(incoming);
                                }

                                    //  Trade is declined.
                                else if (incoming.Accept == 1)
                                {
                                    ConditionalTradingLogic.EditTrade(incoming);

                                    string text;

                                    if (incoming.AssetId != 0)
                                    {
                                        text = "The " + InventoryLogic.GetAssetById(incoming.AssetId).Name +
                                               " trade between you and " +
                                               MessagingLogic.GetTeamById(incoming.Owner).Name +
                                               " was declined.";
                                    }

                                    else
                                    {
                                        text = "The " + InventoryLogic.GetItemById(incoming.ItemId).Name +
                                               " trade between you and " +
                                               MessagingLogic.GetTeamById(incoming.Owner).Name +
                                               " was declined.";
                                    }

                                    var notification = new NotificationModel(0, incoming.Buyer, "Trade declined!", text,
                                                                             _timeStamp);
                                    notification = NotificationLogic.NewNotific(notification);
                                    GetClient(incoming.Buyer).SendNotification(notification);
                                }
                        
                                    //  Trade is accepted.
                                else if (incoming.Accept == 2)
                                {
                                    //  
                                    if (incoming.Owner != 0)
                                    {
                                        var notification = new NotificationModel {Time = _timeStamp};

                                        if (incoming.AssetId != 0)
                                        {
                                            //  Checks if trade is still valid.
                                            if (
                                                InventoryLogic.GetAssetInventory(incoming.AssetId, incoming.Owner).Share >=
                                                incoming.Amount)
                                            {
                                                ConditionalTradingLogic.EditTrade(incoming);
                                                List<AssetInventoryModel> ai =
                                                    ConditionalTradingLogic.TradeAsset(incoming);

                                                foreach (HandleClinet client in GetClients())
                                                {
                                                    foreach (AssetInventoryModel assetInventory in ai)
                                                    {
                                                        client.SendAssetInventory(assetInventory);
                                                        //Thread.Sleep(100);
                                                    }
                                                }

                                                notification.Subject = "Trade accepted!";
                                                notification.Text = "The " +
                                                                    InventoryLogic.GetAssetById(incoming.AssetId).Name +
                                                                    " trade between you and " +
                                                                    MessagingLogic.GetTeamById(incoming.Owner).Name +
                                                                    " was accepted.";
                                            }

                                            else
                                            {
                                                incoming.Accept = 4;
                                                ConditionalTradingLogic.EditTrade(incoming);
                                                SendTrading(incoming);
                                                notification.Subject = "Trade invalidated!";
                                                notification.Text = "The " +
                                                                    InventoryLogic.GetAssetById(incoming.AssetId).Name +
                                                                    " trade between you and " +
                                                                    MessagingLogic.GetTeamById(incoming.Owner).Name +
                                                                    " was invalidated, because they traded with someone else.";
                                            }
                                        }

                                        else
                                        {
                                            //  Checks if trade is still valid.
                                            if (
                                                InventoryLogic.GetItemInventory(incoming.ItemId, incoming.Owner)
                                                              .Quantity >=
                                                incoming.Amount)
                                            {
                                                ConditionalTradingLogic.EditTrade(incoming);

                                                List<ItemInventoryModel> ii = ConditionalTradingLogic.TradeItem(incoming);

                                                foreach (HandleClinet client in GetClients())
                                                {
                                                    foreach (ItemInventoryModel itemInventory in ii)
                                                    {
                                                        client.SendItemInventory(itemInventory);
                                                        //Thread.Sleep(100);
                                                    }
                                                }

                                                notification.Subject = "Trade accepted!";
                                                notification.Text = "The " +
                                                                    InventoryLogic.GetItemById(incoming.ItemId).Name +
                                                                    " trade between you and " +
                                                                    MessagingLogic.GetTeamById(incoming.Owner).Name +
                                                                    " was accepted.";
                                            }

                                            else
                                            {
                                                incoming.Accept = 4;
                                                ConditionalTradingLogic.EditTrade(incoming);
                                                SendTrading(incoming);
                                                notification.Subject = "Trade invalidated!";
                                                notification.Text = "The " +
                                                                    InventoryLogic.GetItemById(incoming.ItemId).Name +
                                                                    " trade between you and " +
                                                                    MessagingLogic.GetTeamById(incoming.Owner).Name +
                                                                    " was invalidated, because they traded with someone else.";
                                            }
                                        }

                                        ConditionalTradingLogic.EditTrade(incoming);
                                        notification.NotificTo = incoming.Buyer;
                                        notification = NotificationLogic.NewNotific(notification);
                                        GetClient(incoming.Buyer).SendNotification(notification);
                                    }

                                        // Trade has no owner.
                                    else
                                    {
                                        if (incoming.AssetId != 0)
                                        {
                                            int spareShare = 100;

                                            foreach (AssetInventoryModel a in InventoryLogic.GetAllAssetInventories())
                                            {
                                                if (a.AssetId == incoming.AssetId)
                                                    spareShare -= a.Share;
                                            }

                                            if (incoming.Amount <= spareShare)
                                            {
                                                List<AssetInventoryModel> ai =
                                                    ConditionalTradingLogic.TradeAsset(incoming);

                                                foreach (HandleClinet client in GetClients())
                                                {
                                                    foreach (AssetInventoryModel assetInventory in ai)
                                                    {
                                                        client.SendAssetInventory(assetInventory);
                                                        //Thread.Sleep(100);
                                                    }
                                                }
                                            }

                                            else
                                            {
                                                string text = "It's no longer possible to buy " + incoming.Amount +
                                                              "% of " +
                                                              incoming.PurcaseName + "!";
                                                var notification = new NotificationModel(0, incoming.Buyer,
                                                                                         "Trade Invalidated!",
                                                                                         text, _timeStamp);
                                                SendNotification(notification);
                                            }
                                        }

                                        else
                                        {
                                            List<ItemInventoryModel> ii = ConditionalTradingLogic.TradeItem(incoming);

                                            foreach (HandleClinet client in GetClients())
                                            {
                                                foreach (ItemInventoryModel itemInventory in ii)
                                                {
                                                    client.SendItemInventory(itemInventory);
                                                    //Thread.Sleep(100);
                                                }
                                            }
                                        }
                                    }
                                }

                                    //  Trade has expired.
                                else if (incoming.Accept == 3)
                                {
                                    ConditionalTradingLogic.EditTrade(incoming);
                                    string text;

                                    if (incoming.AssetId != 0)
                                    {
                                        text = "The " + InventoryLogic.GetAssetById(incoming.AssetId).Name +
                                               " trade between you and " +
                                               MessagingLogic.GetTeamById(incoming.Owner).Name +
                                               " has expired because they didn't respond in time.";
                                    }

                                    else
                                    {
                                        text = "The " + InventoryLogic.GetItemById(incoming.ItemId).Name +
                                               " trade between you and " +
                                               MessagingLogic.GetTeamById(incoming.Owner).Name +
                                               " has expired because they didn't respond in time.";
                                    }

                                    var notification = new NotificationModel(0, incoming.Buyer, "Trade expired!", text,
                                                                             _timeStamp);
                                    notification = NotificationLogic.NewNotific(notification);
                                    GetClient(incoming.Buyer).SendNotification(notification);
                                }
                            }

                            else if (rootText == "Groupnumber")
                            {
                                int gNr = RecieveGroupNumber(doc);

                                if (GroupNumberTaken(gNr))
                                {
                                    //  Informs the client that another group has claimed the groupnumber
                                    SendGroupnumber(0);
                                }

                                else
                                {
                                    // Initilizes the client
                                    Groupnumber = gNr;
                                    Console.WriteLine(" >> " + "Client " + Groupnumber + " initialized!");
                                    SendGroupnumber(Groupnumber);
                                    Thread.Sleep(100);
                                    SendAssetList(InventoryLogic.GetAllAssets());
                                    Thread.Sleep(100);
                                    SendAssetInventoryList(InventoryLogic.GetAllAssetInventories());
                                    Thread.Sleep(100);
                                    SendItemList(InventoryLogic.GetAllItems());
                                    Thread.Sleep(100);
                                    SendItemInventoryList(InventoryLogic.GetAllItemInventories());
                                    Thread.Sleep(100);
                                    SendMessageList(MessagingLogic.GetMessages(Groupnumber));
                                    Thread.Sleep(100);
                                    SendNotificationList(NotificationLogic.GetNotifications(Groupnumber));
                                    //SendTradesList(ConditionalTradingLogic.GetTrades(Groupnumber));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Attempts to write to client. Disconnects client if not possible.
        /// </summary>
        /// <param name="write"></param>
        private void WriteToClient(TextWriter write)
        {
            if (Disconnected)
                return;

            try
            {
                var writer = new StreamWriter(_clientSocket.GetStream());
                writer.WriteLine(write);
                writer.Flush();
                Thread.Sleep(100);
            }

            catch (IOException)
            {
                Disconnected = true;
            }

            catch (InvalidOperationException)
            {
                Disconnected = true;
            }

            finally
            {
                if (Disconnected)
                {
                    Console.WriteLine(" >> " + "Client " + Groupnumber + " disconnected!");
                    Groupnumber = 0;

                    Server.Pause = true;
                    Console.WriteLine(" >> " + "Game is paused!");
                    Server.Time = Server.Clock.GameTime;
                    foreach (HandleClinet recipient in GetClients())
                    {
                        recipient.SendPauseState(Server.Pause);
                    }
                }
            }
        }

        /// <summary>
        ///     Sends message
        /// </summary>
        /// <param name="incoming">Message</param>
        private void SendMessage(MessageModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Message");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("FromID", incoming.FromId.ToString());
                    newXml.WriteElementString("ToID", incoming.ToId.ToString());
                    newXml.WriteElementString("FromName", incoming.FromName.ToString());
                    newXml.WriteElementString("ToName", incoming.ToName.ToString());
                    newXml.WriteElementString("ParentID", incoming.ParentId.ToString());
                    newXml.WriteElementString("Subject", incoming.Subject);
                    newXml.WriteElementString("Text", incoming.Text);
                    newXml.WriteElementString("Time", incoming.Time.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves message
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Message</returns>
        private static MessageModel RecieveMessage(XmlDocument doc)
        {
            var recieveMessage = new MessageModel(0, 0, 0, 0, null, null, DateTime.Now);

            XmlElement root = doc["Message"];
            string id = root["ID"].InnerText;
            string fId = root["FromID"].InnerText;
            string tId = root["ToID"].InnerText;
            string fName = root["FromName"].InnerText;
            string tName = root["ToName"].InnerText;
            string pId = root["ParentID"].InnerText;
            string subject = root["Subject"].InnerText;
            string text = root["Text"].InnerText;
            string time = root["Time"].InnerText;

            recieveMessage.Id = Int32.Parse(id);
            recieveMessage.FromId = Int32.Parse(fId);
            recieveMessage.ToId = Int32.Parse(tId);
            recieveMessage.FromName = fName;
            recieveMessage.ToName = tName;
            recieveMessage.ParentId = Int32.Parse(pId);
            recieveMessage.Subject = subject;
            recieveMessage.Text = text;
            recieveMessage.Time = DateTime.Parse(time);

            return recieveMessage;
        }

        /// <summary>
        ///     Sends notification
        /// </summary>
        /// <param name="incoming">Notification</param>
        public void SendNotification(NotificationModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Notification");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("ToID", incoming.NotificTo.ToString());
                    newXml.WriteElementString("Subject", incoming.Subject);
                    newXml.WriteElementString("Text", incoming.Text);
                    newXml.WriteElementString("Time", incoming.Time.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves notification
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Notifiation</returns>
        private static NotificationModel RecieveNotification(XmlDocument doc)
        {
            var recieveNotification = new NotificationModel(0, 0, null, null, DateTime.Now);
            XmlElement root = doc["Notification"];
            string id = root["ID"].InnerText;
            string tId = root["ToID"].InnerText;
            string subject = root["Subject"].InnerText;
            string text = root["Text"].InnerText;
            string time = root["Time"].InnerText;

            recieveNotification.Id = Int32.Parse(id);
            recieveNotification.NotificTo = Int32.Parse(tId);
            recieveNotification.Subject = subject;
            recieveNotification.Text = text;
            recieveNotification.Time = DateTime.Parse(time);

            return recieveNotification;
        }

        /// <summary>
        ///     Sends asset
        /// </summary>
        /// <param name="incoming">Asset</param>
        public void SendAsset(AssetModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Asset");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("Name", incoming.Name);
                    newXml.WriteElementString("Price", incoming.Price.ToString());
                    newXml.WriteElementString("Description", incoming.Description);
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends asset inventory
        /// </summary>
        /// <param name="incoming">Asset inventory</param>
        private void SendAssetInventory(AssetInventoryModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("AssetInventory");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("AssetID", incoming.AssetId.ToString());
                    newXml.WriteElementString("GroupID", incoming.GroupId.ToString());
                    newXml.WriteElementString("Share", incoming.Share.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves asset
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Asset</returns>
        private static AssetModel RecieveAsset(XmlDocument doc)
        {
            var recieveAsset = new AssetModel(0, null, 0, null);
            XmlElement root = doc["Asset"];
            string id = root["ID"].InnerText;
            string name = root["Name"].InnerText;
            string price = root["Price"].InnerText;
            string description = root["Description"].InnerText;

            recieveAsset.Id = Int32.Parse(id);
            recieveAsset.Name = name;
            recieveAsset.Price = Int32.Parse(price);
            recieveAsset.Description = description;

            return recieveAsset;
        }

        /// <summary>
        ///     Sends item
        /// </summary>
        /// <param name="incoming">Item</param>
        public void SendItem(ItemModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Item");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("Name", incoming.Name);
                    newXml.WriteElementString("Price", incoming.Price.ToString());
                    newXml.WriteElementString("Description", incoming.Description);
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends item inventory
        /// </summary>
        /// <param name="incoming">Item inventory</param>
        private void SendItemInventory(ItemInventoryModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("ItemInventory");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("GroupID", incoming.GroupId.ToString());
                    newXml.WriteElementString("ItemID", incoming.ItemId.ToString());
                    newXml.WriteElementString("Quantity", incoming.Quantity.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves item
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Item</returns>
        private static ItemModel RecieveItem(XmlDocument doc)
        {
            var recieveItem = new ItemModel(0, null, 0, null);
            XmlElement root = doc["Item"];
            string id = root["ID"].InnerText;
            string name = root["Name"].InnerText;
            string price = root["Price"].InnerText;
            string description = root["Description"].InnerText;

            recieveItem.Id = Int32.Parse(id);
            recieveItem.Name = name;
            recieveItem.Price = Int32.Parse(price);
            recieveItem.Description = description;

            return recieveItem;
        }

        /// <summary>
        ///     Sends trade
        /// </summary>
        /// <param name="incoming">Trade</param>
        private void SendTrading(TradingModel incoming) //Sends a message to a client.
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Trading");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("OwnerID", incoming.Owner.ToString());
                    newXml.WriteElementString("BuyerID", incoming.Buyer.ToString());
                    newXml.WriteElementString("BuyerName", incoming.BuyerName.ToString());
                    newXml.WriteElementString("AssetID", incoming.AssetId.ToString());
                    newXml.WriteElementString("ItemID", incoming.ItemId.ToString());
                    newXml.WriteElementString("Price", incoming.Price.ToString());
                    newXml.WriteElementString("Amount", incoming.Amount.ToString());
                    newXml.WriteElementString("Text", incoming.Text);
                    newXml.WriteElementString("Time", incoming.Time.ToString());
                    newXml.WriteElementString("Accept", incoming.Accept.ToString());
                    newXml.WriteElementString("PurcaseName", incoming.PurcaseName);
                    newXml.WriteElementString("TotalPrice", incoming.TotalPrice.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves trade
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Trade</returns>
        private static TradingModel RecieveTrading(XmlDocument doc) //Recieves a message from a client.
        {
            var recieveTrading = new TradingModel();

            XmlElement root = doc["Trading"];
            string id = root["ID"].InnerText;
            string oId = root["OwnerID"].InnerText;
            string bId = root["BuyerID"].InnerText;
            string bName = root["BuyerName"].InnerText;
            string aId = root["AssetID"].InnerText;
            string iId = root["ItemID"].InnerText;
            string price = root["Price"].InnerText;
            string amount = root["Amount"].InnerText;
            string text = root["Text"].InnerText;
            string time = root["Time"].InnerText;
            string accept = root["Accept"].InnerText;
            string purcasename = root["PurcaseName"].InnerText;
            string totalprice = root["TotalPrice"].InnerText;

            recieveTrading.Id = Int32.Parse(id);
            recieveTrading.Owner = Int32.Parse(oId);
            recieveTrading.Buyer = Int32.Parse(bId);
            recieveTrading.BuyerName = bName;
            recieveTrading.AssetId = Int32.Parse(aId);
            recieveTrading.ItemId = Int32.Parse(iId);
            recieveTrading.Price = double.Parse(price);
            recieveTrading.Amount = Int32.Parse(amount);
            recieveTrading.Text = text;
            recieveTrading.Time = Convert.ToDateTime(time);
            recieveTrading.Accept = Int32.Parse(accept);
            recieveTrading.PurcaseName = purcasename;
            recieveTrading.TotalPrice = double.Parse(totalprice);

            return recieveTrading;
        }

        /// <summary>
        ///     Sends time
        /// </summary>
        /// <param name="time">Time</param>
        private void SendTimeStamp(DateTime time)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Timestamp");
                    newXml.WriteElementString("Time", time.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends cash values
        /// </summary>
        /// <param name="list">Cash values</param>
        private void SendGroupCash(List<CashPerTeamModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Cashlist");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("Cash", list[i].Cash.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends groupnumber
        /// </summary>
        /// <param name="groupNumber">Groupnumber</param>
        private void SendGroupnumber(int groupNumber)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Groupnumber");
                    newXml.WriteElementString("Number", groupNumber.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Recieves groupnumber
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Groupnumber</returns>
        private int RecieveGroupNumber(XmlDocument doc)
        {
            XmlElement root = doc["Groupnumber"];
            int retur = Int32.Parse(root["Number"].InnerText);
            return retur;
        }

        /// <summary>
        ///     Sends pause state
        /// </summary>
        /// <param name="pause">Pause state</param>
        public void SendPauseState(bool pause)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Pause");
                    newXml.WriteElementString("Bool", pause.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }
                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends assets
        /// </summary>
        /// <param name="list">Assets</param>
        private void SendAssetList(List<AssetModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("AssetList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("Name", list[i].Name);
                        newXml.WriteElementString("Price", list[i].Price.ToString());
                        newXml.WriteElementString("Description", list[i].Description);
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends asset inventories
        /// </summary>
        /// <param name="list">Asset inventories</param>
        private void SendAssetInventoryList(List<AssetInventoryModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("AssetInventoryList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("AssetID", list[i].AssetId.ToString());
                        newXml.WriteElementString("GroupID", list[i].GroupId.ToString());
                        newXml.WriteElementString("Share", list[i].Share.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends items
        /// </summary>
        /// <param name="list">Items</param>
        private void SendItemList(List<ItemModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("ItemList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("Name", list[i].Name);
                        newXml.WriteElementString("Price", list[i].Price.ToString());
                        newXml.WriteElementString("Description", list[i].Description);
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends item inventories
        /// </summary>
        /// <param name="list">Item inventories</param>
        private void SendItemInventoryList(List<ItemInventoryModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("ItemInventoryList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("GroupID", list[i].GroupId.ToString());
                        newXml.WriteElementString("ItemID", list[i].ItemId.ToString());
                        newXml.WriteElementString("Quantity", list[i].Quantity.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends teams
        /// </summary>
        /// <param name="list">Teams</param>
        private void SendTeamList(List<TeamModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("GroupList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("Name", list[i].Name);
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends messages
        /// </summary>
        /// <param name="list">Messages</param>
        public void SendMessageList(List<MessageModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("MessageList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("FromID", list[i].FromId.ToString());
                        newXml.WriteElementString("ToID", list[i].ToId.ToString());
                        newXml.WriteElementString("FromName", list[i].FromName.ToString());
                        newXml.WriteElementString("ToName", list[i].ToName.ToString());
                        newXml.WriteElementString("ParentID", list[i].ParentId.ToString());
                        newXml.WriteElementString("Subject", list[i].Subject);
                        newXml.WriteElementString("Text", list[i].Text);
                        newXml.WriteElementString("Time", list[i].Time.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends notifications
        /// </summary>
        /// <param name="list">Messages</param>
        public void SendNotificationList(List<NotificationModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("NotificationList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("ToID", list[i].NotificTo.ToString());
                        newXml.WriteElementString("Subject", list[i].Subject);
                        newXml.WriteElementString("Text", list[i].Text);
                        newXml.WriteElementString("Time", list[i].Time.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }

        /// <summary>
        ///     Sends trades
        /// </summary>
        /// <param name="list">Trades</param>
        public void SendTradesList(List<TradingModel> list)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("TradingList");
                    newXml.WriteElementString("Count", list.Count.ToString());


                    for (int i = 0; i < list.Count; i++)
                    {
                        newXml.WriteStartElement("Number" + i);
                        newXml.WriteElementString("ID", list[i].Id.ToString());
                        newXml.WriteElementString("OwnerID", list[i].Owner.ToString());
                        newXml.WriteElementString("BuyerID", list[i].Buyer.ToString());
                        newXml.WriteElementString("BuyerName", list[i].BuyerName.ToString());
                        newXml.WriteElementString("AssetID", list[i].AssetId.ToString());
                        newXml.WriteElementString("ItemID", list[i].ItemId.ToString());
                        newXml.WriteElementString("Price", list[i].Price.ToString());
                        newXml.WriteElementString("Amount", list[i].Amount.ToString());
                        newXml.WriteElementString("Text", list[i].Text);
                        newXml.WriteElementString("Time", list[i].Time.ToString());
                        newXml.WriteElementString("Accept", list[i].Accept.ToString());
                        newXml.WriteElementString("PurcaseName", list[i].PurcaseName);
                        newXml.WriteElementString("TotalPrice", list[i].TotalPrice.ToString());
                        newXml.WriteEndElement();
                    }
                    newXml.WriteEndElement();
                    newXml.WriteWhitespace("\n");
                    newXml.WriteEndDocument();
                }

                WriteToClient(write);
            }
        }
    }
}