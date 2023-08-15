using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

public class LocalFile
{
    public void GetLocalFile(TreeView treeView)
    {
        foreach (string dir in Directory.GetLogicalDrives())
        {
            try
            {
                TreeViewItem item = new TreeViewItem();

                item.Header = dir;
                item.Tag = dir;
                item.Expanded += new RoutedEventHandler(itemExpanded);

                treeView.Items.Add(item);

                GetSubDirectories(item);

            }
            catch (UnauthorizedAccessException uaae)
            {

            }
            catch (IOException ioe)
            {

            }
        }
    }
    private void itemExpanded(object sender, RoutedEventArgs e)
    {

        TreeViewItem item = sender as TreeViewItem;

        if (item == null) return;
        if (item.Items.Count == 0) return;

        foreach (TreeViewItem subItem in item.Items)
        {
            GetSubDirectories(subItem);
        }
    }

    private void GetSubDirectories(TreeViewItem item)
    {
        if (item == null) return;
        if (item.Items.Count != 0) return;

        try
        {
            string subDir = item.Tag as string;
            DirectoryInfo directory = new DirectoryInfo(subDir);

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                if (IsNotHiddenDirectory(dir))
                {
                    continue;
                }
                TreeViewItem subItem = new TreeViewItem();
                subItem.Header = dir.Name;
                subItem.Tag = dir.FullName;


                //subItem.Expanded += new RoutedEventHandler(itemExpanded);
                //subItem.PreviewMouseDoubleClick += showPic;
                //subItem.PreviewMouseDown += PreViewMouseLeftButDow;
                //subItem.DragOver += TreeMouseDragOver;
                ////subItem.GiveFeedback += giveFeedBack;
                //subItem.PreviewGiveFeedback += giveFeedBack;
                //item.Items.Add(subItem);

            }



        }
        catch (UnauthorizedAccessException uaae)
        {

        }
        catch (IOException ioe)
        {

        }

    }

    private bool IsNotHiddenDirectory(DirectoryInfo directory)
    {
        string[] attributes = directory.Attributes.ToString().Split(',');

        foreach (string attribute in attributes)
        {
            if (attribute.Equals(FileAttributes.Hidden.ToString()) || attribute.Equals(FileAttributes.System.ToString()))
            {
                return true;
            }
        }

        return false;
    }

}
