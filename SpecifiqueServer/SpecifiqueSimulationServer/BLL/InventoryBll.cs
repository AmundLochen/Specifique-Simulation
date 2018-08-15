using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    ///     Contains inventory logic
    /// </summary>
    public static class InventoryLogic
    {
        private static readonly InventoryDal InventoryDal = new InventoryDal();

        public static bool NewItemToDb(ItemModel i)
        {
            return InventoryDal.NewItemToDb(i);
        }

        public static void NewAssetToDb(AssetModel a)
        {
            InventoryDal.NewAssetToDb(a);
        }

        public static ItemInventoryModel BuyItem(ItemInventoryModel i)
        {
            return InventoryDal.BuyItem(i);
        }

        public static AssetInventoryModel BuyAsset(AssetInventoryModel a)
        {
            return InventoryDal.BuyAsset(a);
        }

        public static List<ItemInventoryModel> FindItemInventoryByTeamId(int groupid)
        {
            return InventoryDal.FindItemInventoryByTeamId(groupid);
        }

        public static List<AssetInventoryModel> FindAssetInventoryByTeamId(int groupid)
        {
            return InventoryDal.FindAssetInventoryByTeamId(groupid);
        }

        /// <summary>
        ///     Gets all items
        /// </summary>
        /// <returns>All items</returns>
        public static List<ItemModel> GetAllItems()
        {
            return InventoryDal.GetAllItems();
        }

        /// <summary>
        ///     Gets all assets
        /// </summary>
        /// <returns>All assets</returns>
        public static List<AssetModel> GetAllAssets()
        {
            return InventoryDal.GetAllAssets();
        }

        /// <summary>
        ///     Gets asset
        /// </summary>
        /// <param name="id">Asset id</param>
        /// <returns>The asset</returns>
        public static AssetModel GetAssetById(int id)
        {
            return InventoryDal.GetAssetById(id);
        }

        /// <summary>
        ///     Gets item
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>The item</returns>
        public static ItemModel GetItemById(int id)
        {
            return InventoryDal.GetItemById(id);
        }

        /// <summary>
        ///     Gets all item inventories
        /// </summary>
        /// <returns>The item inventories</returns>
        public static List<ItemInventoryModel> GetAllItemInventories()
        {
            return InventoryDal.GetAllItemInventories();
        }

        /// <summary>
        ///     Gets all asset inventories
        /// </summary>
        /// <returns>The asset inventories</returns>
        public static List<AssetInventoryModel> GetAllAssetInventories()
        {
            return InventoryDal.GetAllAssetInventories();
        }

        /// <summary>
        ///     Gets asset inventory
        /// </summary>
        /// <param name="assetId">The asset id</param>
        /// <param name="groupId">The group id</param>
        /// <returns>The asset inventory</returns>
        public static AssetInventoryModel GetAssetInventory(int assetId, int groupId)
        {
            return InventoryDal.GetAssetInventory(assetId, groupId);
        }

        /// <summary>
        ///     Gets item inventory
        /// </summary>
        /// <param name="itemId">The item id</param>
        /// <param name="groupId">The group id</param>
        /// <returns>The item inventory</returns>
        public static ItemInventoryModel GetItemInventory(int itemId, int groupId)
        {
            return InventoryDal.GetItemInventory(itemId, groupId);
        }
    }
}