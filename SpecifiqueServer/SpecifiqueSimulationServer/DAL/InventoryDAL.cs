using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    /// <summary>
    ///     Contains inventory dal
    /// </summary>
    public class InventoryDal
    {
        private readonly DBlinqDataContext _db = new DBlinqDataContext();

        public bool NewItemToDb(ItemModel i)
        {
            var newItem = new item {itemName = i.Name, itemPrice = i.Price, itemDescription = i.Description};
            //newItem.Id = i.id;

            _db.items.InsertOnSubmit(newItem);
            _db.SubmitChanges();

            return true;
        }

        public bool NewAssetToDb(AssetModel a)
        {
            var newAsset = new asset {assetName = a.Name, assetPrice = a.Price, assetDescription = a.Description};
            //newAsset.Id = a.id;

            _db.assets.InsertOnSubmit(newAsset);
            _db.SubmitChanges();

            return true;
        }

        /// <summary>
        ///     Adds item inventory to DB
        /// </summary>
        /// <param name="i">Item inventory</param>
        /// <returns>Item inventory</returns>
        public ItemInventoryModel BuyItem(ItemInventoryModel i)
        {
            var newItemInventory = new itemInventory {itemId = i.ItemId, teamId = i.GroupId, quantity = i.Quantity};


            _db.itemInventories.InsertOnSubmit(newItemInventory);
            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            ItemInventoryModel outgoing = GetItemInventory(i.ItemId, i.GroupId);
            return outgoing;
        }

        /// <summary>
        ///     Gets item inventory
        /// </summary>
        /// <param name="iId">Item id</param>
        /// <param name="grp">Group id</param>
        /// <returns>Item inventory</returns>
        public ItemInventoryModel GetItemInventory(int iId, int grp)
        {
            ItemInventoryModel iInvM = (from iinv in _db.itemInventories
                                        where iinv.itemId == iId && iinv.teamId == grp
                                        select new ItemInventoryModel
                                            {
                                                Id = iinv.Id,
                                                ItemId = iinv.itemId,
                                                GroupId = iinv.teamId,
                                                Quantity = (double) iinv.quantity
                                            }).ToList().Last();
            return iInvM;
        }

        /// <summary>
        ///     Adds new asset inventory to DB
        /// </summary>
        /// <param name="a">Asset inventory</param>
        /// <returns>Asset inventory</returns>
        public AssetInventoryModel BuyAsset(AssetInventoryModel a)
        {
            var newAssetInventory = new assetInventory {assetId = a.AssetId, teamId = a.GroupId, share = a.Share};

            _db.assetInventories.InsertOnSubmit(newAssetInventory);
            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            AssetInventoryModel outgoing = GetAssetInventory(a.AssetId, a.GroupId);
            return outgoing;
        }

        /// <summary>
        ///     Gets asset inventory
        /// </summary>
        /// <param name="aId">Asset id</param>
        /// <param name="grp">Group id</param>
        /// <returns>Asset inventory</returns>
        public AssetInventoryModel GetAssetInventory(int aId, int grp)
        {
            AssetInventoryModel aInvM = (from ainv in _db.assetInventories
                                         where ainv.assetId == aId && ainv.teamId == grp
                                         select new AssetInventoryModel
                                             {
                                                 Id = ainv.Id,
                                                 AssetId = ainv.assetId,
                                                 GroupId = ainv.teamId,
                                                 Share = (int) ainv.share
                                             }).ToList().Last();
            return aInvM;
        }

        /// <summary>
        ///     Gets a team's item inventories
        /// </summary>
        /// <param name="groupid">Team id</param>
        /// <returns>Item inventories</returns>
        public List<ItemInventoryModel> FindItemInventoryByTeamId(int groupid)
        {
            List<ItemInventoryModel> ivm = (from itemInv in _db.itemInventories
                                            where itemInv.teamId == groupid && itemInv.quantity != 0
                                            select new ItemInventoryModel
                                                {
                                                    Id = itemInv.Id,
                                                    ItemId = itemInv.Id,
                                                    GroupId = itemInv.teamId,
                                                    Quantity = (double) itemInv.quantity
                                                }).ToList();
            return ivm;
        }

        /// <summary>
        ///     Gets a team's asset inventories
        /// </summary>
        /// <param name="groupid">Team id</param>
        /// <returns>Asset inventories</returns>
        public List<AssetInventoryModel> FindAssetInventoryByTeamId(int groupid)
        {
            List<AssetInventoryModel> avm = (from assetInv in _db.assetInventories
                                             where assetInv.teamId == groupid && assetInv.share != 0
                                             select new AssetInventoryModel
                                                 {
                                                     Id = assetInv.Id,
                                                     AssetId = assetInv.assetId,
                                                     GroupId = assetInv.teamId,
                                                     Share = (int) assetInv.share
                                                 }).ToList();
            return avm;
        }

        /// <summary>
        ///     Gets all items
        /// </summary>
        /// <returns>Items</returns>
        public List<ItemModel> GetAllItems()
        {
            List<ItemModel> im = (from i in _db.items
                                  select new ItemModel
                                      {
                                          Id = i.Id,
                                          Name = i.itemName,
                                          Price = (double) i.itemPrice,
                                          Description = i.itemDescription
                                      }).ToList();
            return im;
        }

        /// <summary>
        ///     Gets all assets
        /// </summary>
        /// <returns>Assets</returns>
        public List<AssetModel> GetAllAssets()
        {
            List<AssetModel> am = (from a in _db.assets
                                   select new AssetModel
                                       {
                                           Id = a.Id,
                                           Name = a.assetName,
                                           Price = (double) a.assetPrice,
                                           Description = a.assetDescription
                                       }).ToList();
            return am;
        }

        /// <summary>
        ///     Gets asset
        /// </summary>
        /// <param name="id">Asset id</param>
        /// <returns>Asset</returns>
        public AssetModel GetAssetById(int id)
        {
            AssetModel am = null;

            try
            {
                am = (from a in _db.assets
                      where a.Id == id
                      select new AssetModel
                          {
                              Id = a.Id,
                              Name = a.assetName,
                              Price = (double) a.assetPrice,
                              Description = a.assetDescription
                          }).ToList().Last();
            }

            catch (Exception)
            {
            }

            return am;
        }

        /// <summary>
        ///     Gets item
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>Item</returns>
        public ItemModel GetItemById(int id)
        {
            ItemModel item = null;

            try
            {
                item = (from i in _db.items
                        where i.Id == id
                        select new ItemModel
                            {
                                Id = i.Id,
                                Name = i.itemName,
                                Price = (double) i.itemPrice,
                                Description = i.itemDescription
                            }).ToList().Last();
            }

            catch (Exception)
            {
            }

            return item;
        }

        /// <summary>
        ///     Gets all item inventories
        /// </summary>
        /// <returns>Item inventories</returns>
        public List<ItemInventoryModel> GetAllItemInventories()
        {
            List<ItemInventoryModel> ivm = (from i in _db.itemInventories
                                            where i.quantity != 0
                                            select new ItemInventoryModel
                                                {
                                                    Id = i.Id,
                                                    GroupId = i.teamId,
                                                    ItemId = i.itemId,
                                                    Quantity = (double) i.quantity
                                                }).ToList();
            return ivm;
        }

        /// <summary>
        ///     Gets all asset inventories
        /// </summary>
        /// <returns>Asset inventories</returns>
        public List<AssetInventoryModel> GetAllAssetInventories()
        {
            List<AssetInventoryModel> avm = (from a in _db.assetInventories
                                             where a.share != 0
                                             select new AssetInventoryModel
                                                 {
                                                     Id = a.Id,
                                                     AssetId = a.assetId,
                                                     GroupId = a.teamId,
                                                     Share = (int) a.share
                                                 }).ToList();
            return avm;
        }
    }
}