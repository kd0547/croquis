using initializationControl;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


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

        private Point _lastMouseDown;

        private ControlResize MainFormSize;
        private ControlResize FileGridSize;
        private ControlResize PreViewSize;
        private CroquisPlay croquisPlay;

        private static int ImageSize = 129;


        public MainWindow()
        {
            InitializeComponent();
            SetAllowDrop();
            PreViewGrid.Width = 328;

            croquisPlay = new CroquisPlay(ThreadSleep, showImage);
            MainFormSize = new ControlResize(MainWin.Width, MainWin.Height);
            FileGridSize = new ControlResize(FileGrid.Width, FileGrid.Height);
            PreViewSize = new ControlResize(PreViewGrid.Width, PreViewGrid.Height);



            croquisTreeView.Drop += CroquisTreeDropEvent;
        }
        /// <summary>
        /// Drag & Drop 설정
        /// </summary>
        private void SetAllowDrop()
        {
            DirectoryView.AllowDrop = true;
            croquisTreeView.AllowDrop = true;
        }

        /// <summary>
        /// 윈도우 Load 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoadEvent(object sender, RoutedEventArgs e)
        {
            //디렉토리 가져오기 
            GetLocalFile();

        }

        private void TargetTreeViewDragEnter(object sender, DragEventArgs e)
        {

        }

        //https://it10.tistory.com/8
        //https://learn.microsoft.com/ko-kr/dotnet/api/system.windows.dragdrop.dodragdrop?view=windowsdesktop-7.0
        //https://www.csharpstudy.com/WinForms/WinForms-dragdrop.aspx
        //https://icodebroker.tistory.com/entry/CWPF-DragDrop-%ED%81%B4%EB%9E%98%EC%8A%A4-DoDragDrop-%EC%A0%95%EC%A0%81-%EB%A9%94%EC%86%8C%EB%93%9C%EB%A5%BC-%EC%82%AC%EC%9A%A9%ED%95%B4-%EB%93%9C%EB%9E%98%EA%B7%B8-%EB%93%9C%EB%A1%AD-%EC%82%AC%EC%9A%A9%ED%95%98%EA%B8%B0
        //https://learn.microsoft.com/en-us/answers/questions/363157/wpf-drag-and-drop-from-listbox-into-treeview



        #region 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceTreeViewEntryMouseEvent(object sender, MouseEventArgs e)
        {
            TreeViewItem SendItem = (TreeViewItem)sender;
            Debug.WriteLine(" T!: " + SendItem.ToString());
        }
        #endregion

        /// <summary>
        /// 소스 트리뷰를 클릭할 시 발생하는 이벤트
        /// Drag & Drop의 시작 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreViewMouseLeftButtonDownEvent(object sender, MouseEventArgs e)
        {
            DependencyObject dependencyObject = DirectoryView.InputHitTest(e.GetPosition(DirectoryView)) as DependencyObject;
            try
            {
                if (dependencyObject is TextBlock)
                {
                    TreeViewItem SendItem = sender as TreeViewItem;
                    DragDrop.DoDragDrop(dependencyObject, DirectoryView.SelectedValue, DragDropEffects.Move);
                }
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 타겟 트리뷰에서 Drop 시 발생하는 이벤트 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CroquisTreeDropEvent(object sender, DragEventArgs e)
        {
            TreeViewItem obj = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;
            TreeViewItem targetTreeViewItem = new TreeViewItem();

            targetTreeViewItem.Expanded += new RoutedEventHandler(TargetTreeViewItemExpanded);
            targetTreeViewItem.MouseDoubleClick += FileItemDoubleClickEvent;
            targetTreeViewItem.Header = obj.Header;
            targetTreeViewItem.Tag = obj.Tag;

            TargetGetDirectories(targetTreeViewItem);
            TargetGetFile(targetTreeViewItem);
            croquisTreeView.Items.Add(targetTreeViewItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        private void TargetGetDirectories(TreeViewItem parent)
        {
            string path = parent.Tag.ToString();
            String Extension = Path.GetExtension(path);

            if (IsImageExtension(Extension)) return;
            if (parent == null) return;
            if (parent.Items.Count != 0) return;

            DirectoryInfo directory = new DirectoryInfo(path);
            if (directory != null) ;


            if (directory.Attributes == FileAttributes.Directory)
            {
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Tag = dir.FullName;
                    item.Header = dir.Name;
                    item.Expanded += new RoutedEventHandler(TargetTreeViewItemExpanded);
                    item.MouseDoubleClick += FileItemDoubleClickEvent;
                    TargetGetFile(item);
                    parent.Items.Add(item);
                }
            }
        }
        /// <summary>
        /// 디렉토리에 있는 이미지 파일을 가져온다. 
        /// </summary>
        /// <param name="parent"></param>
        private void TargetGetFile(TreeViewItem parent)
        {
            string filePath = parent.Tag.ToString();
            DirectoryInfo directory = new DirectoryInfo(filePath);

            if (filePath == null) return;
            if (directory == null) return;

            if (directory.Attributes == FileAttributes.Directory)
            {
                foreach (FileInfo dir in directory.GetFiles())
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Tag = dir.FullName;
                    item.Header = dir.Name;
                    item.PreviewMouseLeftButtonDown += ImageClickEvent;


                    parent.Items.Add(item);
                }
            }

        }

        /// <summary>
        /// 디렉토리 확장 시 발생하는 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item == null) return;

            foreach (TreeViewItem subItem in item.Items)
            {
                TargetGetDirectories(subItem);
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void giveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            }
            else
            {
                e.UseDefaultCursors = true;
            }
            e.Handled = true;
        }




        /// <summary>
        /// 
        /// </summary>
        System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            int sum = 0;
            foreach (var item in screens)
            {
                sum += item.WorkingArea.Width;
                if (sum >= this.Left + this.Width / 2)
                {
                    this.MaxHeight = item.WorkingArea.Height;
                    break;
                }
            }
        }

        /// <summary>
        /// 로컬디스크를 가져온다.
        /// </summary>
        private void GetLocalFile()
        {
            foreach (string dir in Directory.GetLogicalDrives())
            {
                try
                {
                    TreeViewItem item = new TreeViewItem();

                    item.Header = dir;
                    item.Tag = dir;
                    item.Expanded += new RoutedEventHandler(itemExpanded);

                    DirectoryView.Items.Add(item);
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
        //https://stackoverflow.com/questions/61041282/showing-image-thumbnail-with-mouse-cursor-while-dragging
        /// <summary>
        /// 하위 폴더를 가져온다.
        /// </summary>
        /// <param name="parent"></param>
        private void GetSubDirectories(TreeViewItem parent)
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
                    TreeViewItem subItem = new TreeViewItem();
                    subItem.Header = dir.Name;
                    subItem.Tag = dir.FullName;


                    subItem.Expanded += new RoutedEventHandler(itemExpanded);
                    subItem.PreviewMouseDoubleClick += FileItemDoubleClickEvent;
                    subItem.PreviewMouseDown += PreViewMouseLeftButtonDownEvent;
                    subItem.MouseEnter += SourceTreeViewEntryMouseEvent;
                    //subItem.GiveFeedback += giveFeedBack;
                    //subItem.PreviewGiveFeedback += giveFeedBack;
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




        //https://stackoverflow.com/questions/61041282/showing-image-thumbnail-with-mouse-cursor-while-dragging
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileItemDoubleClickEvent(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TreeViewItem clickItem = (TreeViewItem)sender;
                DirectoryInfo directory = new DirectoryInfo(clickItem.Tag as string);
                FileInfo[] fileInfos = directory.GetFiles();

                if (fileInfos.Length != 0)
                {
                    Debug.WriteLine(fileInfos.Length);

                    showImageFile(clickItem.Tag.ToString());
                }


            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// 폴더에서 이미지를 가져오는 함수
        /// </summary>
        /// <param name="path"></param>
        private void showImageFile(string path)
        {
            PictureViewer.Items.Clear();

            DirectoryInfo directory = new DirectoryInfo(path);
            int count = directory.GetFiles().Length;
            int col = calculateCol((int)PreViewGrid.Width);
            int row = (count / col) + 1;

            Debug.WriteLine("Rows " + row + ", Cols " + col);
            PictureViewer.DataContext = new PictureBoxRowCols(row, col);

            foreach (FileInfo file in directory.GetFiles())
            {

                if (!IsImageExtension(file.FullName))
                {
                    continue;
                }
                //Debug.WriteLine(file.FullName);
                Image img = new Image();

                img.Width = ImageSize;
                img.Height = ImageSize;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(file.FullName);
                bitmapImage.DecodePixelWidth = ImageSize;
                bitmapImage.EndInit();

                img.Source = bitmapImage;
                img.Tag = file.FullName;

                img.MouseLeftButtonDown += ImageClickEvent;
                PictureViewer.Items.Add(img);
            }
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

        private BitmapImage createImage(String ImagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(ImagePath);
            bitmapImage.DecodePixelWidth = ImageSize;
            bitmapImage.EndInit();

            return bitmapImage;
        }



        /// <summary>
        /// PreView의 width크기를 가져와 
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        private int calculateCol(int width)
        {
            return (int)width / ImageSize;
        }



        /// <summary>
        /// 이미지 미리보기에서 이미지를 클릭하면 원본 이미지를 확인할 수 있다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageClickEvent(object sender, MouseEventArgs e)
        {
            Uri url = null;
            if (sender is Image)
            {
                Image send = sender as Image;
                url = new Uri(send.Tag.ToString());
            }
            if (sender is TreeViewItem)
            {
                TreeViewItem treeViewItem = sender as TreeViewItem;
                url = new Uri(treeViewItem.Tag.ToString());

            }

            if (url == null) return;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = url;
            bitmapImage.EndInit();

            mainContent.Source = bitmapImage;
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




        /// <summary>
        /// 숨김파일 또는 시스템 파일
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
        /// 윈도우 창을 드래그로 변경시 동작하는 이벤트
        /// 화면의 크기가 변경되면 FileGrid와 PreViewGrid의 Height가 변경된다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinFormResizeEvent(object sender, SizeChangedEventArgs e)
        {

            ControlSize tempsize = MainFormSize.minus(MainWin.Width, MainWin.Height);
            double _temp = tempsize.height / 2;

            if (WindowState == WindowState.Maximized)
            {
                double MaxWindow = MainGrid.ActualHeight / 2;


                FileGrid.Height = MaxWindow - 3; // margin 값 수치 조정 
                PreViewGrid.Height = MaxWindow - 3; // margin 값 수치 조정 
            }
            else
            {
                //파일 그리드와 미리보기 그리드의 Height를 재설정한다.
                FileGrid.Height = FileGridSize.plusHeight(_temp);
                PreViewGrid.Height = PreViewSize.plusHeight(_temp);
            }
        }

        public static List<string> imagePath = new List<string>();
        int i = 0;

        private void GetCroquisFile(TreeViewItem item)
        {
            if (IsImageExtension(item.Tag.ToString()))
            {
                imagePath.Add(item.Tag.ToString());
            }

            if (item.Items.Count == 0) return;


            foreach (TreeViewItem subItem in item.Items)
            {
                GetCroquisFile(subItem);
            }

        }
        Thread myThread;

        private void WarningBox(string message)
        {
            MessageBox.Show(message, "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void start_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (croquisTreeView.Items.Count == 0)
            {
                WarningBox("이미지 파일이 없습니다.");
                return;
            }

            foreach (TreeViewItem treeViewItem in croquisTreeView.Items)
            {
                GetCroquisFile(treeViewItem);
            }

            Button button = sender as Button;
            if (button == null) { return; }

            croquisPlay.Interval = 10000;
            croquisPlay.RefreshInterval = 3000;

            if (button.Content.ToString().Equals("시작"))
            {

                button.Content = "중지";
                if (imagePath.Count > 0)
                {
                    //myThread = new Thread(StartCroquis);
                    //myThread.Start(imagePath);
                    croquisPlay.run(imagePath);
                }
                return;
            }


            if (button.Content.ToString().Equals("중지"))
            {
                croquisPlay.stop();
                button.Content = "시작";

                return;
            }
        }

        private void showImage(string path)
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if(path == null) { mainContent.Source = null; return; }
                Uri url = new Uri(path);
                Debug.WriteLine(path);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = url;
                bitmapImage.EndInit();
                mainContent.Source = bitmapImage;

            }));
        }

        private void ThreadSleep(int time)
        {
            Thread.Sleep(time);
        }

    }

}
