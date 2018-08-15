using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Model;

namespace ClientLibrary
{
    public class Clnt
    {
        private readonly TcpClient _clientSocket;
        private readonly string _ip;
        private readonly int _portNr;

        /// <summary>
        ///     Establishes connection to server
        /// </summary>
        /// <param name="ipAddress">IP adress</param>
        /// <param name="portNumber">Port number</param>
        public Clnt(string ipAddress, int portNumber)
        {
            _clientSocket = new TcpClient();

            try
            {
                _clientSocket.Connect(ipAddress, portNumber);
                _ip = ipAddress;
                _portNr = portNumber;
            }
            catch (Exception)
            {
                RecieveObject(1, null);
                MessageBox.Show(
                    "Server not found. Please make sure you inserted the correct IP and portnumber and try again.",
                    "Network Error");
                //  Environment.Exit(1);
            }

            var chatThread = new Thread(DoChat);
            chatThread.IsBackground = true;
            chatThread.Start();
        }

        /// <summary>
        ///     Fired when recieving an object from server
        /// </summary>
        public event EventHandler RecievedObject;

        /// <summary>
        ///     Recieves object from server
        /// </summary>
        /// <param name="recieved">Incoming object</param>
        /// <param name="e"></param>
        private void RecieveObject(object recieved, EventArgs e)
        {
            EventHandler handler = RecievedObject;
            if (handler != null)
                handler(recieved, e);
        }

