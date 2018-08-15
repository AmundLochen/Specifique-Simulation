using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Model;

namespace DAL
{
    public class ValueDal
    {
        private static readonly DBlinqDataContext Db = new DBlinqDataContext();
        private static readonly InventoryDal InventoryDal = new InventoryDal();

        public TimeModel GetTimeValues()
        {
            TimeModel timeValues = (from tv in Db.gameTimes
                                    select new TimeModel
                                        {
                                            CurrentTime = (DateTime) tv.currentTime,
                                            EndTime = (DateTime) tv.endTime
                                        }).ToList().Last();

            return timeValues;
        }

        public void UpdateCurrentTime(DateTime rt)
        {
            IQueryable<gameTime> query =
                from t in Db.gameTimes
                select t;
            foreach (gameTime time in query)
            {
                time.currentTime = rt;
            }

            try
            {
                Db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Increases or decreases a team's cash
        /// </summary>
        /// <param name="team">Team id</param>
        /// <param name="cash">Negative or positive cash</param>
        public void IncreaseOrDecreaseCash(int team, double cash)
        {
            cashPerTeam test = Db.cashPerTeams.FirstOrDefault(m => m.teamId == team);

            if (test != null) test.cash += cash;
            try
            {
                Db.SubmitChanges();
            }

            catch (ChangeConflictException)
            {
                Console.WriteLine("ChangeConflictException!");
                foreach (ObjectChangeConflict occ in Db.ChangeConflicts)
                {
                    // All database values overwrite current values.
                    occ.Resolve(RefreshMode.KeepChanges);
                }
            }

            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Changes an item's price
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="newPrice">New price</param>
        public void EditItemPrice(int id, double newPrice)
        {
            IQueryable<item> item =
                from i in Db.items
                where i.Id == id
                select i;
            foreach (item i in item)
            {
                i.itemPrice = newPrice;
            }
            try
            {
                Db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Changes an asset's price
        /// </summary>
        /// <param name="id">Asset id</param>
        /// <param name="newPrice">New price</param>
        public void EditAssetPrice(int id, double newPrice)
        {
            IQueryable<asset> asset =
                from a in Db.assets
                where a.Id == id
                select a;
            foreach (asset a in asset)
            {
                a.assetPrice = newPrice;
            }
            try
            {
                Db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Gets a team's current cash
        /// </summary>
        /// <param name="team">Team id</param>
        /// <returns>Cash</returns>
        private static double GetCurrentCash(int team)
        {
            double? first = (from c in Db.cashPerTeams
                             where c.teamId == team
                             select c.cash).ToList().First();
            if (first != null)
            {
                var cash = (double) first;
                return cash;
            }
            return 0;
        }

        /// <summary>
        ///     Gets a team's earnings from all assets
        /// </summary>
        /// <param name="team">Team id</param>
        /// <returns>Earnings</returns>
        private static double GetAssetEarnings(int team)
        {
            var assetInfo = new double[2];
            var assets = new List<double[]>();
            double assetEarnings = 0;
            IQueryable<assetInventory> assetInv =
                from a in Db.assetInventories
                where a.teamId == team
                select a; // finner alle assets laget team eier
            foreach (assetInventory a in assetInv)
            {
                // her henter den ut av resultatet a. skal hente ut assetId til asseten gruppa eier og hvor stor share gruppa eier og legge i list
                assetInfo[0] = a.assetId;
                if (a.share != null) assetInfo[1] = (double) a.share;
                assets.Add(assetInfo);
            }
            List<AssetValueModel> assetVal =
                (from av in Db.assetValues
                 select new AssetValueModel
                     {
                         Id = av.Id,
                         AssetId = av.assetId,
                         TaxCost = (double) av.taxCost,
                         OperationExpenses = (double) av.operatingExpenses,
                         IncomePerAsset = (double) av.incomePerAsset
                     }).ToList();
            // nå har du lista med assetId og share. nå skal du sjekke i assetvalues hvor mye du har tjent og tapt på asseten i dag og returnere resultatet
            foreach (var aList in assets)
            {
                foreach (AssetValueModel a in assetVal)
                {
                    if (aList != null && aList[0] == a.AssetId)
                    {
                        AssetModel assetmodel =
                            (from am in Db.assets
                             where am.Id == a.AssetId
                             select new AssetModel
                                 {
                                     Price = (double) am.assetPrice
                                 }).ToList().First();
                        assetEarnings -= a.OperationExpenses*(aList[1]/100);
                        assetEarnings -= (a.TaxCost/100)*assetmodel.Price*(aList[1]/100);
                        assetEarnings += a.IncomePerAsset*(aList[1]/100);
                    }
                }
            }

            return assetEarnings;
        }

        /// <summary>
        ///     Gets a team's earnings from all items
        /// </summary>
        /// <param name="team">Team id</param>
        /// <returns>Earning</returns>
        private double GetItemEarnings(int team)
        {
            var itemInfo = new double[2];
            var items = new List<double[]>();
            double itemEarnings = 0;
            IQueryable<itemInventory> itemInv =
                from i in Db.itemInventories
                where i.teamId == team
                select i; // finner alle assets laget team eier
            foreach (itemInventory a in itemInv)
            {
                // her henter den ut av resultatet a. skal hente ut assetId til asseten gruppa eier og hvor stor share gruppa eier og legge i list
                itemInfo[0] = a.itemId;
                if (a.quantity != null) itemInfo[1] = (double) a.quantity;
                items.Add(itemInfo);
            }
            List<ItemValueModel> itemVal =
                (from iv in Db.itemValues
                 select new ItemValueModel
                     {
                         Id = iv.Id,
                         ItemId = iv.itemId,
                         TaxCost = (double) iv.taxCost,
                         StorageExpenses = (double) iv.storageExpenses,
                         IncomePerItem = (double) iv.incomePerItem
                     }).ToList();
            // nå har du lista med assetId og share. nå skal du sjekke i assetvalues hvor mye du har tjent og tapt på asseten i dag og returnere resultatet
            foreach (var iList in items)
            {
                foreach (ItemValueModel i in itemVal)
                {
                    if (iList != null && iList[0] == i.ItemId)
                    {
                        ItemModel itemmodel =
                            (from im in Db.items
                             where im.Id == i.ItemId
                             select new ItemModel
                                 {
                                     Price = (double) im.itemPrice
                                 }).ToList().First();
                        itemEarnings -= i.StorageExpenses*iList[1];
                        itemEarnings -= (i.TaxCost/100)*itemmodel.Price*iList[1];
                        itemEarnings += i.IncomePerItem*iList[1]*(1 + GetExtraEarningsFromItem(team, i.ItemId));
                    }
                }
            }

            return itemEarnings;
        }

        /// <summary>
        ///     Gets a team's extra earnings from an item
        /// </summary>
        /// <param name="team">Team id</param>
        /// <param name="itemid">Item id</param>
        /// <returns>Extra earnings</returns>
        private static double GetExtraEarningsFromItem(int team, int itemid)
        {
            List<ItemOnAssetValueModel> ioavm =
                (from ioa in Db.itemOnAssetValues
                 where ioa.itemId == itemid
                 select new ItemOnAssetValueModel
                     {
                         AssetId = ioa.assetId,
                         ItemId = ioa.itemId,
                         Ratio = (double) ioa.ratio
                     }).ToList();
            List<AssetInventoryModel> assetInv = InventoryDal.FindAssetInventoryByTeamId(team);
            return (from i in ioavm from a in assetInv where a.AssetId == i.AssetId select i.Ratio).FirstOrDefault();
        }

        /// <summary>
        ///     Updates all cash values
        /// </summary>
        /// <returns>All cash values</returns>
        public List<CashPerTeamModel> UpdateCash()
        {
            var c = new List<CashPerTeamModel>();
            for (int i = 1; i < 5; i++)
            {
                var cptm = new CashPerTeamModel();
                //double cash = getCurrentCash(i); //så mye penger har laget
                double cash = 0;
                cash += GetAssetEarnings(i); //så mye har laget tapt eller tjent på assetene sine
                cash += GetItemEarnings(i); //så mye har laget tapt eller tjent på itemsa sine
                IncreaseOrDecreaseCash(i, cash); //oppdatere cashtabellen i databasen
                double totalCash = GetCurrentCash(i);
                cptm.Id = i;
                cptm.Cash = totalCash;
                c.Add(cptm);
            }

            return c;
        }
    }
}