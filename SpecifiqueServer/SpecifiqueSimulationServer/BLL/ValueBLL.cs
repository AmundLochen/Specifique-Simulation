using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    ///     Contains value logic
    /// </summary>
    public static class ValueLogic
    {
        private static readonly ValueDal ValueDal = new ValueDal();

        /// <summary>
        ///     Gets time values
        /// </summary>
        /// <returns>Time values</returns>
        public static TimeModel GetTimeValues()
        {
            return ValueDal.GetTimeValues();
        }

        /// <summary>
        ///     Updates time in DB
        /// </summary>
        /// <param name="t">Time values</param>
        public static void UpdateCurrentTime(DateTime t)
        {
            ValueDal.UpdateCurrentTime(t);
        }

        public static void IncreaseOrDecreaseCash(int team, double cash)
        {
            ValueDal.IncreaseOrDecreaseCash(team, cash);
        }

        public static void EditItemPrice(int id, double newPrice)
        {
            ValueDal.EditItemPrice(id, newPrice);
        }

        public static void EditAssetPrice(int id, double newPrice)
        {
            ValueDal.EditAssetPrice(id, newPrice);
        }

        /// <summary>
        ///     Gets cash for all teams
        /// </summary>
        /// <returns>Cash for all teams</returns>
        public static List<CashPerTeamModel> UpdateCash()
        {
            return ValueDal.UpdateCash();
        }
    }
}