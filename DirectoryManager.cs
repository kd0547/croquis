using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using croquis;

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


    #region 크로키창 코드

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
        if (directory != null) return;


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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="path"></param>
    #region
    public void AddBookMarkTreeItem(TreeView treeView,string path)
    {
        try
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            if(directory == null) return;

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
    #region
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

            if(bitmap == null)
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
        } catch (Exception e)
        {
            log.LogWrite(e);
        }

        return null;
    }


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
    /// 주어진 파일 경로의 확장자가 이미지 확장자 중 하나인지 확인합니다.
    /// 지원되는 이미지 확장자에는 .jpg, .jpeg, .bmp, .exif, .png, .tif, .tiff 등이 포함됩니다.
    /// </summary>
    /// <param name="path">확장자를 확인하고자 하는 파일의 경로입니다.</param>
    /// <returns>파일이 이미지 확장자를 가지면 true, 그렇지 않으면 false를 반환합니다.</returns>
    private bool IsImageExtension(string path)
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
}
#endregion