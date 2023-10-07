using croquis.Entity;
using ModernWpf.Controls;
using OpenCvSharp;
using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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

    public partial class MainWindow : System.Windows.Window
    {
        private ExceptionLogger log;
        private FileWatcher fileWatcher;
        private ImageTreeViewItemFactory imageTreeViewItemFactory;
        private DirectoryManager directoryManager;
        private ImageStream imageStream;
        private ImageManager imageManager;
        private ImageCacheRepository imageCacheRepository;

        private UserSettingsManager userSettingsManager;


        private System.Windows.Point _lastMouseDown;

        /// <summary>
        /// 
        /// </summary>
        private ControlResize mainWindowSize;
        private ControlResize mainGridSize;

        private ControlResize croquisBorderSize;

        private ControlResize fileViewGridSize;
        private ControlResize fileSearchBorderSize;
        private ControlResize previewBorderSize;
        

        private ControlResize contentBorderSize;
        private ControlResize optionViewSize;

        private ControlResize croquisSplitterSize;
        private ControlResize viewContentSplitterSize;

        //----------------------------------------------------

        private double ViewMinSIze;
        private double ContentMinSize;

        private CroquisPlay croquisPlay;

        private static int ImageSize = 129;

        private double PreGridMinSize;


        public MainWindow()
        {
            log = new ExceptionLogger();
            InitializeComponent();
            SetAllowDrop();
            InitImageTreeViewFactory();
            InitImageManager();
            

            imageCacheRepository = new ImageCacheRepository();

            fileWatcher = new FileWatcher(directoryManager, imageTreeViewItemFactory, log)
            {
                DirectoryView = DirectoryView,
                PictureViewer = PictureViewer,
            };
            fileWatcher.Config();
            //테스트
            DisplayVisibility();

            croquisPlay = new CroquisPlay(ThreadSleep, showImage);
            croquisPlay.PlayButton = start;

            CroquisTreeView.Drop += CroquisTreeDropEvent;

            //fullDisplay.Click += FullDisplay;

            imageManager = new ImageManager(directoryManager, imageStream, imageCacheRepository);


            MainWin.PreviewKeyDown += EndFullDisplayButton;
            MainGrid.KeyDown += EndFullDisplayButton;

            //
            imageCacheRepository = new ImageCacheRepository();

            userSettingsManager = new UserSettingsManager(log,directoryManager,imageTreeViewItemFactory);

            LoadFile();
        }




        /// <summary>
        /// 여러 컨트롤의 초기 크기를 설정하고 관련 정보를 저장하는 메서드입니다.
        /// </summary>
        private void InitControlSize()
        {
            mainWindowSize = new ControlResize(MainWin.Width, MainWin.Height, MainGrid.Margin);
            mainGridSize = new ControlResize(MainGrid.ActualWidth, MainGrid.ActualHeight, MainGrid.Margin);

            fileViewGridSize = new ControlResize(FileViewGrid.Width, FileViewGrid.Height, FileViewGrid.Margin);
            croquisBorderSize = new ControlResize(CroquisBorder .Width, CroquisBorder.ActualHeight, CroquisBorder.Margin);

            fileSearchBorderSize = new ControlResize(FileSearchBorder.Width, FileSearchBorder.Height,DirectoryView.Margin);
            previewBorderSize = new ControlResize(PreviewBorder.Width, PreviewBorder.Height, PreviewBorder.Margin);
            
            contentBorderSize = new ControlResize(ContentBorder.Width, ContentBorder.ActualHeight,ContentBorder.Margin);
            optionViewSize = new ControlResize(OptionBorder.Width, OptionBorder.Height,OptionBorder.Margin);


            croquisSplitterSize = new ControlResize(CroquisSplitter.Width, CroquisSplitter.Height, CroquisSplitter.Margin);
            viewContentSplitterSize = new ControlResize(ViewContentSplitter.Width, ViewContentSplitter.Height, ViewContentSplitter.Margin);

            //컨텐츠 테스트
            ViewMinSIze = FileViewGrid.Width;

            PreviewBorder.Width = FileViewGrid.Width;
            ContentMinSize = ContentBorder.Width;
        }


        ImageSourceValueSerializer imageSourceValueSerializer = new ImageSourceValueSerializer();

        #region 윈도우 시작, 종료 이벤트
        /// <summary>
        /// 윈도우 Load 이벤트입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoadEvent(object sender, RoutedEventArgs e)
        {
            InitControlSize();
            //디렉토리 가져오기 
            directoryManager.GetLocalDrives(DirectoryView);

            MainWin.SizeChanged += new System.Windows.SizeChangedEventHandler(this.WinFormResizeEvent);
            MainWin.StateChanged += this.WindowStateChangedEvent;

            //제목표시줄 
            CreateSubjectMenu(SubjectMenu);

        }

        private void CreateSubjectMenu(Menu menu)
        {
            //MenuItem mainMenu = new MenuItem();
            //TextBlock textBlock = new TextBlock();
            //textBlock.TextAlignment = TextAlignment.Center;
            //textBlock.Text = "창";
            //mainMenu.Header = textBlock;
            //mainMenu.Height = 20;
            //mainMenu.Width = 60;
            //mainMenu.Padding = new Thickness(0);
            //mainMenu.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD8D8D8"));




            //menu.Items.Add(mainMenu);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CroquisTreeView == null)
            {
                return;
            }



            SaveFile();


        }

        #endregion

        #region 북마크, 크로키 목록 저장 또는 불러오기 

        /// <summary>
        /// 
        /// </summary>
        public void SaveFile()
        {
            if (System.Windows.MessageBox.Show("크로키 기록을 저장하시겠습니까?", "croquis", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // 크로키 목록 저장
                userSettingsManager.SaveTreeViewDataAsJson(CroquisTreeView, FilePath.croquisPath);

                //북마크 저장
                userSettingsManager.SaveTreeViewDataAsJson(BookMarkView, FilePath.bookMarkPath);

            }
        }



        public void LoadFile()
        {
            //크로키 목록 불러오기 
            userSettingsManager.LoadCroquisTree(CroquisTreeView, FilePath.croquisPath);

            //북마크 불러오기 
            userSettingsManager.LoadCroquisTree(BookMarkView, FilePath.bookMarkPath);
        }

        #endregion


        #region TopMost체크박스, 항상 맨위 
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




        


        #region 초기화
        /// <summary>
        /// Drag & Drop 설정
        /// </summary>
        private void SetAllowDrop()
        {
            DirectoryView.AllowDrop = true;
            CroquisTreeView.AllowDrop = true;
        }

        

        

        /// <summary>
        /// 
        /// </summary>
        private void InitImageManager()
        {
            imageStream = new ImageStream(log)
            {

                ImageClick = LoadAndDisplayOriginalImageOnClick
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitImageTreeViewFactory()
        {
            imageTreeViewItemFactory = new ImageTreeViewItemFactory.FactoryBuilder()
                .SetExpanded(new RoutedEventHandler(ItemExpanded))

                .SetMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                .SetPreviewMouseDoubleClick(LoadAndDisplayPreviewImagesOnFileItemDoubleClick)
                
                .SetPreviewMouseDown(PreViewMouseLeftButtonDownEvent)

                
                .SetPreviewMouseLeftButtonDown(LoadAndDisplayOriginalImageOnClick)
                .SetMouseRightButtonDown(CroquisRightClickEvent)

                .SetBookMarKImageLeftButtonDown(BookMarkClickEvent)
                .Build();


            directoryManager = new DirectoryManager(imageTreeViewItemFactory, log);


        }
        #endregion

        #region 윈도우 크기 조정

        enum ResizeDirection
        {
            None,
            Left,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft
        }

        private ResizeDirection _resizeDirection = ResizeDirection.None;

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {


            if (e.LeftButton == MouseButtonState.Released)
            {
                _resizeDirection = GetResizeDirection(e.GetPosition(this));
                switch (_resizeDirection)
                {
                    case ResizeDirection.Left:
                    case ResizeDirection.Right:
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case ResizeDirection.Top:
                    case ResizeDirection.Bottom:
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case ResizeDirection.TopLeft:
                    case ResizeDirection.BottomRight:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case ResizeDirection.TopRight:
                    case ResizeDirection.BottomLeft:
                        this.Cursor = Cursors.SizeNESW;
                        break;
                    default:
                        this.Cursor = Cursors.Arrow;
                        break;
                }
            }
            else
            {
                //실제 이벤트 동작
                if(_resizeDirection == ResizeDirection.Left)
                {
                    //Debug.WriteLine(e.GetPosition(this));
                    //Debug.WriteLine(System.Windows.Forms.Cursor.Position.X);

                    this.Width -= e.GetPosition(this).X;
                    this.Left += e.GetPosition(this).X;
                }


              
            }

            e.Handled = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _resizeDirection = GetResizeDirection(e.GetPosition(this));
        }


        private ResizeDirection GetResizeDirection(System.Windows.Point position) 
        {
            const int resizeBorder = 10;

            if(position.X < resizeBorder)
            {
                if (position.Y < 0) return ResizeDirection.TopLeft;
                if (position.Y > this.ActualHeight - resizeBorder) return ResizeDirection.BottomLeft;
                return ResizeDirection.Left;
            }

            if(position.X > this.ActualWidth - resizeBorder) 
            {
                if (position.Y < resizeBorder) return ResizeDirection.TopRight;
                if (position.Y > this.ActualHeight - resizeBorder) return ResizeDirection.BottomRight;
                return ResizeDirection.Right;
            }

            if (position.Y < resizeBorder) return ResizeDirection.Top;
            if (position.Y > this.ActualHeight - resizeBorder) return ResizeDirection.Bottom;


            return ResizeDirection.None;
        }

        #endregion







        #region 최소화, 최대화, 닫기버튼, 윈도우 이동

        private void WindowMove_Event(object sender, MouseButtonEventArgs e) 
        {
            this.DragMove();
            
        }

        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                MaximizeButton_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }



        #endregion


        #region 화면 조절 
        public void WindowChangeEvent(object sender, KeyEventArgs e)
        {
            //크로키 목록  
            if(e.Key == Key.F1) 
            {
                ChangeCroquisView();
            }

            //탐색기 
            if(e.Key == Key.F2)
            {
                ChangeFileViewGrid();
            }
            //옵션 
            if (e.Key == Key.F3)
            {
                ChangeOptionBorder();
            }


            if (e.Key == Key.F4)
            {
            }

            //이미지 넘기기 
            if (e.Key == Key.Tab)
            {
            }

            //이미지 넘기기 
            if (e.Key == Key.Space)
            {
                 CroquisStopEvent();
            }

            
        }

        public void CroquisStopEvent()
        {
            if(croquisPlay.Status == PlayStatus.Play)
            {
                croquisPlay.Skip();
            } 
        }


        /// <summary>
        /// 
        /// </summary>
        public void ChangeCroquisView()
        {
            try
            {
                double croquisBorderwWidth = CroquisBorder.Width;
                double contentBorderwWidth = ContentBorder.Width;

                if (CroquisBorder.Visibility == Visibility.Visible)
                {
                    //크로키 목록 없애기  
                    CroquisBorder.Visibility = Visibility.Collapsed;
                    //화면조절 기능 해제 
                    CroquisSplitter.Visibility = Visibility.Collapsed;

                    //파일 탐색기의 위치를 크로키 목록 위치로 변경 
                    FileViewGrid.Margin = CroquisBorder.Margin;

                    //ViewContentSplitter 위치 변경 
                    ViewContentSplitter.Margin = new Thickness(FileViewGrid.Width,0,0,0);

                    //삭제되는 만큼 ContentView의 width 증가 
                    ContentBorder.Width = croquisBorderwWidth + contentBorderwWidth + CroquisSplitter.Width; 

                    return;
                }

                
                if (CroquisBorder.Visibility == Visibility.Collapsed)
                {
                    CroquisBorder.Visibility = Visibility.Visible;
                    //화면조절 기능 추가 
                    CroquisSplitter.Visibility = Visibility.Visible;

                    //ViewContentSplitter 위치 변경 
                    ViewContentSplitter.Margin = viewContentSplitterSize.DefaultLocation;

                    //원래 위치로 변경 
                    FileViewGrid.Margin = fileViewGridSize.DefaultLocation;

                    //ContentView의 width 감소
                    ContentBorder.Width = contentBorderwWidth - croquisBorderwWidth - CroquisSplitter.Width;

                    return;
                }


            } catch (Exception ex) 
            {

            }
            
        }

        public void ChangeFileViewGrid()
        {
            try
            {
                double FileViewGridWidth = FileViewGrid.Width;
                double ContentBorderWidth = ContentBorder.Width;

                if (FileViewGrid.Visibility == Visibility.Visible)
                {
                    FileViewGrid.Visibility = Visibility.Collapsed;

                    //화면조절 기능 해제 
                    ViewContentSplitter.Visibility = Visibility.Collapsed;


                    //삭제되는 만큼 ContentView의 width 증가 
                    ContentBorder.Width = FileViewGridWidth + ContentBorderWidth + ViewContentSplitter.Width;

                    return;
                }

                if (FileViewGrid.Visibility == Visibility.Collapsed)
                {

                    FileViewGrid.Visibility = Visibility.Visible;
                    //화면조절 기능 추가 
                    ViewContentSplitter.Visibility = Visibility.Visible;

                    //삭제되는 만큼 ContentView의 width 증가 
                    ContentBorder.Width = ContentBorderWidth - FileViewGridWidth - ViewContentSplitter.Width;

                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ChangeOptionBorder()
        {
            try
            {
                double optionBorderWidth = OptionBorder.Width;
                double contentViewWidth = ContentBorder.Width;

                if (OptionBorder.Visibility == Visibility.Visible)
                {
                    //OptionBorder 숨기기
                    OptionBorder.Visibility = Visibility.Collapsed;

                    //ContentView의 위치 이동
                    ContentBorder.Margin = OptionBorder.Margin;

                    //삭제되는 만큼 ContentView의 width 증가 
                    ContentBorder.Width = optionBorderWidth + contentViewWidth + 2;

                    return;
                }

                if (OptionBorder.Visibility == Visibility.Collapsed)
                {
                    //OptionBorder 보이기
                    OptionBorder.Visibility = Visibility.Visible;

                    //ContentView의 위치 이동
                    ContentBorder.Margin = contentBorderSize.DefaultLocation;

                    ContentBorder.Width = contentViewWidth - optionBorderWidth - 2 ;

                    return;
                }
            }
            catch (Exception ex)
            {

            }

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
            string[] outFile = (string[])e.Data.GetData(DataFormats.FileDrop);

            CreateImageTreeViewItem(obj);
            CreateStringArray(outFile);
        }

        public void CreateImageTreeViewItem(ImageTreeViewItem item)
        {
            if (item == null) return;

            ImageTreeViewItem subject = ImageTreeViewItem.createImageTreeViewItem(item);

            subject.Expanded += new RoutedEventHandler(TargetTreeViewItemExpanded);
            subject.MouseDoubleClick += LoadAndDisplayPreviewImagesOnFileItemDoubleClick;
            subject.MouseRightButtonDown += CroquisRightClickEvent;

            directoryManager.TargetGetDirectories(subject);
            directoryManager.TargetGetFile(subject);

            CroquisTreeView.Items.Add(subject);
        }

        public void CreateStringArray(string[] strings)
        {
            if(strings == null || strings.Length == 0 ) return;

            foreach(string s in strings) 
            {
                DirectoryInfo directory = new DirectoryInfo(s);
                if (directory.Exists) 
                {
                    ImageTreeViewItem item = imageTreeViewItemFactory.CreateTargetGetDirectories(directoryManager.GetIcomImage(directory.FullName),directory.Name,directory.FullName);
               

                    FileInfo[] files = directory.GetFiles();
                    foreach (FileInfo file in files) 
                    {
                        if (directoryManager.IsImageExtension(file.Name))
                        {
                            ImageTreeViewItem fileitem = imageTreeViewItemFactory.CreateTargetGetFile(file.Name, file.FullName);
                            item.Items.Add(fileitem);
                        }
                    }
                    CroquisTreeView.Items.Add(item);
                }

                FileInfo fileInfo = new FileInfo(s);
                if(fileInfo.Exists) 
                {
                    ImageTreeViewItem fileitem = imageTreeViewItemFactory.CreateTargetGetFile(fileInfo.Name, fileInfo.FullName);
                    CroquisTreeView.Items.Add(fileitem);
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
            ImageTreeViewItem item = sender as ImageTreeViewItem;

            if (item == null) return;

            foreach (ImageTreeViewItem subItem in item.Items)
            {
                directoryManager.TargetGetDirectories(subItem);
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
            ImageTreeViewItem clickItem = GetTreeViewItemParent(e.OriginalSource as DependencyObject);

            if (clickItem == null) return;
            clickItem.IsSelected = true;

            System.Windows.Point point = e.GetPosition(clickItem);


            if (clickItem.IsSelected == true)
            {
                CroquisTreeViewContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;

                // 메뉴 열기
                CroquisTreeViewContextMenu.IsOpen = true;
            }
            // 기본 오른쪽 클릭 이벤트를 취소하여 ContextMenu의 기본 동작을 방지함

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        private ImageTreeViewItem GetTreeViewItemParent(DependencyObject child)
        {
            while (child != null && !(child is ImageTreeViewItem))
            {
                child = VisualTreeHelper.GetParent(child);
            }

            return child as ImageTreeViewItem;
        }

        #endregion

        #region 이미지 회전 이벤트
        
        /// <summary>
        /// 시계방향으로 회전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RotateClockwise90Degrees(object sender, RoutedEventArgs e)
        {

            ImageTreeViewItem findImageTreeViewItem = directoryManager.FindSelectedItem(CroquisTreeView);

            string ImagePath = findImageTreeViewItem.FullName;

            //확장자가 없으면 종료(디렉토리면 종료) > 디렉토리면 전체 변경 
            if (!directoryManager.IsImageExtension(ImagePath))
                return;

            if (findImageTreeViewItem.ImageCache == null)
            {
                findImageTreeViewItem.ImageCache = imageManager.CreateImageCacheFromTree(findImageTreeViewItem);
            }
            findImageTreeViewItem.ImageCache.RotateClockwise();

            imageManager.ChangeMainWin(mainContent, findImageTreeViewItem);

            PreViewImageChange(PictureViewer, findImageTreeViewItem);

        }

        /// <summary>
        /// 시계 반대방향으로 회전 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RotateCounterClockwise90Degrees(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem findImageTreeViewItem = directoryManager.FindSelectedItem(CroquisTreeView);

            string ImagePath = findImageTreeViewItem.FullName;
            if (!directoryManager.IsImageExtension(ImagePath))
                return;
            if (findImageTreeViewItem.ImageCache == null)
            {
                findImageTreeViewItem.ImageCache = imageManager.CreateImageCacheFromTree(findImageTreeViewItem);
            }
            findImageTreeViewItem.ImageCache.RotateCounterClockwise();


            imageManager.ChangeMainWin(mainContent, findImageTreeViewItem);

            imageManager.DisplayImageOnUIThreadTree(PictureViewer, findImageTreeViewItem);
        }

        #endregion


        #region 이미지 흑백 전환 이벤트

        /// <summary>
        /// 이미지를 흑백으로 변경해서 보여준다. OpenCV를 사용해 GPU를 이용해야한다.
        /// CPU를 이용해 흑백이미지 변환 시 많이 느려짐 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertToGrayscaleEvent(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem findImageTreeViewItem = directoryManager.FindSelectedItem(CroquisTreeView);

            string ImagePath = findImageTreeViewItem.FullName;
            if (!directoryManager.IsImageExtension(ImagePath))
                return;

            imageManager.ConvertToGrayscale(findImageTreeViewItem);

            imageManager.ChangeMainWin(mainContent, findImageTreeViewItem);

            imageManager.DisplayImageOnUIThreadTree(PictureViewer, findImageTreeViewItem);
        }

        #endregion

        #region 이미지 좌우 반전 이벤트

        /// <summary>
        /// 좌우반전 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertToFlipEvent(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem findImageTreeViewItem = directoryManager.FindSelectedItem(CroquisTreeView);

            string ImagePath = findImageTreeViewItem.FullName;
            if (!directoryManager.IsImageExtension(ImagePath))
                return;

            //좌우 반전 
            imageManager.ConvertToFlip(findImageTreeViewItem);

            imageManager.ChangeMainWin(mainContent, findImageTreeViewItem);
            
            imageManager.DisplayImageOnUIThreadTree(PictureViewer,findImageTreeViewItem);
        }
        #endregion



        #region 크로키 목록에서 폴더 or 파일을 삭제 


        /// <summary>
        /// 선택된 Croquis 항목을 TreeView에서 삭제하는 이벤트 핸들러입니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체입니다.</param>
        /// <param name="e">이벤트 데이터를 포함하는 MouseButtonEventArgs 객체입니다.</param>
        private void RemoveCroquisItem(object sender, MouseButtonEventArgs e)
        {
            //삭제할 아이템을 찾는다. 
            ImageTreeViewItem findImageTreeViewItem = directoryManager.FindSelectedItem(CroquisTreeView);

            //삭제할 아이템의 상위 아이템을 찾는다.
            object parent = findImageTreeViewItem.Parent;

            //만약 최상위 요소가 TreeView이면 
            if (parent != null && (parent is TreeView))
            {
                (parent as TreeView).Items.Remove(findImageTreeViewItem);

            }

            //만약 최상위 요소가 ImageTreeViewItem이면
            if (parent != null && (parent is ImageTreeViewItem))
            {
                (parent as ImageTreeViewItem).Items.Remove(findImageTreeViewItem);
                (parent as ImageTreeViewItem).IsSelected = false;

            }


        }

        #endregion


        #region 북마크 이벤트
        private void BookMarkClickEvent(object sender, MouseButtonEventArgs e)
        {
            Image clickImage = sender as Image;
            if (clickImage == null)
                return;

            ImageTreeViewItem clickImageTreeItem = directoryManager.getParent(clickImage);


            if (clickImageTreeItem == null)
                return;

            //북마크에 없으면 
            if (clickImageTreeItem.IsBookMarkSelected == false)
            {
                clickImageTreeItem.IsBookMarkSelected = true;
                //즐겨찾기에 저장 
                directoryManager.AddBookMarkTreeItem(BookMarkView, clickImageTreeItem);

                ImageSource fullStartImage = directoryManager.BookMarFullStarImage();

                clickImageTreeItem.BookMarkImageSource = fullStartImage;

                directoryManager.checkFullImage(clickImageTreeItem, fullStartImage);

                return;
            }

            if (clickImageTreeItem.IsBookMarkSelected == true)
            {
                //즐겨찾기에서 삭제 
                directoryManager.RemoveBookMark(BookMarkView, clickImageTreeItem);

                ImageTreeViewItem Item = directoryManager.FindImageTreeViewItem(DirectoryView, clickImageTreeItem);

                if (Item != null)
                {
                    //탐색기에서 즐겨찾기 기능 해제 
                    directoryManager.EmptyImage(DirectoryView, Item);
                }

            }

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
            originContentView = ContentBorder.Margin;
            originContentWidth = ContentBorder.Width;

            if (fullDisplay.IsChecked == true)
            {

                MainWin.WindowState = WindowState.Maximized;
                MainWin.Visibility = Visibility.Collapsed;
                MainWin.WindowStyle = WindowStyle.None;
                MainWin.ResizeMode = ResizeMode.NoResize;
                MainWin.Visibility = Visibility.Visible;
                MainWin.Topmost = true;


                CroquisBorder.Visibility = Visibility.Hidden;
                FileViewGrid.Visibility = Visibility.Hidden;
                OptionBorder.Visibility = Visibility.Hidden;

                ContentBorder.Width = MainGrid.RenderSize.Width;
                ContentBorder.Margin = new System.Windows.Thickness { Left = 2, Top = 2, Right = 2, Bottom = 2 };
            }

            //포커스 해제 
            fullDisplay.Focusable = false;
        }

        private void EndFullDisplayButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MainWin.WindowState = WindowState.Normal;
                MainWin.WindowStyle = WindowStyle.SingleBorderWindow;
                MainWin.ResizeMode = ResizeMode.CanResize;

                MainWin.Topmost = false;




                CroquisBorder.Visibility = Visibility.Visible;
                FileViewGrid.Visibility = Visibility.Visible;
                OptionBorder.Visibility = Visibility.Visible;

                ContentBorder.Width = originContentWidth;
                ContentBorder.Margin = originContentView;
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
            if (fileSearch.Visibility == Visibility.Visible)
            {
                Brush backgroundColor = new BrushConverter().ConvertFrom("#404040") as Brush;
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
                if (target == null) { return; };
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
                        BookMarkView.Background = backgroundColor2;

                        DirectoryView.Visibility = Visibility.Visible;
                        BookMarkView.Visibility = Visibility.Hidden;
                    }

                }

                if (target.Text.Equals("즐겨찾기"))
                {
                    Brush backgroundColor = new BrushConverter().ConvertFrom("#404040") as Brush;
                    Brush backgroundColor2 = new BrushConverter().ConvertFrom("#262626") as Brush;
                    if (bookMark.Visibility == Visibility.Visible)
                    {

                    }

                    if (BookMarkView.Visibility == Visibility.Hidden)
                    {
                        bookMark.Background = backgroundColor;
                        BookMarkView.Background = backgroundColor;

                        DirectoryView.Background = backgroundColor2;
                        fileSearch.Background = backgroundColor2;

                        DirectoryView.Visibility = Visibility.Hidden;
                        BookMarkView.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (NullReferenceException exception)
            {
                log.LogWrite(exception);
            }

        }

        #endregion

        

        #region 디렉토리 확장 이벤트

        /// <summary>
        /// 파일 탐색기 확장 이벤트 
        /// </summary>
        /// <param name="parent"></param>
        private void ItemExpanded(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem item = sender as ImageTreeViewItem;

            if (item == null) return;
            if (item.Items.Count == 0) return;

            foreach (ImageTreeViewItem subItem in item.Items)
            {
                directoryManager.GetSubDirectories(subItem);
            }

            //북마크 기능 추가 
            FindBookMarkTree(BookMarkView, DirectoryView);
        }


        #endregion


        public void FindBookMarkTree(TreeView bookMarkView,TreeView DirectoryView)
        {
            if(bookMarkView.Items.Count == 0) return;

            //북마크에서 
            foreach(ImageTreeViewItem subItem in bookMarkView.Items)
            {
                string fullName = subItem._FullName;
                bool isBookMark = subItem.IsBookMarkSelected;

                ImageTreeViewItem findItem = SearchFullName(fullName, DirectoryView);

                if(findItem != null) 
                {
                    ImageSource imageSource = directoryManager.BookMarkImage(subItem);

                    findItem.BookMarkImageSource = imageSource;

                }

                if(subItem.Items.Count != 0)
                {
                    SubFindBookMarkTree(subItem);
                }


            }

        }

        public void SubFindBookMarkTree(ImageTreeViewItem subItem)
        {
            foreach(ImageTreeViewItem imageTreeViewItem in subItem.Items)
            {
                string fullName = imageTreeViewItem._FullName;
                

                ImageTreeViewItem findItem = SearchFullName(fullName, DirectoryView);

                if (findItem != null)
                {
                    ImageSource imageSource = directoryManager.BookMarkImage(subItem);

                    findItem.BookMarkImageSource = imageSource;

                }

                if (imageTreeViewItem.Items.Count != 0)
                {
                    SubFindBookMarkTree(imageTreeViewItem);
                }
            }
        }


        public ImageTreeViewItem SearchFullName(string fullName, TreeView DirectoryView) 
        {
            if (DirectoryView.Items.Count == 0) return null;

            foreach(ImageTreeViewItem item in DirectoryView.Items)
            {
                if(directoryManager.IsFileNamesEqual(item.FullName,fullName)) return item;

                ImageTreeViewItem findItem = SubSearchFullName(fullName, item);
                if(findItem != null) return findItem;   
            }

            return null;
        }

        public ImageTreeViewItem SubSearchFullName(string fullName, ImageTreeViewItem item)
        {
            if (item.Items.Count == 0) return null;


            foreach(ImageTreeViewItem subItme in item.Items)
            {
                if(directoryManager.IsFileNamesEqual(subItme.FullName, fullName))
                {
                    return  subItme;
                }
                

                if(subItme.Items.Count  != 0)
                {
                    ImageTreeViewItem imageTreeViewItem = SubSearchFullName(fullName, subItme);

                    if(imageTreeViewItem != null)
                    {
                        return imageTreeViewItem;
                    }
                }

            }


            return null;
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
                ImageTreeViewItem treeViewItem = null;

                //디렉토리가 아닌 파일이 선택될 경우 미리보기 기능이 실행되는 것을 방지 
                if (e.OriginalSource is TextBlock)
                {
                    treeViewItem = FindParent<ImageTreeViewItem>(e.OriginalSource as DependencyObject);

                    if(treeViewItem != null)
                    {
                        if(directoryManager.IsImageExtension(treeViewItem.Tag.ToString()))
                        {
                            return;
                        }
                        
                    }

                }

                //아이템 초기화
                PictureViewer.Items.Clear();
                ImageTreeViewItem clickItem = sender as ImageTreeViewItem;
                if (clickItem == null) return;

                //탐색기 
                if(clickItem.Items.Count == 0)
                {
                    // 디렉토리 정보 가져오기
                    DirectoryInfo directory = new DirectoryInfo(clickItem.Tag as string);
                    FileInfo[] fileInfos = directory.GetFiles();
                    if (fileInfos.Length == 0)
                        return;

                    // 이미지의 그리드 크기를 계산하고 설정
                    imageManager.CalculateAndSetGridSizeForImages(PictureViewer, fileInfos);
                    // 파일로부터 이미지 미리보기를 로드하고 표시
                   

                    DisplayImagePreviewsFromFiles(fileInfos);

                    
                }

                //크로키목록 
                if(clickItem.Items.Count != 0)
                {
                    imageManager.CalculateAndSetGridSizeForImages(PictureViewer,clickItem);

                    DisplayImagePreviewsFromTreeViewItem(clickItem);
                }

                e.Handled = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }



        #endregion


        #region 크로키 목록 미리보기 구현 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeViewItem"></param>
        private async void DisplayImagePreviewsFromTreeViewItem(ImageTreeViewItem treeViewItem)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (ImageTreeViewItem item in treeViewItem.Items)
                    {
                        if (!imageStream.IsImageExtension(item.Tag.ToString()))
                        {
                            continue;
                        }
                        // UI 스레드로 디스패치하여 UI 요소 생성 및 초기화
                        //UI 요소의 생성: 'UI 요소'는 일반적으로 UI 스레드에서만 생성할 수 있습니다.UI 스레드 이외의 스레드에서 UI 요소를 생성하려고 시도하면 'System.InvalidOperationException' 예외가 발생합니다.
                        ImageBlock imageBlock = new ImageBlock(item._ImageName);
                        imageBlock._Image.Tag = item.FullName;
                        imageBlock._Image.MouseLeftButtonDown += LoadAndDisplayOriginalImageOnClick;

                        PictureViewer.Items.Add(imageBlock);
                    }

                }, DispatcherPriority.Normal);



            });

            await Task.Run(() =>
            {
                // 파일 이름 먼저출력 
                foreach (ImageTreeViewItem item in treeViewItem.Items)
                {
                    imageManager.DisplayImageOnUIThreadTree(PictureViewer, item);
                }

            });
        }



        #endregion


        #region
        public void DeleteImageItem(object sender,KeyEventArgs e)
        {

        }

        #endregion

        /// <summary>
        /// 지정된 파일 정보 배열에서 이미지 파일을 필터링하고, 해당 이미지를 UI에 미리보기로 표시합니다.
        /// </summary>
        /// <param name="fileInfos">미리보기를 원하는 파일의 정보 배열</param>
        private async void DisplayImagePreviewsFromFiles(FileInfo[] fileInfos)
        {
            await Task.Run(() =>
            {
                // 파일 이름 먼저출력 
                foreach (FileInfo file in fileInfos.OrderBy(f => f.Name))
                {
                    if (!imageStream.IsImageExtension(file.FullName))
                    {
                        continue;
                    }
                    // UI 스레드로 디스패치하여 UI 요소 생성 및 초기화
                    Dispatcher.Invoke(() =>
                    {
                        //UI 요소의 생성: 'UI 요소'는 일반적으로 UI 스레드에서만 생성할 수 있습니다.UI 스레드 이외의 스레드에서 UI 요소를 생성하려고 시도하면 'System.InvalidOperationException' 예외가 발생합니다.
                        ImageBlock imageBlock = new ImageBlock(file.Name);
                        imageBlock._Image.Tag = file.FullName;
                        imageBlock._Image.MouseLeftButtonDown += LoadAndDisplayOriginalImageOnClick;

                        PictureViewer.Items.Add(imageBlock);
                    }, DispatcherPriority.Normal);


                }
            });

            await Task.Run(() =>
            {
                // 파일 이름 먼저출력 
                foreach (FileInfo file in fileInfos)
                {
                    imageManager.DisplayImageOnUIThread(PictureViewer,file.FullName);
                }
            });


        }

       




        private void PreViewImageChange(ListBox pictureBox,TreeViewItem treeViewItem)
        {
            if(treeViewItem == null) { return; }
            string tag= treeViewItem.Tag.ToString();
            if(tag == null ||  tag.Length == 0 || tag.Equals("")) { return;}

            ImageBlock imageBlock= FindImageBlockByTag(pictureBox, tag);

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






        #region 원본 이미지를 보는 이벤트 

        /// <summary>
        /// 이미지 미리보기에서 이미지를 클릭하면 원본 이미지를 확인할 수 있는 이벤트 핸들러입니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 UI 요소</param>
        /// <param name="e">마우스 이벤트 인자</param>
        private void LoadAndDisplayOriginalImageOnClick(object sender, MouseEventArgs e)
        {
          
            if (sender is System.Windows.Controls.Image)
            {
                Image send = sender as Image;
                string path = send.Tag.ToString();

                BitmapImage bitmapImage = imageManager.CreateBitmapImageFromPath(path);
                if (path == null) return;
                //메인 화면에 이미지 출력 
                mainContent.Source = bitmapImage;


            }


            if(sender is ImageTreeViewItem)
            {
                ImageTreeViewItem imageTreeViewItem = sender as ImageTreeViewItem;

                imageManager.ChangeMainWin(mainContent,imageTreeViewItem);
            }

        }

        

        #endregion



        #region 윈도우 Control 사이즈 변경 
        System.Windows.Point mouseDown;
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
            System.Windows.Point mouseup = e.GetPosition(this);
            double temp_x = (mouseup.X - mouseDown.X);

            // View 요소의 너비 조절
            FileViewGrid.Width = FileViewGrid.Width + temp_x;

            PreviewBorder.Width = FileViewGrid.Width;
            ContentBorder.Width = ContentBorder.Width - temp_x;

            //컨트롤 사이즈 설정 
            fileViewGridSize.DefaultWidth = FileViewGrid.Width;
            contentBorderSize.DefaultWidth = ContentBorder.Width;
            previewBorderSize.DefaultWidth = PreviewBorder.Width;

            // View와 ContentView의 분할선 위치 및 너비 조절
            double contentSplitterLocation = FileViewGrid.Width + FileViewGrid.Margin.Left + FileViewGrid.Margin.Right;
            ViewContentSplitter.Margin = new Thickness(contentSplitterLocation, 0, 0, 0);
        }

        

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
                FullDisplays();

                OnlyContentViewVisibility();
            }
            catch (NullReferenceException ex)
            {
                log.LogWrite(ex);
            } 
            catch (ArgumentException  ex) 
            { 
                log.LogWrite(ex);
            }

        }

        public void WindowStateChangedEvent(object sender, EventArgs e)
        {
            if (MainWin.WindowState == WindowState.Maximized)
            {

                double croquisBorderWidth = CroquisBorder.Visibility == Visibility.Visible ? CroquisBorder.Width : 0;
                double croquisSplitterWidth = CroquisSplitter.Visibility == Visibility.Visible ? CroquisSplitter.Width : 0;
                double fileViewGridWidth = FileViewGrid.Visibility == Visibility.Visible ? FileViewGrid.Width : 0;
                double ViewContentSplitterWidth = ViewContentSplitter.Visibility == Visibility.Visible ? ViewContentSplitter.Width : 0;
                double optionBorderWidth = OptionBorder.Visibility == Visibility.Visible ? OptionBorder.Width : 0;

                double contentBorderWidth = ContentBorder.Visibility == Visibility.Visible ? (ContentBorder.Width + 4) : 0;

                double mainWidth = GetCurrentMonitorWidth();
                double mainHeight = GetCurrentMonitorHeight();
                double EmptyWidth = mainWidth - (contentBorderWidth + croquisBorderWidth + croquisSplitterWidth + fileViewGridWidth + ViewContentSplitterWidth + optionBorderWidth);


                //여기서 6은 MainGrid(4,Left = 2, right = 2) + OptionBorder.Margin(2,Left = 2) 
                ContentBorder.Width = (ContentBorder.Width + EmptyWidth) - 6;
                ContentBorder.Height = mainHeight - 4;
            }
            else
            {
                ContentBorder.Width = contentBorderSize.DefaultWidth;
                ContentBorder.Height = contentBorderSize.DefaultHeight;
            }
        }

        private double GetCurrentMonitorWidth()
        {
            // 현재 앱의 중심 위치를 가져옵니다.
            var windowCenter = new System.Drawing.Point(
                (int)(this.Left + this.Width / 2),
                (int)(this.Top + this.Height / 2));

            // 앱이 현재 위치한 모니터를 찾습니다.
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.Bounds.Contains(windowCenter))
                {
                    return screen.Bounds.Width;
                }
            }

            // 일치하는 모니터가 없을 경우 기본값 반환
            return SystemParameters.PrimaryScreenWidth;
        }

        private double GetCurrentMonitorHeight()
        {
            // 현재 앱의 중심 위치를 가져옵니다.
            var windowCenter = new System.Drawing.Point(
                (int)(this.Left + this.Width / 2),
                (int)(this.Top + this.Height / 2));

            // 앱이 현재 위치한 모니터를 찾습니다.
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.Bounds.Contains(windowCenter))
                {
                    return screen.WorkingArea.Height;
                }
            }

            // 일치하는 모니터가 없을 경우 기본값 반환
            return SystemParameters.PrimaryScreenHeight;
        }

        private ControlResize TempCroquisSize;
        private ControlResize TempViewSize;
        private ControlResize TempContentBorderSize;
        private ControlResize TempOptionSize;
        private ControlResize TempFileSize;
        private ControlResize TempPreViewSize;

        /// <summary>
        /// 
        /// </summary>
        private void FullDisplays()
        {
            double croquisBorderWidth = CroquisBorder.Visibility == Visibility.Visible ? CroquisBorder.Width : 0;
            double croquisSplitterWidth = CroquisSplitter.Visibility == Visibility.Visible ? CroquisSplitter.Width : 0;
            double fileViewGridWidth = FileViewGrid.Visibility == Visibility.Visible ? FileViewGrid.Width : 0;
            double ViewContentSplitterWidth = ViewContentSplitter.Visibility == Visibility.Visible ? ViewContentSplitter.Width : 0;
            double optionBorderWidth = OptionBorder.Visibility == Visibility.Visible ? OptionBorder.Width : 0;

            double contentBorderWidth = ContentBorder.Visibility == Visibility.Visible ? ContentBorder.Width  : 0;

            //현재 윈도우 창 크기 
            double TempContentBorderSize = MainGrid.ActualWidth;


            //기존 윈도우 창 크기 
            double DefultMainGridSize = ((contentBorderWidth + croquisBorderWidth + croquisSplitterWidth + fileViewGridWidth + ViewContentSplitterWidth + optionBorderWidth) + 2);

            double T = TempContentBorderSize - DefultMainGridSize;



            if (MainWin.WindowState == WindowState.Normal && T != 0)
            {
                //ContentBorder 너비 계산 
                double TempContentBorderWidth = T;
                ContentBorder.Width = contentBorderWidth + TempContentBorderWidth;
                ContentBorder.Height = MainGrid.ActualHeight;
            }
           
        }



        private void OnlyContentViewVisibility()
        {
            //ContentView만 남았을 때
            if(CroquisBorder.Visibility == Visibility.Collapsed 
                && FileViewGrid.Visibility == Visibility.Collapsed
                && OptionBorder.Visibility == Visibility.Collapsed)
            {
                ContentBorder.Width = MainGrid.ActualWidth;
                ContentBorder.Height = MainGrid.ActualHeight;
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
        public void CroquisStartButton(object sender, MouseButtonEventArgs e)
        {
            FullDisplay();

            //파일이 비어있으면 중지한다. 
            if (IsTreeViewEmpty(CroquisTreeView))
            {
                WarningBox("이미지 파일이 없습니다.");
                return;
            }

            int interval = GetTime(IntervalHour.Text,IntervalMinute.Text,IntervalSecond.Text);
            int refreshInterval = GetTime(RefreshHour.Text, RefreshMinute.Text, RefreshSecond.Text);
            int page = GetPage(page_input.Text);

            if (interval == 0 || refreshInterval == 0)
            {
                return;
            }

            croquisPlay.Interval = interval;
            croquisPlay.RefreshInterval = refreshInterval;
            croquisPlay.Page = page;

            List<ImageTreeViewItem> croquisList = GetCroquisFile(CroquisTreeView);

            StatusButton(sender, croquisList);
            


        }

        public void SetFocusableFalse()
        {
            PictureViewer.Focusable = false;
            BookMark.Focusable = false;
            DirectoryView.Focusable = false;

            CroquisTreeView.Focusable = false;

            page_input.Focusable = false;
            IntervalHour.Focusable = false;
            IntervalMinute.Focusable = false;
            IntervalSecond.Focusable = false;
            RefreshHour.Focusable = false;
            RefreshMinute.Focusable = false;
            RefreshSecond.Focusable = false;
            fullDisplay.Focusable = false;
            TopMost.Focusable = false;
        }

        public void SetFocusableTrue()
        {
            PictureViewer.Focusable = true;

            BookMark.Focusable = true;
            DirectoryView.Focusable = true;
            
            CroquisTreeView.Focusable = true;

            page_input.Focusable = true;
            IntervalHour.Focusable = true;
            IntervalMinute.Focusable = true;
            IntervalSecond.Focusable = true;
            RefreshHour.Focusable = true;
            RefreshMinute.Focusable = true;
            RefreshSecond.Focusable = true;
            fullDisplay.Focusable = true;
            TopMost.Focusable = true;
        }

        public async void StatusButton(object sender, List<ImageTreeViewItem> croquisList)
        {
            Button button = sender as Button;
            if (button == null) { return; }
            button.Focusable = false;


            if (croquisPlay.Status == PlayStatus.Stop)
            {
                
                SetFocusableFalse();
                
                button.Content = "중지";
                button.Background = new BrushConverter().ConvertFrom("#FF884444") as Brush;

                bool result = await croquisPlay.Run(croquisList);

                if(result)
                {
                    SetFocusableTrue();
                    croquisPlay.stop();
                    button.Content = "시작";
                    button.Background = new BrushConverter().ConvertFrom("#FF737373") as Brush;
                    AlarmBox("크로키 종료");
                }


                return;
            }

            if (croquisPlay.Status == PlayStatus.Play)
            {
                SetFocusableTrue();
                croquisPlay.stop();
                button.Content = "시작";
                button.Background = new BrushConverter().ConvertFrom("#FF737373") as Brush;
                return;
            }

        }

        public int GetPage(string page)
        {
            try
            {
                int result = int.Parse(page);


                return result;
            }
            catch (Exception e)
            {
                WarningBox("문자는 입력할 수 없습니다.");
            }
            return 0;
        }

        public int GetTime(string Hour,string Minute,string Second)
        {
            try
            {
                int hour = int.Parse(Hour);
                int minute = int.Parse(Minute);
                int second = int.Parse(Second);

                return (hour * 60 * 60) + (minute * 60) + second;
            } catch (Exception e) 
            {
                WarningBox("문자는 입력할 수 없습니다.");
            }
            return 0;
        }

        
        /// <summary>
        /// 주어진 TreeView가 비어 있는지 확인한다.
        /// </summary>
        /// <param name="treeView">확인할 TreeView.</param>
        /// <returns>TreeView가 비어 있으면 true, 그렇지 않으면 false를 반환한다.</returns>
        public bool IsTreeViewEmpty(TreeView treeView)
        {
            return treeView.Items.Count == 0;
        }




        /// <summary>
        /// 테스트 중 원본 이미지 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private void showImage(ImageTreeViewItem item)
        {
            if(item != null)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    imageManager.ChangeMainWin(mainContent, item);
                }));
            } else
            {
                mainContent.Source = null;
            }
            
        }


        

        
        public List<ImageTreeViewItem> GetCroquisFile(TreeView treeView)
        {

            List<ImageTreeViewItem> CroquisList = new List<ImageTreeViewItem>();


            foreach (ImageTreeViewItem item in treeView.Items) 
            {
                if(directoryManager.IsImageExtension(item._FullName))
                {
                    CroquisList.Add(item);
                }
                
                if (item.Items.Count != 0)
                {
                    SubFile(item, CroquisList);
                }

            }

            return CroquisList;
        }

        public void SubFile(ImageTreeViewItem item,List<ImageTreeViewItem> CroquisList)
        {
            foreach(ImageTreeViewItem subItem in item.Items)
            {
                if (directoryManager.IsImageExtension(subItem._FullName))
                {
                    CroquisList.Add(subItem);
                }
               
                if (subItem.Items.Count != 0)
                {
                    SubFile(subItem, CroquisList);
                }
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

        private void AlarmBox(string message) 
        {
            MessageBox.Show(message, "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion


    }

}
