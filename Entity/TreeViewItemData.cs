using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TreeViewItemData
{
    public string HeaderText { get; set; }


    public ImageCache ImageCache { get; set; } = null;

    public bool IsBookMarkSeleted { get; set; } = false;


    public List<TreeViewItemData> Children { get; set; }

    public TreeViewItemData()
    {
        Children = new List<TreeViewItemData>();
    }

}
