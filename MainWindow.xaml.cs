using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace croquis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    class DirectoryListing
    {
        public string path { get; set; }
        public Image image { get; set; }
        public DirectoryListing(Image imgage, String path)
        {
            this.image = image;
            this.path = path;
        }
    }

    


    public partial class MainWindow : Window
    {
        private double fGrid_W;
        private double fGrid_H;

        private double preGrid_W;
        private double preGrid_H;

        private double mainForm_W;
        private double mainForm_H;
        private Point _lastMouseDown;


        public MainWindow()
        {


            PicBoxRowCol t = new PicBoxRowCol();
            t.Cols = 3;
            t.Rows = 3;
            InitializeComponent();
            

            PicBox.DataContext = t;



            mainForm_W = MainWin.Width;
            mainForm_H = MainWin.Height;

            fGrid_W = fileGrid.Width;
            fGrid_H = fileGrid.Height;

            preGrid_W = preViewGrid.Width;
            preGrid_H = preViewGrid.Height;

            trView.AllowDrop = true;
            croquisTreeView.AllowDrop = true;


            croquisTreeView.Drop += ListItem_Drop;

        }

        private TreeViewItem datasource;



        
        private void PreViewMouseLeftButDow(object sender, MouseEventArgs e)
        {

            DependencyObject deo = trView.InputHitTest(e.GetPosition(trView)) as DependencyObject;
            try
            {
                if (deo is TextBlock)
                {
                    TreeViewItem item = e.Source as TreeViewItem;

                    TreeViewItem t = sender as TreeViewItem;
                    

                    DragDrop.DoDragDrop(trView, trView.SelectedValue, DragDropEffects.Move);
                    e.Handled = true;

                }

            } catch (ArgumentNullException ex)
            {

            }

            
            
        }

        private void ListItem_Drop(object sender, DragEventArgs e)
        {
            TreeViewItem _tree = e.Source as TreeViewItem;
            TreeViewItem obj = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;

            

            //Debug.WriteLine("tree" + _tree.ToString());

            Debug.WriteLine("obj" + obj.ToString());

            
           

            TreeViewItem item = new TreeViewItem();

            item.Header = obj.Header;
            item.Name = obj.Name;


            if (obj != null)
            {
                croquisTreeView.Items.Add(item);
            }

            


        }


        private void TreeMouseDragOver(object sender, DragEventArgs e)
        {
            
        }
        


        void onClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            //ofd.Multiselect = true;

            String[] pathDirs = null;

            if(ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathDirs = ofd.FileNames.ToArray();

                ListDirectory(trView, pathDirs[0]);
            } else
            {
                return;
            }
            
        }
        //http://inasie.tistory.com/19
        //https://learn.microsoft.com/ko-kr/dotnet/api/system.windows.controls.treeviewitem?view=windowsdesktop-7.0
        
        private void giveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            if(e.Effects == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            } else
            {
                e.UseDefaultCursors = true;
            }
            e.Handled = true;
        }


        private void WinLoad(object sender, RoutedEventArgs e)
        {
            foreach(string dir in Directory.GetLogicalDrives())
            {
                try
                {
                    TreeViewItem item = new TreeViewItem();

                    item.Header = dir;
                    item.Tag = dir;
                    item.Expanded += new RoutedEventHandler(itemExpanded);

                    trView.Items.Add(item);

                    GetSubDirectories(item);

                } catch ( UnauthorizedAccessException uaae)
                {

                } catch(IOException ioe)
                {

                }
            }
        }

        private void GetSubDirectories(TreeViewItem item) 
        {
            if(item == null) return;
            if(item.Items.Count != 0) return;




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


                    subItem.Expanded += new RoutedEventHandler(itemExpanded);
                    subItem.PreviewMouseDoubleClick += showPic;
                    subItem.PreviewMouseDown += PreViewMouseLeftButDow;
                    subItem.DragOver += TreeMouseDragOver;
                    //subItem.GiveFeedback += giveFeedBack;
                    subItem.PreviewGiveFeedback += giveFeedBack;
                    item.Items.Add(subItem);

                }

                

            } catch (UnauthorizedAccessException uaae)
            {

            } catch (IOException ioe)
            {

            }

        }

        private void giveFeedBackEvent(object sender, GiveFeedbackEventArgs e)
        {

        }
        

        private void showPic(object sender, MouseButtonEventArgs e)
        {
            

            try
            {
                TreeViewItem clickItem = (TreeViewItem) sender;
                
                if(clickItem.Items.Count == 0)
                {
                    showImageFile(clickItem.Tag.ToString());
                } 

                

            } catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void showImageFile(string path)
        {
            PicBox.Items.Clear();

            DirectoryInfo directory = new DirectoryInfo(path);
            int count = directory.GetFiles().Length;
            PicBoxRowCol p = (PicBoxRowCol) PicBox.DataContext;
            int col = p.Cols;

            int row = (count / col) + 1 ;

            Debug.WriteLine(count);
            Debug.WriteLine(col);
            Debug.WriteLine(row);

            PicBoxRowCol temp = new PicBoxRowCol();
            temp.Cols = col;
            temp.Rows = row; 

            PicBox.DataContext = temp;

            foreach (FileInfo file in directory.GetFiles())
            {
                Debug.WriteLine(file.FullName);


                Image img = new Image();
                
                img.Width = 129;
                img.Height = 129;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(file.FullName);
                bitmapImage.DecodePixelWidth = 129;
                bitmapImage.EndInit();

                img.Source = bitmapImage;
                img.Tag = file.FullName;
                PicBox.Items.Add(img);
                img.MouseLeftButtonDown += ImageClickEvent;
            }
            
            




        }

      


        private void ImageClickEvent(object sender, RoutedEventArgs e)
        {
            Image send = (Image)sender;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(send.Tag.ToString());
            bitmapImage.EndInit();

            mainContent.Source = bitmapImage;
        }


        private void itemExpanded(object sender, RoutedEventArgs e)
        {
            
            TreeViewItem item = sender as TreeViewItem;

            if(item == null ) return;
            if (item.Items.Count == 0) return;

            foreach(TreeViewItem subItem in item.Items)
            {
                GetSubDirectories(subItem);
            }
        }



        private void ListDirectory(TreeView treeView, String path)
        {
            

            var stack = new Stack<TreeViewItem>();
            var rootDirectory = new DirectoryInfo(path);

            var node = new TreeViewItem()
            {
                Header = rootDirectory.Name,
                Tag = rootDirectory
            };
            stack.Push(node);

            try
            {
                while (stack.Count > 0)
                {
                    var currnetNode = stack.Pop();
                    var directoryInfo = (DirectoryInfo)currnetNode.Tag;


                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        if (IsNotHiddenDirectory(directory))
                        {
                            continue;
                        }

                        var childDirectoryNode = new TreeViewItem() {Header = directory.Name, Tag = directory };

                        currnetNode.Items.Add(childDirectoryNode);
                        stack.Push(childDirectoryNode);
                    }

                    foreach (var file in directoryInfo.GetFiles())
                    {
                        var treeNode = new TreeViewItem() {Header = file.Name };

                        currnetNode.Items.Add(treeNode);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {

            }


            treeView.Items.Add(node);


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

        //파일 탐색기
        //void onClick(object sender, RoutedEventArgs e)
        //{

        //    var fileContent = string.Empty;
        //    var filePath = string.Empty;

        //    using (OpenFileDialog ofd = new OpenFileDialog())
        //    {
        //        ofd.InitialDirectory = "c:\\";
        //        ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        //        ofd.FilterIndex = 2;
        //        ofd.RestoreDirectory = true;

        //        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            //Get the path of specified file
        //            filePath = ofd.FileName;

        //            //Read the contents of the file into a stream
        //            var fileStream = ofd.OpenFile();

        //            using (StreamReader reader = new StreamReader(fileStream))
        //            {
        //                fileContent = reader.ReadToEnd();
        //            }


        //        }
        //    }






        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void winFormResizeEvent(object sender, SizeChangedEventArgs e)
        {
            double temp_W = MainWin.Width - mainForm_W;
            double temp_H = MainWin.Height - mainForm_H;
            int _temp = (int) temp_H / 2;

            fileGrid.Height = fGrid_H + _temp;
            preViewGrid.Height = preGrid_H + _temp;
        }
    }

    public class PicBoxRowCol
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
    }

    
    
}
