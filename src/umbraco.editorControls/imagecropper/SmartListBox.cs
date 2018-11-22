using System;
using System.Web.UI.WebControls;

namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SmartListBox : ListBox
    {
        //Moves the selected items up one level
        public void MoveUp()
        {

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Selected)//identify the selected item
                {
                    //swap with the top item(move up)
                    if (i > 0 && !Items[i - 1].Selected)
                    {
                        ListItem bottom = Items[i];
                        Items.Remove(bottom);
                        Items.Insert(i - 1, bottom);
                        Items[i - 1].Selected = true;
                    }
                }
            }
        }
        //Moves the selected items one level down
        public void MoveDown()
        {
            int startindex = Items.Count - 1;
            for (int i = startindex; i > -1; i--)
            {
                if (Items[i].Selected)//identify the selected item
                {
                    //swap with the lower item(move down)
                    if (i < startindex && !Items[i + 1].Selected)
                    {
                        ListItem bottom = Items[i];
                        Items.Remove(bottom);
                        Items.Insert(i + 1, bottom);
                        Items[i + 1].Selected = true;
                    }

                }
            }
        }
    }
}