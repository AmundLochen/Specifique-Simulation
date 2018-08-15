using System.Collections;

namespace Model
{
    public class ItemValueModel : IEnumerable
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double TaxCost { get; set; }
        public double StorageExpenses { get; set; }
        public double IncomePerItem { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return ItemId;
            yield return TaxCost;
            yield return StorageExpenses;
            yield return IncomePerItem;
        }
    }
}