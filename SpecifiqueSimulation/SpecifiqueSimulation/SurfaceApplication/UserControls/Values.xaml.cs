using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Values.xaml
    /// </summary>
    public partial class Values
    {
        //Storing variables
        public List<GroupModel> Groups;
        private List<TheValues> _values;

        public Values()
        {
            _values = new List<TheValues>();
            Groups = new List<GroupModel>();
            InitializeComponent();
        }

        public void UpdateGroups(List<CashPerTeamModel> ca)
        {
            List<TheValues> newValues = new List<TheValues>();

            foreach (GroupModel group in Groups)
            {
                foreach (CashPerTeamModel cash in ca)
                {
                    if(cash.Id == group.Id)
                        newValues.Add(new TheValues(group.Id, group.Name, cash.Cash));
                }
            }

            _values = newValues;
            SortGroupValues();
            Dispatcher.Invoke((Action) (() => { groupValues.DataContext = _values; }));
        }

        private void SortGroupValues()
        {
            List<TheValues> sortedList = _values.OrderByDescending(o => o.Cash).ToList();
            _values = sortedList;
        }

        public class TheValues
        {
            public TheValues(int i, string n, double c)
            {
                Id = i;
                Name = n;
                Cash = c;
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public double Cash { get; set; }
        }
    }
}