        /// <summary>
        ///     Communcates with server
        /// </summary>
        private void DoChat()
        {
            Thread.Sleep(100);
            while (true)
            {
                Thread.Sleep(10);
                string incomeData;
                TextReader reader = new StreamReader(_clientSocket.GetStream());

                if (_clientSocket.Available > 0)
                {
                    var doc = new XmlDocument();

                    while ((incomeData = reader.ReadLine()) != null && (incomeData.StartsWith("<") && incomeData.EndsWith(">")))
                    {
                        Console.WriteLine(incomeData);
                        doc.LoadXml(incomeData);
                        XmlElement root = doc.DocumentElement;
                        if (root != null)
                        {
                            string rootText = root.Name;

                            if (rootText == "Timestamp")
                            {
                                DateTime s = RecieveTimeStamp(doc);
                                RecieveObject(s, null);
                            }

                            else if (rootText == "Cashlist")
                            {
                                List<CashPerTeamModel> s = RecieveCurrentCash(doc);
                                RecieveObject(s, null);
                            }

                            else if (rootText == "Message")
                            {
                                MessageModel incoming = RecieveMessage(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "Trading")
                            {
                                TradingModel incoming = RecieveTrading(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "Notification")
                            {
                                NotificationModel incoming = RecieveNotification(doc);
                                RecieveObject(incoming, null);
                            }
                            else if (rootText == "Asset")
                            {
                                AssetModel incoming = RecieveAsset(doc);
                                RecieveObject(incoming, null);
                            }
                            else if (rootText == "AssetInventory")
                            {
                                AssetInventoryModel incoming = RecieveAssetInventory(doc);
                                RecieveObject(incoming, null);
                            }
                            else if (rootText == "AssetList")
                            {
                                List<AssetModel> incoming = RecieveAssetList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "AssetInventoryList")
                            {
                                List<AssetInventoryModel> incoming = RecieveAssetInventoryList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "Item")
                            {
                                ItemModel incoming = RecieveItem(doc);
                                RecieveObject(incoming, null);
                            }
                            else if (rootText == "ItemInventory")
                            {
                                ItemInventoryModel incoming = RecieveItemInventory(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "ItemList")
                            {
                                List<ItemModel> incoming = RecieveItemList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "ItemInventoryList")
                            {
                                List<ItemInventoryModel> incoming = RecieveItemInventoryList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "GroupList")
                            {
                                List<GroupModel> incoming = RecieveGroupList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "MessageList")
                            {
                                List<MessageModel> incoming = RecieveMessageList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "NotificationList")
                            {
                                List<NotificationModel> incoming = RecieveNotificationList(doc);
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "TradingList")
                            {
                                List<TradingModel> incoming = RecieveTradingList(doc);
                                //Console.WriteLine("test");
                                RecieveObject(incoming, null);
                            }

                            else if (rootText == "Groupnumber")
                            {
                                int groupNumber = RecieveGroupNumber(doc);
                                RecieveObject(groupNumber, null);
                            }

                            else if (rootText == "Pause")
                            {
                                bool paused = RecievePauseState(doc);
                                RecieveObject(paused, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Sends message
        /// </summary>
        /// <param name="incoming">Message</param>
        public void SendMessage(MessageModel incoming)
        {
            using (TextWriter write = new StringWriter())
            {
                using (XmlWriter newXml = XmlWriter.Create(write))
                {
                    newXml.WriteStartDocument();
                    newXml.WriteStartElement("Message");
                    newXml.WriteElementString("ID", incoming.Id.ToString());
                    newXml.WriteElementString("FromID", incoming.From.ToString());
                    newXml.WriteElementString("ToID", incoming.To.ToString());
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
                var writer = new StreamWriter(_clientSocket.GetStream());
                writer.WriteLine(write);
                writer.Flush();
            }
        }

        /// <summary>
        ///     Recieves message
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private MessageModel RecieveMessage(XmlDocument doc)
        {
            var recieveMessage = new MessageModel(0, 0, 0,null, null, 0, null, null, new DateTime());
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
            recieveMessage.From = Int32.Parse(fId);
            recieveMessage.To = Int32.Parse(tId);
            recieveMessage.FromName = fName;
            recieveMessage.ToName = tName;
            recieveMessage.ParentId = Int32.Parse(pId);
            recieveMessage.Subject = subject;
            recieveMessage.Text = text;
            recieveMessage.Time = Convert.ToDateTime(time);

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
                    newXml.WriteElementString("ToID", incoming.To.ToString());
                    newXml.WriteElementString("Subject", incoming.Subject);
                    newXml.WriteElementString("Text", incoming.Text);
                    newXml.WriteElementString("Time", incoming.Time.ToString());
                    newXml.WriteEndElement();
                    newXml.WriteEndDocument();
                }
                var writer = new StreamWriter(_clientSocket.GetStream());
                writer.WriteLine(write);
                writer.Flush();
            }
        }

        /// <summary>
        ///     Recieves notification
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Notification</returns>
        private NotificationModel RecieveNotification(XmlDocument doc)
        {
            var recieveNotification = new NotificationModel(0, null, null, new DateTime(), 0);
            XmlElement root = doc["Notification"];
            string id = root["ID"].InnerText;
            string tId = root["ToID"].InnerText;
            string subject = root["Subject"].InnerText;
            string text = root["Text"].InnerText;
            string time = root["Time"].InnerText;

            recieveNotification.Id = Int32.Parse(id);
            recieveNotification.To = Int32.Parse(tId);
            recieveNotification.Subject = subject;
            recieveNotification.Text = text;
            recieveNotification.Time = Convert.ToDateTime(time);

            return recieveNotification;
        }

        /// <summary>
        ///     Recieves asset
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Asset</returns>
        private AssetModel RecieveAsset(XmlDocument doc)
        {
            var recieveAsset = new AssetModel(0, null, 0, null);
            XmlElement root = doc["Asset"];
            string id = root["ID"].InnerText;
            string name = root["Name"].InnerText;
            string price = root["Price"].InnerText;
            string description = root["Description"].InnerText;

            recieveAsset.Id = Int32.Parse(id);
            recieveAsset.Name = name;
            recieveAsset.Price = double.Parse(price);
            recieveAsset.Description = description;
            return recieveAsset;
        }

        /// <summary>
        ///     Recieves asset inventory
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Asset inventory</returns>
        private AssetInventoryModel RecieveAssetInventory(XmlDocument doc)
        {
            var recieveAssetInventory = new AssetInventoryModel(0, 0, 0, 0);
            XmlElement root = doc["AssetInventory"];
            string id = root["ID"].InnerText;
            string assetId = root["AssetID"].InnerText;
            string groupId = root["GroupID"].InnerText;
            string share = root["Share"].InnerText;

            recieveAssetInventory.Id = Int32.Parse(id);
            recieveAssetInventory.AssetId = Int32.Parse(assetId);
            recieveAssetInventory.GroupId = Int32.Parse(groupId);
            recieveAssetInventory.Share = Int32.Parse(share);
            return recieveAssetInventory;
        }

        /// <summary>
        ///     Recieves item with new price
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Item</returns>
        private ItemModel RecieveItem(XmlDocument doc)
        {
            var recieveItem = new ItemModel(0, null, 0, null);
            XmlElement root = doc["Item"];
            string id = root["ID"].InnerText;
            string name = root["Name"].InnerText;
            string price = root["Price"].InnerText;
            string description = root["Description"].InnerText;

            recieveItem.Id = Int32.Parse(id);
            recieveItem.Name = name;
            recieveItem.Value = double.Parse(price);
            recieveItem.Description = description;
            return recieveItem;
        }

        /// <summary>
        ///     Recieves item inventory
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Item inventory</returns>
        private ItemInventoryModel RecieveItemInventory(XmlDocument doc)
        {
            var recieveItemInventory = new ItemInventoryModel(0, 0, 0, 0);
            XmlElement root = doc["ItemInventory"];
            string id = root["ID"].InnerText;
            string groupId = root["GroupID"].InnerText;
            string itemId = root["ItemID"].InnerText;
            string quantity = root["Quantity"].InnerText;

            recieveItemInventory.Id = Int32.Parse(id);
            recieveItemInventory.GroupId = Int32.Parse(groupId);
            recieveItemInventory.ItemId = Int32.Parse(itemId);
            recieveItemInventory.Quantity = double.Parse(quantity);
            return recieveItemInventory;
        }

        /// <summary>
        ///     Sends trade
        /// </summary>
        /// <param name="incoming">Trade</param>
        public void SendTrading(TradingModel incoming) //Sends a message to a client.
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
                var writer = new StreamWriter(_clientSocket.GetStream());
                writer.WriteLine(write);
                writer.Flush();
            }
        }

        /// <summary>
        ///     Recieves trade
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Trade</returns>
        private static TradingModel RecieveTrading(XmlDocument doc) //Recieves a message from a client.
        {
            var recieveTrading = new TradingModel(0, 0, 0,"", 0, 0, 0, 0, "", new DateTime(), 0, "", 0);

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
        ///     Sends string
        /// </summary>
        /// <param name="incoming">string</param>
        public void SendString(string incoming)
        {
            NetworkStream stream = _clientSocket.GetStream();
            var enc = new ASCIIEncoding();
            string s = incoming;
            byte[] buffer = enc.GetBytes(s);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        /// <summary>
        ///     Recieves timestamp
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Timestamp</returns>
        private DateTime RecieveTimeStamp(XmlDocument doc)
        {
            XmlElement root = doc["Timestamp"];
            DateTime time = DateTime.Parse(root["Time"].InnerText);
            return time;
        }

        /// <summary>
        ///     Recieves groupnumber
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private int RecieveGroupNumber(XmlDocument doc)
        {
            XmlElement root = doc["Groupnumber"];
            int retur = Int32.Parse(root["Number"].InnerText);
            return retur;
        }

        /// <summary>
        ///     Recieves cash values
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Cash values</returns>
        private List<CashPerTeamModel> RecieveCurrentCash(XmlDocument doc)
        {
            XmlElement root = doc["Cashlist"];
            var list = new List<CashPerTeamModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement cashPerTeam = root["Number" + i];
                string id = cashPerTeam["ID"].InnerText;
                string cash = cashPerTeam["Cash"].InnerText;

                list.Add(new CashPerTeamModel(Int32.Parse(id), double.Parse(cash)));
            }
            return list;
        }

        /// <summary>
        ///     Recieves assets
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Asset</returns>
        private List<AssetModel> RecieveAssetList(XmlDocument doc)
        {
            XmlElement root = doc["AssetList"];
            var list = new List<AssetModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string name = asset["Name"].InnerText;
                string price = asset["Price"].InnerText;
                string description = asset["Description"].InnerText;

                list.Add(new AssetModel(Int32.Parse(id), name, double.Parse(price), description));
            }
            return list;
        }

        /// <summary>
        ///     Recieves asset inventories
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Asset inventories</returns>
        private List<AssetInventoryModel> RecieveAssetInventoryList(XmlDocument doc)
        {
            XmlElement root = doc["AssetInventoryList"];
            var list = new List<AssetInventoryModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string assetId = asset["AssetID"].InnerText;
                string groupId = asset["GroupID"].InnerText;
                string share = asset["Share"].InnerText;

                list.Add(new AssetInventoryModel(Int32.Parse(id), Int32.Parse(assetId), Int32.Parse(groupId),
                                                 Int32.Parse(share)));
            }
            return list;
        }

        /// <summary>
        ///     Recieves items
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Items</returns>
        private List<ItemModel> RecieveItemList(XmlDocument doc)
        {
            XmlElement root = doc["ItemList"];
            var list = new List<ItemModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string name = asset["Name"].InnerText;
                string price = asset["Price"].InnerText;
                string description = asset["Description"].InnerText;

                list.Add(new ItemModel(Int32.Parse(id), name, double.Parse(price), description));
            }
            return list;
        }

        /// <summary>
        ///     Recieves item inventories
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Item inventories</returns>
        private List<ItemInventoryModel> RecieveItemInventoryList(XmlDocument doc)
        {
            XmlElement root = doc["ItemInventoryList"];
            var list = new List<ItemInventoryModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string groupId = asset["GroupID"].InnerText;
                string itemId = asset["ItemID"].InnerText;
                string quantity = asset["Quantity"].InnerText;

                list.Add(new ItemInventoryModel(Int32.Parse(id), Int32.Parse(groupId), Int32.Parse(itemId),
                                                double.Parse(quantity)));
            }
            return list;
        }

        /// <summary>
        ///     Recieves groups
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Groups</returns>
        private List<GroupModel> RecieveGroupList(XmlDocument doc)
        {
            XmlElement root = doc["GroupList"];
            var list = new List<GroupModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string name = asset["Name"].InnerText;

                list.Add(new GroupModel(Int32.Parse(id), name));
            }
            return list;
        }

        /// <summary>
        ///     Recieves messages
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Messages</returns>
        private List<MessageModel> RecieveMessageList(XmlDocument doc)
        {
            XmlElement root = doc["MessageList"];
            var list = new List<MessageModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string fId = asset["FromID"].InnerText;
                string tId = asset["ToID"].InnerText;
                string fName = asset["FromName"].InnerText;
                string tName = asset["ToName"].InnerText;
                string pId = asset["ParentID"].InnerText;
                string subject = asset["Subject"].InnerText;
                string text = asset["Text"].InnerText;
                string time = asset["Time"].InnerText;

                list.Add(new MessageModel(Int32.Parse(id), Int32.Parse(fId), Int32.Parse(tId),fName, tName, Int32.Parse(pId), subject,
                                          text, Convert.ToDateTime(time)));
            }
            return list;
        }

        /// <summary>
        ///     Recieves notifications
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Notifications</returns>
        private List<NotificationModel> RecieveNotificationList(XmlDocument doc)
        {
            XmlElement root = doc["NotificationList"];
            var list = new List<NotificationModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string tId = asset["ToID"].InnerText;
                string subject = asset["Subject"].InnerText;
                string text = asset["Text"].InnerText;
                string time = asset["Time"].InnerText;

                list.Add(new NotificationModel(Int32.Parse(id), subject,
                                               text, Convert.ToDateTime(time), Int32.Parse(tId)));
            }
            return list;
        }

        /// <summary>
        ///     Recieves trades
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Trades</returns>
        private List<TradingModel> RecieveTradingList(XmlDocument doc)
        {
            XmlElement root = doc["TradingList"];
            var list = new List<TradingModel>();
            int count = Int32.Parse(root["Count"].InnerText);
            for (int i = 0; i < count; i++)
            {
                XmlElement asset = root["Number" + i];
                string id = asset["ID"].InnerText;
                string oId = asset["OwnerID"].InnerText;
                string bId = asset["BuyerID"].InnerText;
                string bName = asset["BuyerName"].InnerText;
                string aId = asset["AssetID"].InnerText;
                string iId = asset["ItemID"].InnerText;
                string price = asset["Price"].InnerText;
                string amount = asset["Amount"].InnerText;
                string text = asset["Text"].InnerText;
                string time = asset["Time"].InnerText;
                string accept = asset["Accept"].InnerText;
                string purcasename = asset["PurcaseName"].InnerText;
                string totalprice = asset["TotalPrice"].InnerText;

                list.Add(new TradingModel(Int32.Parse(id), Int32.Parse(oId), Int32.Parse(bId), bName, Int32.Parse(aId), Int32.Parse(iId), double.Parse(price), Int32.Parse(amount), text, Convert.ToDateTime(time), Int32.Parse(accept), "", double.Parse(totalprice)));
            }
            return list;
        }

        /// <summary>
        ///     Sends groupnumber
        /// </summary>
        /// <param name="groupNumber">Groupnumber</param>
        public void SendGroupnumber(int groupNumber)
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

                var writer = new StreamWriter(_clientSocket.GetStream());
                writer.WriteLine(write);
                writer.Flush();
            }
        }

        /// <summary>
        ///     Recieves pause state
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Pause state</returns>
        private bool RecievePauseState(XmlDocument doc)
        {
            XmlElement root = doc["Pause"];
            string pauseState = root["Bool"].InnerText;
            bool retur = bool.Parse(pauseState);
            return retur;
        }
    }
}