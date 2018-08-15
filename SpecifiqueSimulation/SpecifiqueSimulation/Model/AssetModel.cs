namespace Model
{
    public class AssetModel
    {
        public AssetModel(int i, string n, double p, string d)
        {
            Id = i;
            Name = n;
            Price = p;
            Description = d;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
    }
}