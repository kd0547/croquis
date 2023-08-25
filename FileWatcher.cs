using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

public class FileWatcher
{
    private ImageTreeViewItemFactory imageTreeViewItemFactory;
    private FileSystemWatcher watcher;
    
    public event FileSystemEventHandler? FileSystemChanged ;


    public FileWatcher(ImageTreeViewItemFactory imageTreeViewItemFactory, FileSystemWatcher watcher) 
    {
        this.imageTreeViewItemFactory = imageTreeViewItemFactory;
        this.watcher = watcher;
    }

    private void Config()
    {
        watcher = new FileSystemWatcher(@"G:\");
        watcher.Filter = "*.*";

        watcher.EnableRaisingEvents = true;
        //하위 디렉토리의 변화까지 감지
        watcher.IncludeSubdirectories = true;

        //테스트 

        watcher.NotifyFilter = NotifyFilters.Attributes    //속성 변경
                            | NotifyFilters.CreationTime   //생성시간
                            | NotifyFilters.DirectoryName  //디렉토리 이름
                            | NotifyFilters.FileName       //파일 이름
                            | NotifyFilters.LastAccess     //마지막 접근
                            | NotifyFilters.LastWrite      //마지막 쓰여진
                            | NotifyFilters.Security       //보안
                            | NotifyFilters.Size;          //크기

        //watcher.Changed += watcher_Change;
        //watcher.Deleted += watcher_Delete;
        //watcher.Created += watcher_Create;
        //watcher.Renamed += watcher_Rename;
    }


    /// <summary>
    /// 파일 탐색기에서 파일을 삭제한다. 
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="DeletePath"></param>
    private void RemoveDirectoryTree(ImageTreeViewItem tree, string DeletePath)
    {
        try
        {
            if (tree.Tag.ToString() == DeletePath)
            {
                DependencyObject dependency = tree.Parent;
                ImageTreeViewItem ImageParent = dependency as ImageTreeViewItem;

                if (ImageParent != null)
                {
                    ImageParent.Items.Remove(tree);
                    ImageParent.IsSelected = false;
                }

                return;
            }

            if (tree.Items.Count == 0) return;
            foreach (ImageTreeViewItem treeViewItem in tree.Items)
            {
                RemoveDirectoryTree(treeViewItem, DeletePath);
            }
        }
        catch (InvalidOperationException e)
        {

        }
    }



    /// <summary>
    /// 미리보기에서 이미지를 삭제한다.
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="DeletePath"></param>
    private void RemoveImage(ListBox tree, string DeletePath)
    {
        try
        {
            if (tree.Items.Count == 0) return;
            foreach (Image item in tree.Items)
            {
                if (item.Tag.ToString().Equals(DeletePath))
                {
                    tree.Items.Remove(item);
                }
            }
        }
        catch (InvalidOperationException e)
        {

        }

    }

    /// <summary>
    /// 로컬드라이브에 새로운 디렉토리가 추가되면 파일 탐색기에 추가한다.
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="NewPath"></param>
    private void CreateDirectoryTree(ImageTreeViewItem tree, string NewPath)
    {
        string PathEx = Path.GetExtension(NewPath); //directory or file 
        string _Path = Path.GetDirectoryName(NewPath);

        //directory 
        if (PathEx == "")
        {
            string TreeTagPath = tree.Tag.ToString();

            if (TreeTagPath.Equals(_Path))
            {
                string TreeTagPathFileName = Path.GetFileName(NewPath);
                //ImageTreeViewItem item = ImageTreeViewItem.createImageTreeViewItem(GetIcomImage(NewPath), TreeTagPathFileName, NewPath);
                //item.Expanded += new RoutedEventHandler(itemExpanded);
                //item.PreviewMouseDoubleClick += LoadPreviewImagesOnFileItemDoubleClick;
                //item.PreviewMouseDown += PreViewMouseLeftButtonDownEvent;
                //item.MouseEnter += SourceTreeViewEntryMouseEvent;

                //tree.Items.Add(item);
            }
        }
        else
        {
            //File을 사용할 때 추가하기 
        }

        if (tree.Items.Count == 0) return;
        foreach (ImageTreeViewItem treeViewItem in tree.Items)
        {
            CreateDirectoryTree(treeViewItem, NewPath);
        }
    }

}
