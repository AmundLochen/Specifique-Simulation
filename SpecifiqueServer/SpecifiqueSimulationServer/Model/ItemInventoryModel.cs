using System.Collections;

namespace Model
{
    public class ItemInventoryModel : IEnumerable
    {
        public ItemInventoryModel()
        {
        }

        public ItemInventoryModel(int i, int gi, int ii, double q)
        {
            Id = i;
            GroupId = gi;
            ItemId = ii;
            Quantity = q;
        }

        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ItemId { get; set; }
        public double Quantity { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return GroupId;
            yield return ItemId;
            yield return Quantity;
        }
    }
}