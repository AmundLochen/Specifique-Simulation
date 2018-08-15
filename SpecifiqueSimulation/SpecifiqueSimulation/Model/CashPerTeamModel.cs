namespace Model
{
    public class CashPerTeamModel
    {
        public CashPerTeamModel()
        {
        }

        public CashPerTeamModel(int i, double c)
        {
            Id = i;
            Cash = c;
        }

        public int Id { get; set; }
        public double Cash { get; set; }
    }
}