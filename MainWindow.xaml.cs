using initializationControl;
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Threading;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace croquis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private ExceptionLogger log;
        private FileWatcher fileWatcher;
        private ImageTreeViewItemFactory imageTreeViewItemFactory;
        private DirectoryManager _directoryManager;
        private ImageManager _imageManager;
        private Point _lastMouseDown;

        private ControlResize MainFormSize;
        private ControlResize FileGridSize;
        private ControlResize PreViewSize;
        private ControlResize ViewSize;
        private ControlResize ContenViewtSize;
        public static List<string> imagePath = new List<string>();
        private double ViewMinSIze;
        private double ContentMinSize;

        private CroquisPlay croquisPlay;

        private static int ImageSize = 129;

         private double PreGridMinSize ;

       
        public MainWindow()
        {
            log = new ExceptionLogger();

            InitializeComponent();
            SetAllowDrop();
            InitImageTreeViewFactory();
            InitImageManager();
            InitControlSize();


            fileWatcher = new FileWatcher(_directoryManager, imageTreeViewItemFactory, log)
            {
                DirectoryView = DirectoryView,
                PictureViewer = PictureViewer,
            };
            fileWatcher.Config();
            //테스트
            DisplayVisibility();

            croquisPlay = new CroquisPlay(ThreadSleep, showImage);
            croquisTreeView.Drop += CroquisTreeDropEvent;

            //fullDisplay.Click += FullDisplay;
            

            MainWin.PreviewKeyDown += EndFullDisplayButton;
            MainGrid.KeyDown += EndFullDisplayButton;

        }
       
        ImageSourceValueSerializer imageSourceValueSerializer = new ImageSourceValueSerializer();
        
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(croquisTreeView == null)
            {
                return;
            }


            if (System.Windows.MessageBox.Show("크로키 기록을 저장하시겠습니까?", "croquis", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var treeViewData = SaveTreeViewItems(croquisTreeView);


                // 객체를 JSON 문자열로 직렬화
                string jsonData = JsonSerializer.Serialize(treeViewData);
                
                // JSON 문자열을 파일로 저장
                File.WriteAllText("croquis.json", jsonData);
            }
        }

        public TreeViewItemData SaveTreeViewItems(TreeView treeView)
        {
            var rootData = new TreeViewItemData();
         
            
            foreach (ImageTreeViewItem item in treeView.Items)
            {
                rootData.Children.Add(SaveTreeViewItem(item));
            }
            return rootData;
        }


        private TreeViewItemData SaveTreeViewItem(ImageTreeViewItem item)
        {
            var itemData = new TreeViewItemData
            {
                HeaderText = item._FullName
            };

            foreach(ImageTreeViewItem subItem in item.Items)
            {
                itemData.Children.Add(SaveTreeViewItem(subItem));
            }

            
            return itemData;
        }




        /// <summary>
        /// 여러 컨트롤의 초기 크기를 설정하고 관련 정보를 저장하는 메서드입니다.
        /// </summary>
        private void InitControlSize()
        {
            


            MainFormSize = new ControlResize(MainWin.Width, MainWin.Height);
            FileGridSize = new ControlResize(FileGrid.Width, FileGrid.Height);
            PreViewSize = new ControlResize(PreViewGrid.Width, PreViewGrid.Height);
            ViewSize = new ControlResize(view.Width, view.Height);
            ContenViewtSize = new ControlResize(ContentView.Width, ContentView.Height);

            //컨텐츠 테스트
            ViewMinSIze = view.Width;


            PreViewGrid.Width = view.Width;
            ContentMinSize = ContentView.Width;
        }


        #region 테스트 중 로컬드라이브에서 파일 또는 디렉토리가 변경되면 감지해 파일 탐색기에 반영된다.
        #endregion

        #region 초기화
        /// <summary>
        /// Drag & Drop 설정
        /// </summary>
        private void SetAllowDrop()
        {
            DirectoryView.AllowDrop = true;
            croquisTreeView.AllowDrop = true;
        }

        /// <summary>
        /// 윈도우 Load 이벤트입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoadEvent(object sender, RoutedEventArgs e)
        {
            
            //디렉토리 가져오기 
            _directoryManager.GetLocalDrives(DirectoryView);

            MainWin.SizeChanged += new System.Windows.SizeChangedEventHandler(this.WinFormResizeEvent);
          
        }

        #region TopMost체크박스
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopMost_Checked(object sender, RoutedEventArgs e)
        {
            MainWin.Topmost = true;
        }

        private void TopMost_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWin.Topmost = false;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void InitImageManager()
        {
            _imageManager = new ImageManager(log)
            {

                ImageClick = ImageClickEvent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitImageTreeViewFactory()
        {
            imageTreeViewItemFactory = new ImageTreeViewItemFactory.FactoryBuilder()
                .SetExpanded(new RoutedEventHandler(itemExpanded))
                .SetPreviewMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                .SetMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                .SetPreviewMouseDown(PreViewMouseLeftButtonDownEvent)

                .SetPreviewMouseLeftButtonDown(ImageClickEvent)
                .SetPreviewMouseRightButtonDown(CroquisRightClickEvent)

                .SetBookMarKImageLeftButtonDown(BookMarkClickEvent)
                .Build();


            _directoryManager = new DirectoryManager(imageTreeViewItemFactory, log);


        }
        #endregion

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

        #region 파일 drag & Drop 이벤트
        /// <summary>
        /// 소스 트리뷰 아이템을 클릭할 때 발생하는 이벤트 핸들러입니다.
        /// 해당 이벤트는 Drag & Drop 작업을 시작하기 위해 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발생 객체</param>
        /// <param name="e">Mouse 이벤트에 대한 매개변수</param>
        private void PreViewMouseLeftButtonDownEvent(object sender, MouseEventArgs e)
        {
            DependencyObject dependencyObject = DirectoryView.InputHitTest(e.GetPosition(DirectoryView)) as DependencyObject;
            try
            {
                if (dependencyObject is TextBlock)
                {
                    ImageTreeViewItem SendItem = sender as ImageTreeViewItem;
                    DragDrop.DoDragDrop(dependencyObject, DirectoryView.SelectedValue, DragDropEffects.Move);
                }
            }
            catch (ArgumentNullException ex)
            {
                log.LogWrite(ex.Message);
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
            ImageTreeViewItem obj = e.Data.GetData(typeof(ImageTreeViewItem)) as ImageTreeViewItem;

            ImageTreeViewItem item = ImageTreeViewItem.createImageTreeViewItem(obj); 
            
            item.Expanded += new RoutedEventHandler(TargetTreeViewItemExpanded);
            item.MouseDoubleClick += LoadAndDisplayPreviewImagesOnFileItemDoubleClick;
            item.PreviewMouseRightButtonDown += CroquisRightClickEvent;
         
            _directoryManager.TargetGetDirectories(item);
            _directoryManager.TargetGetFile(item);
           
            croquisTreeView.Items.Add(item);
        } 


        /// <summary>
        /// 디렉토리 확장 시 발생하는 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem item = sender as ImageTreeViewItem;

            if (item == null) return;
            
            foreach (ImageTreeViewItem subItem in item.Items)
            {
                _directoryManager.TargetGetDirectories(subItem);
            }
        }



        #endregion

        #region 파일 or 디렉토리 삭제 이벤트
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CroquisRightClickEvent(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this);
            ImageTreeViewItem obj = sender as ImageTreeViewItem;

            if (obj.IsSelected)
            {
                DependencyObject dependency = obj.Parent;
                
                ImageTreeViewItem ImageParent = dependency as ImageTreeViewItem;
                TreeView TreeViewParent = dependency as TreeView;

                if(ImageParent != null)
                {
                    ImageParent.Items.Remove(obj);
                    ImageParent.IsSelected = false;
                }

                if(TreeViewParent != null)
                {
                    TreeViewParent.Items.Remove(obj);
                }

            }

        }

        #endregion


        #region 북마크 기능 
        private void BookMarkClickEvent(object sender, MouseButtonEventArgs e)
        {
            Image clickImage = sender as Image;
            if (clickImage == null)
                return;
            
            ImageTreeViewItem clickImageTreeItem = getParent(clickImage);


            if (clickImageTreeItem == null)
                return;

            //북마크에 없으면 
            if(clickImageTreeItem.IsBookMarkSelected == false) 
            {
                clickImageTreeItem.IsBookMarkSelected = true;
                //즐겨찾기에 저장 
                _directoryManager.AddBookMarkTreeItem(bookMarkView, clickImageTreeItem);

                ImageSource fullStartImage = _directoryManager.BookMarFullStarImage();

                clickImageTreeItem.BookMarkImageSource = fullStartImage;

                checkFullImage(clickImageTreeItem, fullStartImage);

                return;
            }

            if(clickImageTreeItem.IsBookMarkSelected == true) 
            {
                //즐겨찾기에서 삭제 
                RemoveBookMark(bookMarkView,clickImageTreeItem);

                ImageTreeViewItem Item = FindImageTreeViewItem(DirectoryView, clickImageTreeItem);

                if(Item != null)
                {
                    //탐색기에서 즐겨찾기 기능 해제 
                    EmptyImage(DirectoryView, Item);
                }
                
            }

        }

        private ImageTreeViewItem FindImageTreeViewItem(TreeView directoryView, ImageTreeViewItem clickImageTreeItem)
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

                if(root.Equals(clickImageTreeItemRoot))
                {
                    selectItem = item;
                }
            }

            if (selectItem == null)
                return null;

            foreach(ImageTreeViewItem item in selectItem.Items)
            {
                ImageTreeViewItem foundItem = FindImageTreeViewItemSub(item, clickImageTreeItem);

                if(foundItem != null) 
                {
                    return foundItem;
                }
            }
            


            return null;
        }

        private ImageTreeViewItem FindImageTreeViewItemSub(ImageTreeViewItem item, ImageTreeViewItem clickImageTreeItem)
        {
            if (item.Items.Count == 0)
                return null;

            string clickImageTreeSub = clickImageTreeItem.FullName;

            foreach (ImageTreeViewItem subItem in item.Items)
            {
                string subItemFullName = subItem.FullName;

                if(clickImageTreeSub.Equals(subItemFullName))
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


        private void EmptyImage(TreeView directoryView,ImageTreeViewItem clickImageTreeItem)
        {

            clickImageTreeItem.IsBookMarkSelected = false;
            clickImageTreeItem.BookMarkImageSource = _directoryManager.BookMarkEmptyStarImage();

            if(clickImageTreeItem.Items.Count != 0) 
            {
                foreach(var item in clickImageTreeItem.Items)
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
        private void checkFullImage(ImageTreeViewItem clickImageTreeItem, ImageSource fullStartImage)
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
            } catch (Exception e)
            {
                log.LogWrite(e);
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private ImageTreeViewItem getParent(Image image)
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

        /// <summary>
        /// 즐겨찾기에서 
        /// </summary>
        /// <param name="clickImageTreeItem"></param>
        private void RemoveBookMark(TreeView treeView, ImageTreeViewItem clickImageTreeItem)
        {
            foreach (ImageTreeViewItem item in treeView.Items)
            {
                if (RemoveItemFromChildren(item, clickImageTreeItem))
                    break;  
            }
        }

        private bool RemoveItemFromChildren(ImageTreeViewItem parentItem, ImageTreeViewItem targetItem)
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



        #endregion



        #region 전체화면 이벤트

        public System.Windows.Thickness originContentView;
        public double originContentWidth;
        /// <summary>
        /// 
        /// </summary>
        private void FullDisplay()
        {
            originContentView = ContentView.Margin;
            originContentWidth = ContentView.Width;

            if (fullDisplay.IsChecked  == true)
            {
                
                MainWin.WindowState = WindowState.Maximized;
                MainWin.Visibility = Visibility.Collapsed;
                MainWin.WindowStyle = WindowStyle.None;
                MainWin.ResizeMode = ResizeMode.NoResize;
                MainWin.Visibility = Visibility.Visible;
                MainWin.Topmost = true;


                croquisView.Visibility = Visibility.Hidden;
                view.Visibility = Visibility.Hidden;
                OptionView.Visibility = Visibility.Hidden;

                ContentView.Width = MainGrid.RenderSize.Width;
                ContentView.Margin = new System.Windows.Thickness { Left = 2, Top = 2, Right = 2, Bottom = 2 };
            }
        }

        private void EndFullDisplayButton(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                MainWin.WindowState = WindowState.Normal;
                MainWin.WindowStyle = WindowStyle.SingleBorderWindow;
                MainWin.ResizeMode = ResizeMode.CanResize;
                
                MainWin.Topmost = false;




                croquisView.Visibility = Visibility.Visible;
                view.Visibility = Visibility.Visible;
                OptionView.Visibility = Visibility.Visible;

                ContentView.Width = originContentWidth;
                ContentView.Margin = originContentView;
            }
            fullDisplay.IsChecked = false;


        }


        #endregion


        #region 탐색기 테스트 
        
        
        /// <summary>
        /// 초기화
        /// </summary>
        public void DisplayVisibility()
        {
            if(fileSearch.Visibility == Visibility.Visible)
            {
                Brush backgroundColor = new BrushConverter().ConvertFrom("#404040") as Brush ;
                DirectoryView.Background = backgroundColor;
                fileSearch.Background = backgroundColor;
            }

            fileSearch.MouseDown += MenuClickEvent;
            bookMark.MouseDown += MenuClickEvent;

        }

        public void MenuClickEvent(object sender, MouseEventArgs e)
        {
           

            try
            {
                Run target = e.Source as Run;
                if(target == null) { return; };
                if (target.Text.Equals("탐색기"))
                {
                    Brush backgroundColor = new BrushConverter().ConvertFrom("#404040") as Brush;
                    Brush backgroundColor2 = new BrushConverter().ConvertFrom("#262626") as Brush;
                    if (DirectoryView.Visibility == Visibility.Visible)
                    {

                    }

                    if (DirectoryView.Visibility == Visibility.Hidden)
                    {
                        DirectoryView.Background = backgroundColor;
                        fileSearch.Background = backgroundColor;

                        bookMark.Background = backgroundColor2;
                        bookMarkView.Background = backgroundColor2;

                        DirectoryView.Visibility = Visibility.Visible;
                        bookMarkView.Visibility = Visibility.Hidden;
                    }

                }

                if (target.Text.Equals("즐겨찾기"))
                {
                    Brush backgroundColor = new BrushConverter().ConvertFrom("#404040") as Brush;
                    Brush backgroundColor2 = new BrushConverter().ConvertFrom("#262626") as Brush;
                    if (bookMark.Visibility == Visibility.Visible)
                    {

                    }

                    if (bookMarkView.Visibility == Visibility.Hidden)
                    {
                        bookMark.Background = backgroundColor;
                        bookMarkView.Background = backgroundColor;

                        DirectoryView.Background = backgroundColor2;
                        fileSearch.Background = backgroundColor2;

                        DirectoryView.Visibility = Visibility.Hidden;
                        bookMarkView.Visibility = Visibility.Visible;
                    }
                }
            } 
            catch (NullReferenceException exception)
            {
                log.LogWrite(exception);
            }

        }

        #endregion

        #region 


        #endregion




        #region 디렉토리 확장 이벤트



        /// <summary>
        /// 파일 탐색기 확장 이벤트 
        /// </summary>
        /// <param name="parent"></param>
        private void itemExpanded(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem item = sender as ImageTreeViewItem;

            if (item == null) return;
            if (item.Items.Count == 0) return;

            foreach (ImageTreeViewItem subItem in item.Items)
            {
                _directoryManager.GetSubDirectories(subItem);
            }
        }

        


        
        /// <summary>
        /// 아이콘을 가져온다. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ImageSource GetIcomImage(string path)
        {
            IntPtr hImgSmall; //system Image list 
            IntPtr hImgLarge;
            string Fname;
            SHFILEINFO shinfo = new SHFILEINFO();
            hImgLarge = Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);
            
            System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }


        #endregion

        #region 이미지 이벤트
        /// <summary>
        /// 파일 항목을 더블 클릭했을 때 발생하는 이벤트 핸들러입니다.
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">마우스 버튼 이벤트 인자</param>
        private void LoadPreviewImagesOnFileItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 오른쪽 마우스 버튼 클릭 시 처리하지 않음
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            try
            {
                ImageTreeViewItem clickItem = sender as ImageTreeViewItem;

                // 디렉토리 정보 가져오기
                DirectoryInfo directory = new DirectoryInfo(clickItem.Tag as string);
                FileInfo[] fileInfos = directory.GetFiles();

                // 파일이 존재하면 미리보기 이미지 로드 및 표시
                if (fileInfos.Length != 0)
                {
                    _imageManager.LoadAndDisplayPreviewImages(PictureViewer, clickItem.Tag.ToString());
                }
                e.Handled = true;

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }

        }

        #endregion




        #region 파일 미리보기 이벤트 
        /// <summary>
        /// 파일 항목을 더블 클릭했을 때 발생하는 이벤트 핸들러입니다.
        /// </summary>
        /// <param name="sender">이벤트 소스</param>
        /// <param name="e">마우스 버튼 이벤트 인자</param>
        private void LoadAndDisplayPreviewImagesOnFileItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 오른쪽 마우스 버튼 클릭 시 처리하지 않음
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            try
            {
                //아이템 초기화
                PictureViewer.Items.Clear();

                ImageTreeViewItem clickItem = sender as ImageTreeViewItem;

                // 디렉토리 정보 가져오기
                DirectoryInfo directory = new DirectoryInfo(clickItem.Tag as string);
                FileInfo[] fileInfos = directory.GetFiles();

                if (fileInfos.Length == 0)
                    return;


                int imageCount = fileInfos.Length;

                // ListBox 열 수 계산
                int columns = _imageManager.CalculateColumns((int)PictureViewer.ActualWidth);
                int rows = (imageCount / columns) + 1;

                // ListBox에 DataContext 설정
                PictureViewer.DataContext = new PreviewGridSize(rows, columns);

                // 파일이 존재하면 미리보기 이미지 로드 및 표시
                if (fileInfos.Length != 0)
                {
                    DisplayImagePreviewsFromFiles(fileInfos);
                }
                    
                e.Handled = true;

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }

        }

        /// <summary>
        /// 지정된 파일 정보 배열에서 이미지 파일을 필터링하고, 해당 이미지를 UI에 미리보기로 표시합니다.
        /// </summary>
        /// <param name="fileInfos">미리보기를 원하는 파일의 정보 배열</param>
        private async void DisplayImagePreviewsFromFiles(FileInfo[] fileInfos)
        {
            await Task.Run(() =>
            {
                // 파일 이름 먼저
                foreach (FileInfo file in fileInfos)
                {
                    if (!_imageManager.IsImageExtension(file.FullName))
                    {
                        continue;
                    }
                    // UI 스레드로 디스패치하여 UI 요소 생성 및 초기화
                    Dispatcher.Invoke(() =>
                    {
                        //UI 요소의 생성: 'UI 요소'는 일반적으로 UI 스레드에서만 생성할 수 있습니다.UI 스레드 이외의 스레드에서 UI 요소를 생성하려고 시도하면 'System.InvalidOperationException' 예외가 발생합니다.
                        ImageBlock imageBlock = new ImageBlock(file.Name);
                        imageBlock._Image.Tag = file.FullName;
                        imageBlock._Image.MouseLeftButtonDown += ImageClickEvent;

                        PictureViewer.Items.Add(imageBlock);
                    },DispatcherPriority.Normal);

                    DisplayImageOnUIThread(file.FullName);


                }

            });

        }

        /// <summary>
        ///  지정된 이미지 파일을 UI 스레드에서 로드하여 미리보기 컨트롤에 표시합니다.
        /// </summary>
        /// <param name="fullName"></param>
        private async void DisplayImageOnUIThread(string fullName)
        {
            // 이미지 파일을 읽어와 MemoryStream에 저장합니다.
            using (MemoryStream memoryStream = _imageManager.LoadImageStream(fullName))
            {
                // 이미지 회전 여부를 확인합니다.
                bool rotateCheck = _imageManager.ImageFileRotateCheck(memoryStream);

                // 이미지를 지정된 크기로 리사이징한 후 MemoryStream에 저장합니다.
                using (MemoryStream ResizeMemoryStream = _imageManager.ResizeImage(memoryStream, 129, 129))
                {
                    // 이미지 리사이징에 실패한 경우 처리를 중단합니다.
                    if (ResizeMemoryStream == null)
                    {
                        return;
                    }
                    // MemoryStream의 위치를 처음으로 되돌립니다.
                    ResizeMemoryStream.Seek(0, SeekOrigin.Begin);

                    // UI 스레드로 비동기적으로 이미지를 표시합니다.
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();

                        if (rotateCheck) 
                        {
                            // 이미지가 회전되었다면 비트맵 이미지의 회전 속성을 설정합니다.
                            bitmapImage.Rotation = Rotation.Rotate90;
                        }
                        // 이미지를 캐시로 로드하고 스트림을 지정합니다.
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ResizeMemoryStream;
                        bitmapImage.EndInit();

                        // 이미지를 찾아서 미리보기 컨트롤에 표시합니다.
                        ImageBlock FindBlock = FindImageBlockByTag(PictureViewer, fullName);
                        {
                            if (FindBlock != null)
                            {
                                FindBlock._Image.Source = bitmapImage;
                            }
                        }


                    }, DispatcherPriority.Normal);
                }
            }
        }

      

        /// <summary>
        /// PictureViewer 내에서 특정 태그를 가진 ImageBlock을 찾는 보조 메서드
        /// </summary>
        /// <param name="pictureBox"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private ImageBlock FindImageBlockByTag(ListBox pictureBox, string tag)
        {
            foreach (var item in pictureBox.Items)
            {
                if (item is ImageBlock imageBlock && imageBlock._Image.Tag as string == tag)
                {
                    return imageBlock;
                }
            }
            return null;
        }







        #endregion

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

        #region 원본 이미지를 보는 이벤트 

        /// <summary>
        /// 이미지 미리보기에서 이미지를 클릭하면 원본 이미지를 확인할 수 있는 이벤트 핸들러입니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 UI 요소</param>
        /// <param name="e">마우스 이벤트 인자</param>
        private void ImageClickEvent(object sender, MouseEventArgs e)
        {
            Uri url = null;
            string path = null;
            if (sender is System.Windows.Controls.Image)
            {
                Image send = sender as Image;
                url = new Uri(send.Tag.ToString());
                path = send.Tag.ToString();
            }
            if (sender is TreeViewItem)
            {
                TreeViewItem treeViewItem = sender as TreeViewItem;
                url = new Uri(treeViewItem.Tag.ToString());
                path = treeViewItem.Tag.ToString();
            }

            if (url == null) return;

            BitmapImage bitmapImage = _imageManager.LoadOriginalImage(path);

            //Debug.WriteLine( bitmapImage.Metadata);


            mainContent.Source = bitmapImage;
        }

        #endregion



        #region 윈도우 Control 사이즈 변경 
        Point mouseDown;
        /// <summary>
        /// 분할기 컨트롤에서 마우스 왼쪽 버튼이 눌렸을 때 호출되는 이벤트 핸들러입니다.
        /// 현재 마우스 포인터의 위치를 저장하여 드래그 동작을 준비합니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 컨트롤</param>
        /// <param name="e">마우스 이벤트 인수</param>

        private void SplitterMouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouseDown = e.GetPosition(this);
            }
        }

        /// <summary>
        /// 분할 창의 드래그 종료 시 동작하는 이벤트 핸들러입니다.
        /// 드래그 종료 시 화면 요소의 크기 및 위치를 조절합니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 컨트롤</param>
        /// <param name="e">마우스 이벤트 관련 인수</param>
        private void SplitterMouseupEvent(object sender, MouseEventArgs e)
        {
            // 드래그 종료 시 마우스 포인터 위치를 기록
            Point mouseup = e.GetPosition(this);
            double temp_x = (mouseup.X - mouseDown.X);

            // View 요소의 너비 조절
            view.Width = view.Width + temp_x;
            
            PreViewGrid.Width = view.Width;
            ContentView.Width = ContentView.Width - temp_x;

            //컨트롤 사이즈 설정 
            ViewSize.default_width = view.Width;
            ContenViewtSize.default_width = ContentView.Width;
            PreViewSize.default_width = PreViewGrid.Width;

            // View와 ContentView의 분할선 위치 및 너비 조절
            double contentSplitterLocation = view.Width + view.Margin.Left + view.Margin.Right;
            view_contentSplitter.Margin = new Thickness(contentSplitterLocation, 0, 0, 0);
        }

        private ControlResize TempCroquisSize;
        private ControlResize TempViewSize;
        private ControlResize TempContentSize;
        private ControlResize TempOptionSize;
        private ControlResize TempFileSize;
        private ControlResize TempPreViewSize;

        /// <summary>
        /// 윈도우 창의 크기가 변경될 때 발생하는 이벤트 핸들러입니다.
        /// 창 크기 변경 시 FileGrid와 PreViewGrid의 높이가 조절됩니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 컨트롤</param>
        /// <param name="e">크기 변경 관련 이벤트 인수</param>
        private void WinFormResizeEvent(object sender, SizeChangedEventArgs e)
        {
            try
            {
                TempCroquisSize = new ControlResize(croquisView.Width, croquisView.RenderSize.Height);
                TempViewSize = new ControlResize(view.Width, view.RenderSize.Height);
                TempContentSize = new ControlResize(ContentView.Width, ContentView.Height);
                TempOptionSize = new ControlResize(OptionView.Width, OptionView.Height);
                TempFileSize = new ControlResize(FileGrid.Width, FileGrid.Height);
                TempPreViewSize = new ControlResize(PreViewGrid.Width, PreViewGrid.Height);

                // 현재 창 크기와 기존 크기와의 차이를 계산하여 크기 조절에 활용
                ControlSize tempsize = MainFormSize.minus(MainWin.Width, MainWin.Height);
                double _tempH = tempsize.height / 2;

                if (fullDisplay.IsChecked == true)
                {
                    return;
                }


                if (tempsize.height != 0 && tempsize.width != 0)
                {
                    // 창이 최대화되지 않은 상태에서는 FileGrid와 PreViewGrid의 높이를 조절
                    FileGrid.Height = FileGridSize.plusHeight(_tempH);
                    PreViewGrid.Height = PreViewSize.plusHeight(_tempH);

                    // ContentView의 너비 조절
                    ContentView.Width = ContenViewtSize.plusWidth(tempsize.width - 2);
                }


                if ((WindowState == WindowState.Maximized) || tempsize.height == 0 && tempsize.width == 0)
                {
                    // 창이 최대화된 상태에서는 FileGrid와 PreViewGrid의 높이를 조절
                    double MaxWindow_H = MainGrid.ActualHeight / 2;


                    //FileGrid.Height = MaxWindow_H - 3; // 상단 여백 값 조정
                    //PreViewGrid.Height = MaxWindow_H - 3; // 하단 여백 값 조정

                    // ContentView의 너비 조절
                    double temp = MainWin.RenderSize.Width - view.Width - croquisView.Width - OptionView.Width - ContentView.Width;
                    ContentView.Width = (ContentView.Width + temp) - 26;
                }
                else
                {

                    croquisView.Width = TempCroquisSize.default_width;
                    croquisView.Height = TempCroquisSize.default_height;

                    view.Width = TempViewSize.default_width;
                    view.Height = TempViewSize.default_height;



                    ContentView.Width = TempContentSize.default_width;
                    ContentView.Height = TempContentSize.default_height;

                    OptionView.Width = TempOptionSize.default_width;
                    OptionView.Height = TempOptionSize.default_height;
                }
            } catch (NullReferenceException ex)
            {
                log.LogWrite(ex);
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreViewGridResizeEvent(object sender, SizeChangedEventArgs e)
        {
            int col = ((int)e.NewSize.Width) / ImageSize;
            int ItemCount = PictureViewer.Items.Count;
            int row = (ItemCount / col) + 1;
            PictureViewer.DataContext = new PreviewGridSize(row, col);
        }


        #endregion



        #region 크로키 시작 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void start_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FullDisplay();


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

            int timer = 0;
            int refreshTimer = 0;

            try
            {
                timeCheck(out timer, out refreshTimer);

                croquisPlay.Interval = timer;
                croquisPlay.RefreshInterval = refreshTimer;
            } catch (FormatException FormatException)
            {
                WarningBox("시간은 초 단위만 사용할 수 있습니다. ");
            }

            if (button.Content.ToString().Equals("시작"))
            {

                button.Content = "중지";
                if (imagePath.Count > 0)
                {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="refreshTimer"></param>
        /// <exception cref="FormatException"></exception>
        private void timeCheck(out int timer,out int refreshTimer)
        {
            bool time = int.TryParse(timerInput.Text, out timer);
            bool refreshTime = int.TryParse(refreshTimerInput.Text, out refreshTimer);

            if(!time || !refreshTime)
            {
                throw new FormatException();
            }

        }
        

        /// <summary>
        /// 테스트 중
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private void showImage(string path)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                
                mainContent.Source = _imageManager.LoadOriginalImage(path);

            }));
        }


        /// <summary>
        /// 테스트 중
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //private Dispatcher showImage(string path)
        //{

        //    return (Dispatcher)Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        //    {

        //        mainContent.Source = _imageManager.LoadOriginalImage(path);

        //    }));
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
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

        #endregion

        #region 기타
        private void ThreadSleep(int time)
        {
            Thread.Sleep(time);
        }
        private void WarningBox(string message)
        {
            MessageBox.Show(message, "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        #endregion

        
    }

}
