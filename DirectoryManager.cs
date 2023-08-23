using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

public class DirectoryManager
{
    private ImageTreeViewItemFactory imageTreeViewItemFactory;



    public DirectoryManager(ImageTreeViewItemFactory imageTreeViewItemFactory)
    {
        this.imageTreeViewItemFactory = imageTreeViewItemFactory;
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

            }
            catch (IOException ioe)
            {

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

                ImageTreeViewItem subItem = imageTreeViewItemFactory.CreateSubDirectories(GetIcomImage(dir.FullName), dir.Name, dir.FullName);
                
                //subItem.MouseEnter += SourceTreeViewEntryMouseEvent;

                parent.Items.Add(subItem);
            }
        }
        catch (UnauthorizedAccessException uaae)
        {

        }
        catch (IOException ioe)
        {

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
    /// 아이콘을 가져온다. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private ImageSource GetIcomImage(string path)
    {
        IntPtr hImgSmall; //system image list 
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
    /// 이미지 확장자 체크
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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
