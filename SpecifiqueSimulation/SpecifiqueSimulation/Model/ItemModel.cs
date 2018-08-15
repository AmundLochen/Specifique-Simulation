namespace Model
{
    public class ItemModel
    {
        public ItemModel(int i, string n, double v, string d)
        {
            Id = i;
            Name = n;
            Value = v;
            Description = d;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
    }
}