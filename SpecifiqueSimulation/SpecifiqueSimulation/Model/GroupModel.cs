namespace Model
{
    public class GroupModel
    {
        public GroupModel(int i, string n)
        {
            Id = i;
            Name = n;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}