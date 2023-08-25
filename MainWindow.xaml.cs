using initializationControl;
using System;

using System.Collections.Generic;
using System.Diagnostics;

using System.IO;

using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

    public partial class MainWindow : Window
    {
        private ImageTreeViewItemFactory _itemFactory;
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

         private double PreGridMinSize = 328;

        //파일 시스템 감지 
        private FileSystemWatcher watcher;

        public MainWindow()
        {
            InitializeComponent();
            SetAllowDrop();
            InitImageTreeViewFactory();
            InitImageManager();

            PreViewGrid.Width = PreGridMinSize;
            



            croquisPlay = new CroquisPlay(ThreadSleep, showImage);
            MainFormSize = new ControlResize(MainWin.Width, MainWin.Height);
            FileGridSize = new ControlResize(FileGrid.Width, FileGrid.Height);
            PreViewSize = new ControlResize(PreViewGrid.Width, PreViewGrid.Height);
            ViewSize = new ControlResize(view.Width, view.Height);
            ContenViewtSize = new ControlResize(ContentView.Width, ContentView.Height);

            //컨텐츠 테스트
            ViewMinSIze = view.Width;
            ContentMinSize = ContentView.Width;

            croquisTreeView.Drop += CroquisTreeDropEvent;
            Run();
        }
        #region 테스트 중 로컬드라이브에서 파일 또는 디렉토리가 변경되면 감지해 파일 탐색기에 반영된다.

        
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
        /// 윈도우 Load 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoadEvent(object sender, RoutedEventArgs e)
        {
            //디렉토리 가져오기 
            _directoryManager.GetLocalDrives(DirectoryView);


        }

        /// <summary>
        /// 
        /// </summary>
        private void InitImageManager()
        {
            _imageManager = new ImageManager()
            {

                ImageClick = ImageClickEvent
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitImageTreeViewFactory()
        {
            _itemFactory = new ImageTreeViewItemFactory.FactoryBuilder()
                .SetExpanded(new RoutedEventHandler(itemExpanded))
                .SetPreviewMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                .SetMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                .SetPreviewMouseDown(PreViewMouseLeftButtonDownEvent)

                .SetPreviewMouseLeftButtonDown(ImageClickEvent)
                .SetPreviewMouseRightButtonDown(CroquisRightClickEvent)
                .Build();


            _directoryManager = new DirectoryManager(_itemFactory);


        }
        #endregion


        private void Run()
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

            watcher.Changed += watcher_Change;
            watcher.Deleted += watcher_Delete;
            watcher.Created += watcher_Create;
            watcher.Renamed += watcher_Rename;

        }

        /// <summary>
        /// 디렉토리가 추가될 때 발생하는 이벤트  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watcher_Create(object sender, FileSystemEventArgs e)
        {
            string root = Path.GetPathRoot(e.FullPath);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                foreach (ImageTreeViewItem item in DirectoryView.Items)
                {
                    string rootPath = item.Tag.ToString();
                    if (rootPath.Equals(root))
                    {
                        CreateDirectoryTree(item, e.FullPath);
                    }
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
            if(e.ChangeType != WatcherChangeTypes.Changed) return;

            Debug.WriteLine($"변화된 파일 경로 {e.FullPath}");
            Debug.WriteLine($"변화된 파일 경로 {Path.GetDirectoryName(e.FullPath)}");
        }


        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watcher_Delete(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"삭제된 파일 경로 {e.FullPath}");
            Debug.WriteLine($"삭제된 파일 경로 {Path.GetDirectoryName(e.FullPath)}");

            string root = Path.GetPathRoot(e.FullPath);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
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

                 
            }));

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watcher_Rename(object sender, RenamedEventArgs e)
        {

            Debug.WriteLine($"이름이 바뀐 파일 경로 {e.OldFullPath}");
            Debug.WriteLine($"이름이 바뀐 파일 경로 {e.FullPath}");
            Debug.WriteLine($"이름이 바뀐 파일 경로 {Path.GetDirectoryName(e.FullPath)}");
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
                if (PictureViewer.Items.Count == 0) return;
                foreach (System.Windows.Controls.Image item in PictureViewer.Items)
                {
                    if (item.Tag.ToString().Equals(DeletePath))
                    {
                        tree.Items.Remove(item);
                    }
                }
            } catch (InvalidOperationException e)
            {
                
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
            } catch(InvalidOperationException e)
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
                    ImageTreeViewItem item = ImageTreeViewItem.createImageTreeViewItem(GetIcomImage(NewPath), TreeTagPathFileName, NewPath);
                    item.Expanded += new RoutedEventHandler(itemExpanded);
                    item.PreviewMouseDoubleClick += LoadAndDisplayPreviewImagesOnFileItemDoubleClick;
                    item.PreviewMouseDown += PreViewMouseLeftButtonDownEvent;
                    //item.MouseEnter += SourceTreeViewEntryMouseEvent;

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
                    ImageTreeViewItem SendItem = sender as ImageTreeViewItem;
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
            IntPtr hImgSmall; //system image list 
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
                    _imageManager.LoadAndDisplayPreviewImages(PictureViewer,clickItem.Tag.ToString());
                }
                e.Handled = true;

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }

         }

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
                PictureViewer.DataContext = new PictureBoxRowCols(rows, columns);

                // 파일이 존재하면 미리보기 이미지 로드 및 표시
                if (fileInfos.Length != 0)
                {
                    ShowPreviewImages(fileInfos);
                }
                    
                e.Handled = true;

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }

        }

        /// <summary>
        /// 이미지 미리보기를 표시하는 메서드 
        /// </summary>
        /// <param name="fileInfos">파일 정보 배열</param>
        private async void ShowPreviewImages(FileInfo[] fileInfos)
        {
            await Task.Run(async () =>
            {
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
        /// 
        /// </summary>
        /// <param name="fullName"></param>
        private void DisplayImageOnUIThread(string fullName)
        {
            Dispatcher.Invoke(async() =>
            {
                BitmapImage bitmapImage = await _imageManager.LoadBitmapImageAsync(fullName);

                ImageBlock FindBlock = FindImageBlockByTag(PictureViewer, fullName);
                {
                    if (FindBlock != null)
                    {
                        FindBlock._Image.Source = bitmapImage;
                    }
                }
            }, DispatcherPriority.Normal);

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

     



        /// <summary>
        /// 이미지 미리보기에서 이미지를 클릭하면 원본 이미지를 확인할 수 있다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterMouseupEvent(object sender, MouseEventArgs e)
        {
            Point mouseup = e.GetPosition(this);
            double temp_x = (mouseup.X - mouseDown.X);

            view.Width = view.Width + temp_x;
            PreViewGrid.Width = view.Width;
            ContentView.Width = ContentView.Width - temp_x;
            ContenViewtSize.default_width = ContentView.Width;

            double contentSplitterLocation = view.Width + view.Margin.Left + view.Margin.Right;

            view_contentSplitter.Margin = new Thickness(contentSplitterLocation, 0, 0, 0);
            view_contentSplitter.Width = 4;
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
            double _tempH = tempsize.height / 2;

            //Debug.WriteLine(tempsize.width);
            if (WindowState == WindowState.Maximized)
            {
                double MaxWindow_H = MainGrid.ActualHeight / 2;

                

                FileGrid.Height = MaxWindow_H - 3; // margin 값 수치 조정 
                PreViewGrid.Height = MaxWindow_H - 3; // margin 값 수치 조정 

                //contentView 추가 width 크기 재설정
                double temp = MainWin.RenderSize.Width - view.Width - croquisView.Width - OptionView.Width - ContentView.Width;
                Debug.WriteLine(temp);
                ContentView.Width = (ContentView.Width + temp) - 26;
            }
            else
            {
                //파일 그리드와 미리보기 그리드의 Height를 재설정한다.
                FileGrid.Height = FileGridSize.plusHeight(_tempH);
                PreViewGrid.Height = PreViewSize.plusHeight(_tempH);

                //contentView 추가 width 크기 재설정
                ContentView.Width = ContenViewtSize.plusWidth(tempsize.width - 2);
            }
        }
        #endregion

        #region
        /// <summary>
        /// 
        /// </summary>
        Point mouseDown;
        private void splitterMouseDownEvent(object sender ,MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                mouseDown = e.GetPosition(this);
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
            PictureViewer.DataContext = new PictureBoxRowCols(row, col);
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
