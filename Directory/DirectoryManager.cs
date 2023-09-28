using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using croquis;
using OpenCvSharp;

public class DirectoryManager
{
    private ImageTreeViewItemFactory imageTreeViewItemFactory;
    private ExceptionLogger log;


    public DirectoryManager(ImageTreeViewItemFactory imageTreeViewItemFactory, ExceptionLogger log)
    {
        this.imageTreeViewItemFactory = imageTreeViewItemFactory;
        this.log = log;
    }

    #region 파일탐색기 코드 
    /// <summary>
    /// 파일탐색기에서 사용한다. 
    /// 사용자 컴퓨터의 로컬 드라이브를 가져온다.
    /// </summary>
    public void GetLocalDrives(TreeView treeView)
    {
        foreach (string dir in Directory.GetLogicalDrives())
        {
            try
            {
                ImageTreeViewItem item = imageTreeViewItemFactory.CreateLocalDrivesItem(GetIcomImage(dir), dir, dir);

                GetSubDirectories(item);

                treeView.Items.Add(item);
            }
            catch (UnauthorizedAccessException uaae)
            {
                log.LogWrite(uaae.Message);
            }
            catch (IOException ioe)
            {
                log.LogWrite(ioe.Message);
            }
        }
    }

    /// <summary>
    /// 파일 탐색기에서 사용한다. 
    /// 하위 디렉토리를 가져온다. 
    /// </summary>
    /// <param name="parent"></param>
    public void GetSubDirectories(ImageTreeViewItem parent)
    {
        if (parent == null) return;
        if (parent.Items.Count != 0) return;

        try
        {
            string SubDirectory = parent.Tag as string;
            DirectoryInfo directory = new DirectoryInfo(SubDirectory);

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                if (IsNotHiddenDirectory(dir))
                {
                    continue;
                }

                ImageTreeViewItem subItem = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(dir.FullName), dir.Name, dir.FullName, BookMarkEmptyStarImage());
                
                //subItem.MouseEnter += SourceTreeViewEntryMouseEvent;

                parent.Items.Add(subItem);
            }
        }
        catch (UnauthorizedAccessException uaae)
        {
            log.LogWrite(uaae);
        }
        catch (IOException ioe)
        {
            log.LogWrite(ioe);
        }

    }


    #endregion


    #region 크로키 목록 코드

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    public void TargetGetDirectories(ImageTreeViewItem parent)
    {

        string path = parent.Tag.ToString();
        String Extension = Path.GetExtension(path);

        if (IsImageExtension(Extension)) return;
        if (parent == null) return;
        if (parent.Items.Count != 0) return;

        DirectoryInfo directory = new DirectoryInfo(path);
        if (directory == null) return;


        if (directory.Attributes == FileAttributes.Directory)
        {
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                //빌터패턴으로 다시 구현하기 
                ImageTreeViewItem item = imageTreeViewItemFactory.CreateTargetGetDirectories(GetIcomImage(dir.FullName), dir.Name, dir.FullName);

                TargetGetFile(item);
                parent.Items.Add(item);
            }
        }
    }


    /// <summary>
    /// 디렉토리에 있는 이미지 파일을 가져온다. 
    /// </summary>
    /// <param name="parent"></param>
    public void TargetGetFile(TreeViewItem parent)
    {
        string filePath = parent.Tag.ToString();
        DirectoryInfo directory = new DirectoryInfo(filePath);

        if (filePath == null) return;
        if (directory == null) return;

        if (directory.Attributes == FileAttributes.Directory)
        {
            foreach (FileInfo dir in directory.GetFiles())
            {
                ImageTreeViewItem item = imageTreeViewItemFactory.CreateTargetGetFile(dir.Name, dir.FullName);
                parent.Items.Add(item);
            }
        }

    }
    #endregion

    #region 북마크 기능

    /// <summary>
    /// 
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="path"></param>

    public void AddBookMarkTreeItem(TreeView treeView, string path)
    {
        try
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            if (directory == null) return;

            ImageTreeViewItem item = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(directory.FullName), directory.Name, directory.FullName, BookMarkEmptyStarImage());


            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                if (IsNotHiddenDirectory(dir))
                {
                    continue;
                }

                ImageTreeViewItem subItem = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(dir.FullName), dir.Name, dir.FullName, BookMarkEmptyStarImage());


                item.Items.Add(subItem);
            }

            treeView.Items.Add(item);
        }
        catch (Exception e)
        {
            log.LogWrite(e);
        }

    }

    /// <summary>
    /// 즐겨찾기 트리에 추가하는 메서드 
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="path"></param>

    public void AddBookMarkTreeItem(TreeView treeView, ImageTreeViewItem viewItem)
    {
        try
        {
            string path = viewItem.Tag.ToString();
            DirectoryInfo directory = new DirectoryInfo(path);

            if (directory == null) return;



            ImageTreeViewItem item = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(directory.FullName), directory.Name, directory.FullName, BookMarkImage(viewItem));
            item.IsBookMarkSelected = true;

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                if (IsNotHiddenDirectory(dir))
                {
                    continue;
                }

                ImageTreeViewItem subItem = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(dir.FullName), dir.Name, dir.FullName, BookMarkImage(viewItem));
                subItem.IsBookMarkSelected = true;

                item.Items.Add(subItem);
            }

            treeView.Items.Add(item);
        }
        catch (Exception e)
        {
            log.LogWrite(e);
        }

    }
    #endregion


    #region 북마크 이미지(수정하기)


    public ImageSource BookMarFullStarImage()
    {
        System.Drawing.Bitmap bitmap = Resource1.FullStar;
        System.Drawing.Bitmap resize = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(14, 14));
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        MemoryStream ms = new MemoryStream();
        resize.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        ms.Seek(0, SeekOrigin.Begin);
        bitmapImage.StreamSource = ms;
        bitmapImage.EndInit();

        return bitmapImage;
    }





    public ImageSource BookMarkEmptyStarImage()
    {
        System.Drawing.Bitmap bitmap = Resource1.EmptyStar;
        System.Drawing.Bitmap resize = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(14, 14));
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        MemoryStream ms = new MemoryStream();
        resize.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        ms.Seek(0, SeekOrigin.Begin);
        bitmapImage.StreamSource = ms;
        bitmapImage.EndInit();

        return bitmapImage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ImageSource BookMarkImage(ImageTreeViewItem item)
    {
        try
        {
            System.Drawing.Bitmap bitmap = null;
            if (item.IsBookMarkSelected)
            {
                bitmap = Resource1.FullStar;
            }
            else
            {
                bitmap = Resource1.EmptyStar;
            }

            if (bitmap == null)
            {
                bitmap = Resource1.EmptyStar;
            }

            System.Drawing.Bitmap resize = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(14, 14));
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            MemoryStream ms = new MemoryStream();
            resize.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();

            return bitmapImage;
        }
        catch (Exception e)
        {
            log.LogWrite(e);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ImageSource BookMarkImage(bool item)
    {
        try
        {
            System.Drawing.Bitmap bitmap = null;
            if (item)
            {
                bitmap = Resource1.FullStar;
            }
            else
            {
                bitmap = Resource1.EmptyStar;
            }

            if (bitmap == null)
            {
                bitmap = Resource1.EmptyStar;
            }

            System.Drawing.Bitmap resize = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(14, 14));
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            MemoryStream ms = new MemoryStream();
            resize.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();

            return bitmapImage;
        }
        catch (Exception e)
        {
            log.LogWrite(e);
        }

        return null;
    }

    #endregion



    #region 북마크 보조 기능 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directoryView"></param>
    /// <param name="clickImageTreeItem"></param>
    /// <returns></returns>
    public ImageTreeViewItem FindImageTreeViewItem(TreeView directoryView, ImageTreeViewItem clickImageTreeItem)
    {
        if (directoryView.Items.Count == 0)
            return null;


        //탐색 수를 줄이기 위해 루트를 조회 
        string clickImageTreeItemFullName = clickImageTreeItem.FullName;
        string clickImageTreeItemRoot = Path.GetPathRoot(clickImageTreeItemFullName);

        ImageTreeViewItem selectItem = null;

        foreach (ImageTreeViewItem item in directoryView.Items)
        {
            string itemFullName = item.FullName;
            string root = Path.GetPathRoot(itemFullName);

            if (root.Equals(clickImageTreeItemRoot))
            {
                selectItem = item;
            }
        }

        if (selectItem == null)
            return null;

        foreach (ImageTreeViewItem item in selectItem.Items)
        {
            if (item.FullName.Equals(clickImageTreeItem.FullName))
                return item;


            ImageTreeViewItem foundItem = FindImageTreeViewItemSub(item, clickImageTreeItem);
            if (foundItem != null)
            {
                return foundItem;
            }
        }



        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="clickImageTreeItem"></param>
    /// <returns></returns>
    public ImageTreeViewItem FindImageTreeViewItemSub(ImageTreeViewItem item, ImageTreeViewItem clickImageTreeItem)
    {
        if (item.Items.Count == 0)
            return null;

        string clickImageTreeSub = clickImageTreeItem.FullName;

        foreach (ImageTreeViewItem subItem in item.Items)
        {
            string subItemFullName = subItem.FullName;

            if (clickImageTreeSub.Equals(subItemFullName))
            {
                return subItem;
            }

            ImageTreeViewItem foundItem = FindImageTreeViewItemSub(subItem, clickImageTreeItem);

            if (foundItem != null)
            {
                return foundItem;
            }
        }


        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directoryView"></param>
    /// <param name="clickImageTreeItem"></param>
    public void EmptyImage(TreeView directoryView, ImageTreeViewItem clickImageTreeItem)
    {

        clickImageTreeItem.IsBookMarkSelected = false;
        clickImageTreeItem.BookMarkImageSource = BookMarkEmptyStarImage();

        if (clickImageTreeItem.Items.Count != 0)
        {
            foreach (var item in clickImageTreeItem.Items)
            {
                ImageTreeViewItem imageTreeViewItem = item as ImageTreeViewItem;
                EmptyImage(directoryView, imageTreeViewItem);
            }
        }
    }

    /// <summary>
    /// 파일탐색기에서 즐겨찾기 이미지를 변경한다. 
    /// </summary>
    /// <param name="clickImageTreeItem"></param>
    /// <param name="fullStartImage"></param>
    public void checkFullImage(ImageTreeViewItem clickImageTreeItem, ImageSource fullStartImage)
    {
        try
        {
            if (clickImageTreeItem == null)
                return;

            foreach (ImageTreeViewItem item in clickImageTreeItem.Items)
            {
                item.IsBookMarkSelected = true;
                item.BookMarkImageSource = fullStartImage;


                checkFullImage(item, fullStartImage);
            }
        }
        catch (Exception e)
        {
            log.LogWrite(e);
        }

    }




    /// <summary>
    /// 즐겨찾기에서 
    /// </summary>
    /// <param name="clickImageTreeItem"></param>
    public void RemoveBookMark(TreeView treeView, ImageTreeViewItem clickImageTreeItem)
    {



        foreach (ImageTreeViewItem item in treeView.Items)
        {
            if (item.FullName.Equals(clickImageTreeItem.FullName))
            {
                treeView.Items.Remove(item);

                break;
            }

            if (RemoveItemFromChildren(item, clickImageTreeItem))
                break;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentItem"></param>
    /// <param name="targetItem"></param>
    /// <returns></returns>
    public bool RemoveItemFromChildren(ImageTreeViewItem parentItem, ImageTreeViewItem targetItem)
    {
        for (int i = 0; i < parentItem.Items.Count; i++)
        {
            ImageTreeViewItem childItem = (ImageTreeViewItem)parentItem.Items[i];

            if (childItem.FullName.Equals(targetItem.FullName))
            {
                parentItem.Items.RemoveAt(i);
                return true;
            }


            if (RemoveItemFromChildren(childItem, targetItem))
                return true;
        }

        return false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public ImageTreeViewItem getParent(Image image)
    {
        if (image != null)
        {
            object parent = image.Parent;

            while (parent != null && !(parent is ImageTreeViewItem))
            {
                if (parent is FrameworkElement element)
                {
                    parent = element.Parent; // 다음 상위 요소로 이동
                }
                else
                {
                    parent = null;
                }
            }

            return parent as ImageTreeViewItem;
        }

        return null;
    }
    #endregion

    #region 파일 이름 찾기

    /// <summary>
    /// 두 파일 이름이 동일한지 확인합니다.
    /// </summary>
    /// <param name="fileName1">비교할 첫 번째 파일 이름입니다.</param>
    /// <param name="fileName2">비교할 두 번째 파일 이름입니다.</param>
    /// <returns>두 파일 이름이 동일하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool IsFileNamesEqual(string fileName1, string fileName2) 
    {
        if (fileName1 == null)
        {
            throw new ArgumentNullException(nameof(fileName1), "The first file name cannot be null.");
        }

        if (fileName2 == null)
        {
            throw new ArgumentNullException(nameof(fileName2), "The second file name cannot be null.");
        }

        return fileName1.Equals(fileName2);
    }


    #endregion



    #region 기타 


    /// <summary>
    /// 아이콘을 가져온다. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ImageSource GetIcomImage(string path)
    {
        IntPtr hImgSmall; //system Image list 
        IntPtr hImgLarge;
        string Fname;
        SHFILEINFO shinfo = new SHFILEINFO();
        hImgLarge = Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

        System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);

        return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

    /// <summary>
    /// 숨김파일 또는 시스템 파일을 
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="treeView"></param>
    /// <returns></returns>
    public ImageTreeViewItem FindSelectedItem(TreeView treeView)
    {
        if (treeView.Items.Count == 0)
            return null;

        foreach (ImageTreeViewItem item in treeView.Items)
        {
            if (item.IsSelected) { return item; }

            if (item.Items.Count != 0)
            {
                ImageTreeViewItem subItem = FindSelectedSubItem(item);
                if (subItem != null)
                {
                    return subItem;
                }
            }

        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private ImageTreeViewItem FindSelectedSubItem(ImageTreeViewItem item)
    {
        if (item.Items.Count == 0)
            return null;

        foreach (ImageTreeViewItem subItem in item.Items)
        {
            if (subItem.IsSelected) { return subItem; }

            if (item.Items.Count != 0)
            {
                ImageTreeViewItem FindsubItem = FindSelectedSubItem(subItem);

                if (FindsubItem != null)
                {
                    return FindsubItem;
                }
            }

        }

        return null;
    }

    /// <summary>
    /// 주어진 파일 경로의 확장자가 이미지 확장자 중 하나인지 확인합니다.
    /// 지원되는 이미지 확장자에는 .jpg, .jpeg, .bmp, .exif, .png, .tif, .tiff 등이 포함됩니다.
    /// </summary>
    /// <param name="path">확장자를 확인하고자 하는 파일의 경로입니다.</param>
    /// <returns>파일이 이미지 확장자를 가지면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool IsImageExtension(string path)
    {
        string extension = Path.GetExtension(path);
        string _extension = extension.ToLower();
        string[] extensionArray = new string[] { ".jpg", ".jpeg", ".bmp", ".exif", ".png", ".tif", ".tiff" };

        foreach (string e in extensionArray)
        {
            if (e.Equals(_extension))
            {
                return true;
            }
        }
        return false;
    }

    #endregion



    
}
