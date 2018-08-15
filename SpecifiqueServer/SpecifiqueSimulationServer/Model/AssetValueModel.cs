using System.Collections;

namespace Model
{
    public class AssetValueModel : IEnumerable
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public double TaxCost { get; set; }
        public double OperationExpenses { get; set; }
        public double IncomePerAsset { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return AssetId;
            yield return TaxCost;
            yield return OperationExpenses;
            yield return IncomePerAsset;
        }
    }
}