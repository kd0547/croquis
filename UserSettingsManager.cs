using Microsoft.VisualBasic.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class UserSettingsManager
{
    private ExceptionLogger logger;
    private DirectoryManager directoryManager;
    private ImageTreeViewItemFactory imageTreeViewItemFactory;
    public UserSettingsManager()
	{
	}


    private void SaveCroquisTree(TreeView tree)
    {
        if (System.Windows.MessageBox.Show("크로키 기록을 저장하시겠습니까?", "croquis", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            var treeViewData = SaveTreeViewItems(tree);


            // 객체를 JSON 문자열로 직렬화
            string jsonData = JsonSerializer.Serialize(treeViewData);

            // JSON 문자열을 파일로 저장
            File.WriteAllText("croquis.json", jsonData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="treeView"></param>
    /// <returns></returns>
    public TreeViewItemData SaveTreeViewItems(TreeView treeView)
    {
        var rootData = new TreeViewItemData();


        foreach (ImageTreeViewItem item in treeView.Items)
        {
            rootData.Children.Add(SaveTreeViewItem(item));
        }
        return rootData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private TreeViewItemData SaveTreeViewItem(ImageTreeViewItem item)
    {
        var itemData = new TreeViewItemData
        {
            HeaderText = item._FullName
        };

        foreach (ImageTreeViewItem subItem in item.Items)
        {
            itemData.Children.Add(SaveTreeViewItem(subItem));
        }


        return itemData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tree"></param>
    public void LoadCroquisTree(TreeView tree)
    {
        TreeViewItemData data = LoadJson();
        if (data != null)
        {
            LoadTreeViewItems(tree, data);
        }
    }

    /// <summary>
    /// Croquis 설정을 JSON 파일에서 로드하여 TreeViewItemData 객체로 반환합니다.
    /// </summary>
    /// <returns>
    /// 로드된 TreeViewItemData 객체. JSON 파일을 읽지 못하거나 데이터가 없는 경우 null을 반환합니다.
    /// </returns>
    private TreeViewItemData LoadJson()
    {
        try
        {
            string LoadJsonData = File.ReadAllText("croquis.json");

            if (LoadJsonData == null) return null;

            TreeViewItemData item = JsonSerializer.Deserialize<TreeViewItemData>(LoadJsonData);

            return item;
        }
        catch (Exception e)
        {
            logger.LogWrite(e.StackTrace);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="data"></param>
    public void LoadTreeViewItems(TreeView treeView, TreeViewItemData data)
    {
        treeView.Items.Clear();
        foreach (var itemData in data.Children)
        {
            treeView.Items.Add(LoadTreeViewItem(itemData));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <returns></returns>
    private ImageTreeViewItem LoadTreeViewItem(TreeViewItemData itemData)
    {
        string path = itemData.HeaderText;
        string extension = Path.GetExtension(path);
        ImageTreeViewItem item = null;

        if (extension == "")
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            ImageSource icon = directoryManager.GetIcomImage(path);

            item = imageTreeViewItemFactory.CreateTargetGetDirectories(icon, directory.Name, directory.FullName);
        }
        else
        {
            FileInfo fileInfo = new FileInfo(path);

            item = imageTreeViewItemFactory.CreateTargetGetFile(fileInfo.Name, fileInfo.FullName);
        }



        foreach (var subItemData in itemData.Children)
        {
            item.Items.Add(LoadTreeViewItem(subItemData));
        }

        return item;
    }

}
