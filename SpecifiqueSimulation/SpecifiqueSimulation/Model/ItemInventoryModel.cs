namespace Model
{
    public class ItemInventoryModel
    {
        public ItemInventoryModel(int i, int g, int ii, double q)
        {
            Id = i;
            GroupId = g;
            ItemId = ii;
            Quantity = q;
        }

        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ItemId { get; set; }
        public double Quantity { get; set; }
    }
}