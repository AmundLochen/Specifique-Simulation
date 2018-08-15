using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    ///     Contains trading logic
    /// </summary>
    public static class ConditionalTradingLogic
    {
        private static readonly ConditionalTradingDal TradingDal = new ConditionalTradingDal();

        /// <summary>
        ///     Adds new trade to DB
        /// </summary>
        /// <param name="tm">The trade</param>
        /// <returns>The trade</returns>
        public static TradingModel NewTrade(TradingModel tm)
        {
            return TradingDal.NewTrade(tm);
        }

        /// <summary>
        ///     Edits the trade in DB
        /// </summary>
        /// <param name="tm">The Trade</param>
        public static void EditTrade(TradingModel tm)
        {
            TradingDal.EditTrade(tm);
        }

        /// <summary>
        ///     Performs the asset trade
        /// </summary>
        /// <param name="tm">The Trade</param>
        /// <returns>The changed asset inventories</returns>
        public static List<AssetInventoryModel> TradeAsset(TradingModel tm)
        {
            return TradingDal.TradeAsset(tm);
        }

        /// <summary>
        ///     Performs the asset trade
        /// </summary>
        /// <param name="tm">The Trade</param>
        /// <returns>The changed item inventories</returns>
        public static List<ItemInventoryModel> TradeItem(TradingModel tm)
        {
            return TradingDal.TradeItem(tm);
        }

        /// <summary>
        ///     Gets all trades for team
        /// </summary>
        /// <param name="owner">Owner</param>
        /// <returns>Messages</returns>
        public static List<TradingModel> GetTrades(int owner)
        {
            return TradingDal.GetTrades(owner);
        }
    }
}