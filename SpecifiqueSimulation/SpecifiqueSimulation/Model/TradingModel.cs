using System;

namespace Model
{
    public class TradingModel
    {
        public TradingModel(int i, int o, int b, string bn, int ai, int ii, double p, int a, string txt, DateTime tm, int acc,
                            string pur, double tp)
        {
            Id = i;
            Owner = o;
            Buyer = b;
            BuyerName = bn;
            AssetId = ai;
            ItemId = ii;
            Price = p;
            Amount = a;
            Text = txt;
            Time = tm;
            Accept = acc;
            PurcaseName = pur;
            TotalPrice = tp;
        }

        public int Id { get; set; }
        public int Owner { get; set; }
        public int Buyer { get; set; }
        public string BuyerName { get; set; }
        public int AssetId { get; set; }
        public int ItemId { get; set; }
        public double Price { get; set; }

        /// <summary>
        ///     Share or quantity
        /// </summary>
        public int Amount { get; set; }

        public string Text { get; set; }
        public DateTime Time { get; set; }

        /// <summary>
        ///     0 = unread, 1 = declined, 2 = accepted, 3 = expired, 4 = invalidated
        /// </summary>
        public int Accept { get; set; }

        public string PurcaseName { get; set; }
        public double TotalPrice { get; set; }
    }
}