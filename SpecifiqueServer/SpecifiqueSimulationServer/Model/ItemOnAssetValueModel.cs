using System.Collections;

namespace Model
{
    public class ItemOnAssetValueModel : IEnumerable
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int ItemId { get; set; }
        public double Ratio { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return AssetId;
            yield return ItemId;
            yield return Ratio;
        }
    }
}