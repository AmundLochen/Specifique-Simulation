using System.Collections.Generic;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory
    {
        public int GroupNumber;
        public List<GroupModel> Groups;
        public List<GroupModel> OtherGroups;

        public Inventory()
        {
            Groups = new List<GroupModel>();
            OtherGroups = new List<GroupModel>();
            InitializeComponent();
        }

        public void SetGroups(List<GroupModel> gs)
        {
            Groups = gs;

            if (GroupNumber != 0)
                foreach (GroupModel group in Groups)
                    if (group.Id != GroupNumber)
                        OtherGroups.Add(group);
        }
    }
}