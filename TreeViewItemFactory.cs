using System;
using System.Windows.Controls;
using System.Windows.Input;

public class TreeViewItemFactory
{
    public TreeViewItemFactory()
    {

    }


    public TreeViewItem createTreeViewItem()
    {
        return null;
    }


    private void showPic(object sender, MouseButtonEventArgs e)
    {
        try
        {
            TreeViewItem clickItem = (TreeViewItem)sender;

            if (clickItem.Items.Count == 0)
            {
                showImageFile(clickItem.Tag.ToString());
            }



        }
        catch (Exception exception)
        {

        }
    }


    private void showImageFile(string path)
    {
        //PicBox.Items.Clear();

        //DirectoryInfo directory = new DirectoryInfo(path);
        //int count = directory.GetFiles().Length;
        //PicBoxRowCol p = (PicBoxRowCol)PicBox.DataContext;
        //int col = p.Cols;

        //int row = (count / col) + 1;


        //PicBoxRowCol temp = new PicBoxRowCol();
        //temp.Cols = col;
        //temp.Rows = row;

        //PicBox.DataContext = temp;

        //foreach (FileInfo file in directory.GetFiles())
        //{
        //    Debug.WriteLine(file.FullName);


        //    Image img = new Image();

        //    img.Width = 129;
        //    img.Height = 129;

        //    BitmapImage bitmapImage = new BitmapImage();
        //    bitmapImage.BeginInit();
        //    bitmapImage.UriSource = new Uri(file.FullName);
        //    bitmapImage.DecodePixelWidth = 129;
        //    bitmapImage.EndInit();

        //    img.Source = bitmapImage;
        //    img.Tag = file.FullName;
        //    PicBox.Items.Add(img);
        //    img.MouseLeftButtonDown += ImageClickEvent;
        //}
    }

}
