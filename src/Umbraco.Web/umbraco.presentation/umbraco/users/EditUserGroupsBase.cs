using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using umbraco.BasePages;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Base class provided common functionality to the web forms that allow for assignment of users to groups
    /// </summary>
    public abstract class EditUserGroupsBase : UmbracoEnsuredPage
    {
        protected void MoveItems(ListBox from, ListBox to)
        {
            for (var i = from.Items.Count - 1; i >= 0; i--)
            {
                if (from.Items[i].Selected)
                {
                    to.Items.Add(from.Items[i]);
                    var li = from.Items[i];
                    from.Items.Remove(li);
                }
            }

            from.SelectedIndex = -1;
            to.SelectedIndex = -1;
            SortItems(to);
        }

        protected void SortItems(ListBox listBox)
        {
            // Hat-tip: http://stackoverflow.com/a/3396527
            var list = new List<ListItem>(listBox.Items.Cast<ListItem>());
            list = list.OrderBy(li => li.Text).ToList<ListItem>();
            listBox.Items.Clear();
            listBox.Items.AddRange(list.ToArray<ListItem>());
        }
    }
}