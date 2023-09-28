using Microsoft.VisualBasic.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


public class FilePath
{
    public static readonly string croquisPath = "croquis.json";
    public static readonly string bookMarkPath = "bookMarkView.json";
}


public class UserSettingsManager
{
    private ExceptionLogger logger;
    private DirectoryManager directoryManager;
    private ImageTreeViewItemFactory imageTreeViewItemFactory;

    

    public UserSettingsManager(ExceptionLogger logger, DirectoryManager directoryManager, ImageTreeViewItemFactory imageTreeViewItemFactory)
    {
        this.logger = logger;
        this.directoryManager = directoryManager;
        this.imageTreeViewItemFactory = imageTreeViewItemFactory;
    }

    #region 북마크, 크로키목록의 TreeView를 TreeViewItemData로 변환 후 JSON 파일로 저장

    /// <summary>
    /// TreeView의 데이터를 지정된 경로에 JSON 형식으로 저장합니다.
    /// </summary>
    /// <param name="treeView">저장할 TreeView 객체입니다.</param>
    /// <param name="filePath">저장할 파일 경로입니다.</param>
    public void SaveTreeViewDataAsJson(TreeView tree, string filePath)
    {
        try
        {
            var treeViewData = ConvertTreeViewItemsToData(tree, filePath);

            // 객체를 JSON 문자열로 직렬화
            string jsonData = JsonSerializer.Serialize(treeViewData);

            //JSON 문자열을 파일에 저장
            File.WriteAllText(filePath, jsonData);
        } catch (Exception ex)
        {
            logger.LogWrite(ex);
        }

    }

    /// <summary>
    /// TreeView의 아이템을 TreeViewItemData 형식으로 변환합니다.
    /// 루트 값에 파일 경로를 저장한다.
    /// </summary>
    /// <param name="treeView">변환할 TreeView 객체입니다.</param>
    /// <returns>TreeView의 아이템 정보를 담은 TreeViewItemData 객체입니다.</returns>
    public TreeViewItemData ConvertTreeViewItemsToData(TreeView treeView, string filePath)
    {
        //Root 값에 파일 경로를 저장한다. 
        TreeViewItemData rootItemData = new TreeViewItemData()
        {
            HeaderText = filePath,
        };


        foreach (ImageTreeViewItem item in treeView.Items)
        {
            rootItemData.Children.Add(ConvertImageTreeViewItemToData(item));
        }

        return rootItemData;
    }

    /// <summary>
    /// ImageTreeViewItem 객체를 TreeViewItemData 형식으로 변환합니다.
    /// </summary>
    /// <param name="item">변환할 ImageTreeViewItem 객체입니다.</param>
    /// <returns>ImageTreeViewItem의 정보를 담은 TreeViewItemData 객체입니다.</returns>
    private TreeViewItemData ConvertImageTreeViewItemToData(ImageTreeViewItem item)
    {
        // ImageTreeViewItem의 정보를 TreeViewItemData 형식으로 저장
        var itemData = new TreeViewItemData
        {
            HeaderText = item._FullName,
            ImageCache = item.ImageCache,
            IsBookMarkSeleted = item.IsBookMarkSelected
        };

        // ImageTreeViewItem의 하위 아이템들도 TreeViewItemData 형식으로 변환하고 저장
        foreach (ImageTreeViewItem subItem in item.Items)
        {
            itemData.Children.Add(ConvertImageTreeViewItemToData(subItem));
        }


        return itemData;
    }

    #endregion



    #region JSON 파일에서 데이터를 로드한 후 북마크, 크로키 트리를 생성 후 컨트롤에 추가 
    /// <summary>
    /// JSON 파일에서 트리 뷰 데이터를 불러와 지정된 TreeView 컨트롤에 로드합니다.
    /// </summary>
    /// <param name="tree">데이터를 로드할 TreeView 컨트롤입니다.</param>
    /// <param name="path">불러올 JSON 파일의 경로입니다.</param>
    public void LoadCroquisTree(TreeView tree, string path)
    {
        try
        {
            TreeViewItemData loadedData = LoadTreeViewDataFromJson(path);

            // 불러온 데이터가 null이 아니라면 TreeView에 로드합니다.
            if (loadedData != null)
            {
                LoadItemsIntoTreeView(tree, loadedData);
            }
        }
        catch (Exception e)
        {
            logger.LogWrite(e.StackTrace);
        }
    }

