namespace Model
{
    public class ItemModel
    {
        public ItemModel()
        {
        }

        public ItemModel(int i, string n, int p, string des)
        {
            Id = i;
            Name = n;
            Price = p;
            Description = des;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
    }
}