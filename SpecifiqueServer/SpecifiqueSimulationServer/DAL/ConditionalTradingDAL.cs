using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    /// <summary>
    ///     Contains trading dal
    /// </summary>
    public class ConditionalTradingDal
    {
        private static readonly ValueDal ValueDal = new ValueDal();
        private readonly DBlinqDataContext _db = new DBlinqDataContext();
        private readonly InventoryDal _inventoryDal = new InventoryDal();

        /// <summary>
        ///     Adds new trade to DB
        /// </summary>
        /// <param name="tm">Trade</param>
        /// <returns>Trade</returns>
        public TradingModel NewTrade(TradingModel tm)
        {
            var newTrade = new trade
                {
                    ownerId = tm.Owner,
                    buyerId = tm.Buyer,
                    buyerName = tm.BuyerName,
                    assetId = tm.AssetId,
                    itemId = tm.ItemId,
                    price = tm.Price,
                    amount = tm.Amount,
                    tradeText = tm.Text,
                    tradeTime = tm.Time,
                    accept = tm.Accept,
                    purchaseName = tm.PurcaseName,
                    totalPrice = tm.TotalPrice
                };

            _db.trades.InsertOnSubmit(newTrade);

            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            int id = (from t in _db.trades
                      orderby t.Id descending
                      select t.Id).First();

            TradingModel outgoing = GetTradeId(id);
            return outgoing;
        }

        /// <summary>
        ///     Edits trade
        /// </summary>
        /// <param name="tm">Trade</param>
        public void EditTrade(TradingModel tm)
        {
            IQueryable<trade> trade =
                from t in _db.trades
                where t.Id == tm.Id
                select t;
            foreach (trade t in trade)
            {
                t.ownerId = tm.Owner;
                t.buyerId = tm.Buyer;
                t.buyerName = tm.BuyerName;
                t.assetId = tm.AssetId;
                t.itemId = tm.ItemId;
                t.price = tm.Price;
                t.amount = tm.Amount;
                t.tradeText = tm.Text;
                t.tradeTime = tm.Time;
                t.accept = tm.Accept;
                t.purchaseName = tm.PurcaseName;
                t.totalPrice = tm.TotalPrice;
            }
            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Gets trade
        /// </summary>
        /// <param name="id">Trade id</param>
        /// <returns>Trade</returns>
        private TradingModel GetTradeId(int id)
        {
            TradingModel trade = (from trades in _db.trades
                                  where trades.Id == id
                                  select new TradingModel
                                      {
                                          Id = trades.Id,
                                          Owner = trades.ownerId,
                                          Buyer = trades.buyerId,
                                          BuyerName = trades.buyerName,
                                          AssetId = (int) trades.assetId,
                                          ItemId = (int) trades.itemId,
                                          Price = (double) trades.price,
                                          Amount = (int) trades.amount,
                                          Text = trades.tradeText,
                                          Time = (DateTime) trades.tradeTime,
                                          Accept = (int) trades.accept,
                                          PurcaseName = trades.purchaseName,
                                          TotalPrice = (double) trades.totalPrice
                                      }).ToList().Last();

            return trade;
        }

        /// <summary>
        ///     Performs asset trade
        /// </summary>
        /// <param name="tm">Trade</param>
        /// <returns>Changed asset inventories</returns>
        public List<AssetInventoryModel> TradeAsset(TradingModel tm)
        {
            var assetInventories = new List<AssetInventoryModel>();
            var assetInventory = new AssetInventoryModel();

            IQueryable<assetInventory> query =
                (from assetInv in _db.assetInventories
                 where assetInv.assetId == tm.AssetId
                       && assetInv.teamId == tm.Buyer
                 select assetInv);

            if (query.Count() != 0)
            {
                assetInventory result = query.First();
                result.share += tm.Amount;
                if (result.share != null)
                    assetInventory = new AssetInventoryModel(result.Id, tm.AssetId, tm.Buyer, (int) result.share);
            }

            else
            {
                var newAssetInventory = new AssetInventoryModel(0, tm.AssetId, tm.Buyer, tm.Amount);
                assetInventory = _inventoryDal.BuyAsset(newAssetInventory);
            }

            // Submit the changes to the database.
            try
            {
                _db.SubmitChanges();
                assetInventories.Add(assetInventory);
                ValueDal.IncreaseOrDecreaseCash(tm.Buyer, -tm.Price);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            if (tm.Owner != 0)
            {
                query = (from assetInv in _db.assetInventories
                         where assetInv.assetId == tm.AssetId
                               && assetInv.teamId == tm.Owner
                         select assetInv);
                if (query.Count() != 0)
                {
                    assetInventory result = query.First();
                    result.share -= tm.Amount;
                    if (result.share != null)
                        assetInventory = new AssetInventoryModel(result.Id, tm.AssetId, tm.Owner, (int) result.share);
                }

                // Submit the changes to the database.
                try
                {
                    _db.SubmitChanges();
                    assetInventories.Add(assetInventory);
                    ValueDal.IncreaseOrDecreaseCash(tm.Owner, tm.Price);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Provide for exceptions.
                }
            }

            return assetInventories;
        }

        /// <summary>
        ///     Performs item trade
        /// </summary>
        /// <param name="tm">Trade</param>
        /// <returns>Changed item inventories</returns>
        public List<ItemInventoryModel> TradeItem(TradingModel tm)
        {
            var itemInventories = new List<ItemInventoryModel>();
            var itemInventory = new ItemInventoryModel();

            IQueryable<itemInventory> query =
                (from itemInv in _db.itemInventories
                 where itemInv.itemId == tm.ItemId
                       && itemInv.teamId == tm.Buyer
                 select itemInv);

            if (query.Count() != 0)
            {
                itemInventory result = query.First();
                result.quantity += tm.Amount;
                if (result.quantity != null)
                    itemInventory = new ItemInventoryModel(result.Id, tm.Buyer, tm.ItemId, (double) result.quantity);
            }

            else
            {
                itemInventory = new ItemInventoryModel(0, tm.Buyer, tm.ItemId, tm.Amount);
                _inventoryDal.BuyItem(itemInventory);
            }

            // Submit the changes to the database.
            try
            {
                _db.SubmitChanges();
                itemInventories.Add(itemInventory);
                ValueDal.IncreaseOrDecreaseCash(tm.Buyer, -tm.Price);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            if (tm.Owner != 0)
            {
                query = (from itemInv in _db.itemInventories
                         where itemInv.itemId == tm.ItemId
                               && itemInv.teamId == tm.Owner
                         select itemInv);

                if (query.Count() != 0)
                {
                    itemInventory result = query.First();
                    result.quantity -= tm.Amount;
                    if (result.quantity != null)
                        itemInventory = new ItemInventoryModel(result.Id, tm.Buyer, tm.ItemId, (double) result.quantity);
                }

                // Submit the changes to the database.
                try
                {
                    _db.SubmitChanges();
                    itemInventories.Add(itemInventory);
                    ValueDal.IncreaseOrDecreaseCash(tm.Owner, tm.Price);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Provide for exceptions.
                }
            }

            return itemInventories;
        }

        /// <summary>
        ///     Gets all trades for team
        /// </summary>
        /// <param name="owner">Owner</param>
        /// <returns>Trades</returns>
        public List<TradingModel> GetTrades(int owner)
        {
            List<TradingModel> list = (from trades in _db.trades
                                         where trades.ownerId == owner
                                         orderby trades.Id
                                         select new TradingModel
                                         {
                                             Id = trades.Id,
                                             Owner = trades.ownerId,
                                             Buyer = trades.buyerId,
                                             BuyerName = trades.buyerName,
                                             AssetId = (int)trades.assetId,
                                             ItemId = (int)trades.itemId,
                                             Price = (double)trades.price,
                                             Amount = (int)trades.amount,
                                             Text = trades.tradeText,
                                             Time = (DateTime)trades.tradeTime,
                                             Accept = (int)trades.accept,
                                             PurcaseName = trades.purchaseName,
                                             TotalPrice = (double)trades.totalPrice
                                         }).ToList();

            return list;
        }
    }
}