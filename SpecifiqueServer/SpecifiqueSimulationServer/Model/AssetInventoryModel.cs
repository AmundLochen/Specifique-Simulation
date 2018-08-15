using System.Collections;

namespace Model
{
    public class AssetInventoryModel : IEnumerable
    {
        public AssetInventoryModel()
        {
        }

        public AssetInventoryModel(int i, int ai, int gi, int s)
        {
            Id = i;
            AssetId = ai;
            GroupId = gi;
            Share = s;
        }

        public int Id { get; set; }
        public int AssetId { get; set; }
        public int GroupId { get; set; }

        /// <summary>
        ///     Percentage ownership.
        /// </summary>
        public int Share { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return AssetId;
            yield return GroupId;
            yield return Share;
        }
    }
}