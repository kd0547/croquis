using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

public class FileWatcher
{
    private ImageTreeViewItemFactory imageTreeViewItemFactory;
    private DirectoryManager directoryManager;
    private List<FileSystemWatcher> _watchers;
    private ExceptionLogger logger;

    public event FileSystemEventHandler? FileSystemChanged ;

    public TreeView DirectoryView { get; set; }
    public ListBox PictureViewer { get; set; }

    public FileWatcher(DirectoryManager directoryManager,ImageTreeViewItemFactory imageTreeViewItemFactory, ExceptionLogger logger) 
    {
        this.directoryManager = directoryManager;
        this.imageTreeViewItemFactory = imageTreeViewItemFactory;
        this.logger = logger;
    }

    public void Config()
    {
        try
        {
            string osDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            Debug.WriteLine(osDrive);
            string[] drivers = Directory.GetLogicalDrives();
            _watchers = new List<FileSystemWatcher>();

            foreach (string driver in drivers)
            {
                if (driver.Equals(osDrive))
                {
                    continue;
                }

                FileSystemWatcher watcher = new FileSystemWatcher(driver);
                watcher.Filter = "*.*";

                watcher.EnableRaisingEvents = true;
                //하위 디렉토리의 변화까지 감지
                watcher.IncludeSubdirectories = true;



                watcher.NotifyFilter = NotifyFilters.Attributes    //속성 변경
                                    | NotifyFilters.CreationTime   //생성시간
                                    | NotifyFilters.DirectoryName  //디렉토리 이름
                                    | NotifyFilters.FileName       //파일 이름
                                    | NotifyFilters.LastAccess     //마지막 접근
                                    | NotifyFilters.LastWrite      //마지막 쓰여진
                                    | NotifyFilters.Security       //보안
                                    | NotifyFilters.Size;          //크기

                watcher.Changed += watcher_Change;
                watcher.Deleted += watcher_Delete;
                watcher.Created += watcher_Create;
                watcher.Renamed += watcher_Rename;

                _watchers.Add(watcher);
            }
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogWrite(e);
        }
        catch (ArgumentException e)
        {
            logger.LogWrite(e);
        }
        catch (IOException e)
        {
            logger.LogWrite(e);
        }
    }

    /// <summary>
    /// 디렉토리가 추가될 때 발생하는 이벤트  
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void watcher_Create(object sender, FileSystemEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        {
            try
            {
                string root = Path.GetPathRoot(e.FullPath);
                foreach (ImageTreeViewItem item in DirectoryView.Items)
                {
                    string rootPath = item.Tag.ToString();
                    if (rootPath.Equals(root))
                    {
                        CreateDirectoryTree(item, e.FullPath);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                logger.LogWrite(e);
            }
            catch (Exception e)
            {
                logger.LogWrite(e);
            }
        }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void watcher_Change(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed) return;

        //Debug.WriteLine($"변화된 파일 경로 {e.FullPath}");
        //Debug.WriteLine($"변화된 파일 경로 {Path.GetDirectoryName(e.FullPath)}");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void watcher_Delete(object sender, FileSystemEventArgs e)
    {
        

        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        {
            try
            {
                string root = Path.GetPathRoot(e.FullPath);
                string PathEx = Path.GetExtension(e.FullPath); //directory or file 

                if (PathEx == "")
                {
                    foreach (ImageTreeViewItem item in DirectoryView.Items)
                    {
                        string rootPath = item.Tag.ToString();
                        if (rootPath.Equals(root))
                        {

                            RemoveDirectoryTree(item, e.FullPath);

                        }
                    }
                }
                else
                {
                    RemoveImage(PictureViewer, e.FullPath);
                }
            } 
            catch (ArgumentException e) 
            {
                logger.LogWrite(e);
            }
            catch (NullReferenceException e)
            {
                logger.LogWrite(e);
            }
            catch (Exception e)
            {
                logger.LogWrite(e);
            }
            
        }));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void watcher_Rename(object sender, RenamedEventArgs e)
    {

        //Debug.WriteLine($"이름이 바뀐 파일 경로 {e.OldFullPath}");
        //Debug.WriteLine($"이름이 바뀐 파일 경로 {e.FullPath}");
        //Debug.WriteLine($"이름이 바뀐 파일 경로 {Path.GetDirectoryName(e.FullPath)}");
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
            logger.LogWrite(e.Message);
        } 
        catch (Exception e)
        {
            logger.LogWrite(e.Message);
        }

    }




    /// <summary>
    /// 로컬드라이브에 새로운 디렉토리가 추가되면 파일 탐색기에 추가한다.
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="NewPath"></param>
    private void CreateDirectoryTree(ImageTreeViewItem tree, string NewPath)
    {
        try
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

                    ImageTreeViewItem item = imageTreeViewItemFactory.CreateSubDirectories(directoryManager.GetIcomImage(NewPath), TreeTagPathFileName, NewPath);
                    
                    tree.Items.Add(item);
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
        catch (Exception e)
        {
            logger.LogWrite(e);
        }
        
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
            logger.LogWrite(e);
        }
        catch (Exception e)
        {
            logger.LogWrite(e);
        }
    }
}
