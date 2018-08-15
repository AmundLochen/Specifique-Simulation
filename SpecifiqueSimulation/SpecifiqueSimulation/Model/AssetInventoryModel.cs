namespace Model
{
    public class AssetInventoryModel
    {
        public AssetInventoryModel(int i, int a, int g, int s)
        {
            Id = i;
            AssetId = a;
            GroupId = g;
            Share = s;
        }

        public int Id { get; set; }
        public int AssetId { get; set; }
        public int GroupId { get; set; }
        public int Share { get; set; }
    }
}