    /// <summary>
    /// Croquis 설정을 JSON 파일에서 로드하여 TreeViewItemData 객체로 반환합니다.
    /// </summary>
    /// <returns>
    /// 로드된 TreeViewItemData 객체. JSON 파일을 읽지 못하거나 데이터가 없는 경우 null을 반환합니다.
    /// </returns>
    private TreeViewItemData LoadTreeViewDataFromJson(string filePath)
    {

        // 파일에서 JSON 데이터를 읽습니다.
        string LoadJsonData = File.ReadAllText(filePath);

        // 파일이 비어 있는 경우 null을 반환합니다
        if (string.IsNullOrEmpty(LoadJsonData)) return null;

        // JSON 데이터를 TreeViewItemData 객체로 역직렬화합니다.
        TreeViewItemData treeViewItemData = JsonSerializer.Deserialize<TreeViewItemData>(LoadJsonData);

        return treeViewItemData;

    }

    /// <summary>
    /// TreeViewItemData 객체의 데이터를 사용하여 TreeView에 ImageTreeViewItem 형태로 아이템을 로드합니다.
    /// </summary>
    /// <param name="treeView">아이템을 로드할 TreeView 컨트롤입니다.</param>
    /// <param name="treeItemData">TreeView에 로드할 ImageTreeViewItem의 데이터를 담고 있는 TreeViewItemData 객체입니다.</param>
    public void LoadItemsIntoTreeView(TreeView treeView, TreeViewItemData treeItemData)
    {
        if (treeItemData == null) { return; }
        if (treeView.Items.Count > 0)
            treeView.Items.Clear();

        //루트값에 있는 파일 경로를 가져온다. 
        string path = treeItemData.HeaderText;

        if(FilePath.croquisPath.Equals(path))
        {
            foreach (var itemData in treeItemData.Children)
            {
                treeView.Items.Add(CreateCroquisTree(itemData));
            }
        }

        if(FilePath.bookMarkPath.Equals(path))
        {
            foreach (var itemData in treeItemData.Children)
            {
                treeView.Items.Add(CreateBookMarkTree(itemData));
            }
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <returns></returns>
    private ImageTreeViewItem CreateCroquisTree(TreeViewItemData itemData)
    {
        string path = itemData.HeaderText;
        string extension = Path.GetExtension(path);
        ImageTreeViewItem item = null;


        //디렉토리 
        if (extension.Equals(""))
        {

            DirectoryInfo directory = new DirectoryInfo(path);
            ImageSource icon = directoryManager.GetIcomImage(path);

            ImageSource bookMarkImage = null;

            
            item = imageTreeViewItemFactory.CreateTargetGetDirectories(icon, directory.Name, directory.FullName, bookMarkImage);
        }
        //파일
        else
        {
            FileInfo fileInfo = new FileInfo(path);

            item = imageTreeViewItemFactory.CreateTargetGetFile(fileInfo.Name, fileInfo.FullName);
        }

        if (itemData != null)
        {
            item.ImageCache = itemData.ImageCache;
            item.IsBookMarkSelected = itemData.IsBookMarkSeleted;
        }

        foreach (var subItemData in itemData.Children)
        {
            item.Items.Add(CreateCroquisTree(subItemData));
        }

        return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <returns></returns>
    private ImageTreeViewItem CreateBookMarkTree(TreeViewItemData itemData)
    {
        string path = itemData.HeaderText;
   
        ImageTreeViewItem item = null;
        DirectoryInfo directory = new DirectoryInfo(path);
        ImageSource icon = directoryManager.GetIcomImage(path);

        //북마크 저장 
        ImageSource bookMarkImage = directoryManager.BookMarkImage(itemData.IsBookMarkSeleted) ;


        item = imageTreeViewItemFactory.CreateSubDirectories(icon, directory.Name, directory.FullName, bookMarkImage);

        //TreeViewItemData의 데이터를 저장 
        if (itemData != null)
        {
            item.ImageCache = itemData.ImageCache;
            item.IsBookMarkSelected = itemData.IsBookMarkSeleted;
        }

        foreach (var subItemData in itemData.Children)
        {
            item.Items.Add(CreateBookMarkTree(subItemData));
        }

        return item;
    }

    #endregion



}
