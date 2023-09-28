using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

public class ImageManager
{
    private DirectoryManager directoryManager;

    private ImageStream imageStream;
    
    private ImageCacheRepository cacheManager;

    public ImageManager(DirectoryManager directoryManager, ImageStream imageStream, ImageCacheRepository cacheManager)
    {
        this.directoryManager = directoryManager;
        this.imageStream = imageStream;
        this.cacheManager = cacheManager;
    }

    #region 미리보기 이미지 


    /// <summary>
    /// 
    /// </summary>
    /// <param name="PictureViewer"></param>
    /// <param name="imageTreeViewItem"></param>
    public void CalculateAndSetGridSizeForImages(ListBox PictureViewer,ImageTreeViewItem imageTreeViewItem)
    {
        int imageCount = imageTreeViewItem.Items.Count;

        // ListBox 열 수 계산
        int columns = imageStream.CalculateColumns((int)PictureViewer.ActualWidth);
        int rows = (imageCount / columns) + 1;

        // ListBox에 DataContext 설정
        PictureViewer.DataContext = new PreviewGridSize(rows, columns);
    }


    /// <summary>
    /// 주어진 이미지 파일들을 화면에 어떻게 배열할지 결정하는 그리드 크기를 계산하고 설정합니다.
    /// </summary>
    /// <param name="fileInfos">이미지 파일 정보 배열</param
    public void CalculateAndSetGridSizeForImages(ListBox PictureViewer, FileInfo[] fileInfos)
    {
        int imageCount = fileInfos.Length;

        // ListBox 열 수 계산
        int columns = imageStream.CalculateColumns((int)PictureViewer.ActualWidth);
        int rows = (imageCount / columns) + 1;

        // ListBox에 DataContext 설정
        PictureViewer.DataContext = new PreviewGridSize(rows, columns);
    }


    /// <summary>
    ///  지정된 이미지 파일을 UI 스레드에서 로드하여 미리보기 컨트롤에 표시합니다.
    /// </summary>
    /// <param name="fullName"></param>
    public async void DisplayImageOnUIThread(System.Windows.Controls.ListBox pictureView, string fullName)
    {
        // 이미지 파일을 읽어와 MemoryStream에 저장합니다.
        using (MemoryStream memoryStream = imageStream.LoadImageToMemoryStream(fullName))
        {
            // 이미지 회전 여부를 확인합니다.
            bool rotateCheck = imageStream.ImageFileRotateCheck(memoryStream);

            // 이미지를 지정된 크기로 리사이징한 후 MemoryStream에 저장합니다.
            using (MemoryStream ResizeMemoryStream = imageStream.ResizeImage(memoryStream, 129, 129))
            {
                // 이미지 리사이징에 실패한 경우 처리를 중단합니다.
                if (ResizeMemoryStream == null)
                {
                    return;
                }
                // MemoryStream의 위치를 처음으로 되돌립니다.
                ResizeMemoryStream.Seek(0, SeekOrigin.Begin);

                // UI 스레드로 비동기적으로 이미지를 표시합니다.
                await Application.Current.Dispatcher.InvokeAsync(async () =>
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
                    ImageBlock FindBlock = FindImageBlockByTag(pictureView, fullName);
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



    private async void DisplayImageAsync(MemoryStream ResizeMemoryStream, System.Windows.Controls.ListBox pictureView, string fullName)
    {
        // UI 스레드로 비동기적으로 이미지를 표시합니다.
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();

            // 이미지를 캐시로 로드하고 스트림을 지정합니다.
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = ResizeMemoryStream;
            bitmapImage.EndInit();

            // 이미지를 찾아서 미리보기 컨트롤에 표시합니다.
            ImageBlock FindBlock = FindImageBlockByTag(pictureView, fullName);
            {
                if (FindBlock != null)
                {
                    FindBlock._Image.Source = bitmapImage;
                }
            }


        }, DispatcherPriority.Normal);
    }


    #endregion

    #region 원본 이미지로 BitmapImage 생성

    /// <summary>
    /// 
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public BitmapImage CreateBitmapImageFromPath(string path)
    {
        MemoryStream memoryStream = null;
        try
        {
            memoryStream = imageStream.LoadOriginalImage(path);
            if (memoryStream == null || memoryStream.Length == 0) return null;
            
            BitmapImage bitmapImage = CreateBitmapImageFromStream(memoryStream);
            
            return bitmapImage;
        } catch (Exception e)
        {
            
        } finally
        {
            memoryStream?.Close();
        }

        return null;
    }


    #endregion



    #region MemoryStream으로 BitmapImage 생성

    /// <summary>
    /// 주어진 MemoryStream에서 BitmapImage를 생성하여 반환합니다.
    /// </summary>
    /// <param name="memoryStream">이미지 데이터를 포함하는 MemoryStream</param>
    /// <returns>생성된 BitmapImage 객체</returns>
    public BitmapImage CreateBitmapImageFromStream(MemoryStream memoryStream)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        memoryStream.Seek(0, SeekOrigin.Begin);
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();

        return bitmapImage;
    }


    #endregion




    #region
    /// <summary>
    /// 이미지를 화면에 출력
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    public void ChangeMainWin(Image image, ImageTreeViewItem imageTreeViewItem)
    {
        MemoryStream memoryStream = null;

        if (imageTreeViewItem.ImageCache != null)
        {
            //이미지 좌우반전
            memoryStream = ChangeConvertToFlip(imageTreeViewItem);

            //이미지 흑백전환 
            if (imageTreeViewItem.ImageCache.IsGrayscale)
            {
                if (memoryStream != null)
                {
                    memoryStream = ConvertToGrayscale(memoryStream);
                }
                else
                {
                    memoryStream = ConvertToGrayscale(imageTreeViewItem.FullName);
                }


            }

            //이미지 회전 
            if (memoryStream != null)
            {
                MemoryStream result = RotateImage(memoryStream, imageTreeViewItem);
                memoryStream = result == null ? memoryStream : result;
            }
            else
            {
                MemoryStream result = RotateImage(imageTreeViewItem);
                memoryStream = result == null ? memoryStream : result;
            }



        }


        if (memoryStream == null || imageTreeViewItem.ImageCache == null)
            memoryStream = imageStream.LoadOriginalImage(imageTreeViewItem.FullName);

        BitmapImage bitmapImage = new BitmapImage();

        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();

        image.Source = bitmapImage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    /// <param name="sourceMemoryStream"></param>
    /// <returns></returns>
    public MemoryStream ChangeMainWin(ImageTreeViewItem imageTreeViewItem, MemoryStream sourceMemoryStream)
    {
        MemoryStream memoryStream = null;

        if (imageTreeViewItem.ImageCache != null)
        {
            //이미지 좌우반전
            memoryStream = ChangeConvertToFlip(imageTreeViewItem);

            //이미지 흑백전환 
            if (imageTreeViewItem.ImageCache.IsGrayscale)
            {
                if (memoryStream != null)
                {
                    memoryStream = ConvertToGrayscale(memoryStream);
                }
                else
                {
                    memoryStream = ConvertToGrayscale(imageTreeViewItem.FullName);
                }


            }

            //이미지 회전 
            if (memoryStream != null)
            {
                MemoryStream result = RotateImage(memoryStream, imageTreeViewItem);
                memoryStream = result == null ? memoryStream : result;
            }
            else
            {
                MemoryStream result = RotateImage(imageTreeViewItem);
                memoryStream = result == null ? memoryStream : result;
            }



        }


        if (memoryStream == null || imageTreeViewItem.ImageCache == null)
            memoryStream = imageStream.LoadOriginalImage(imageTreeViewItem.FullName);



        return memoryStream;
    }


    #endregion




    #region 이미지 회전, 흑백 전환, 좌우반전 기능 


    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MemoryStream RotateImage(ImageTreeViewItem imageTreeViewItem)
    {
        string path = imageTreeViewItem.ImageCache.ImagePath;
        MemoryStream memoryStream = imageStream.LoadOriginalImage(path);

        double angle = imageTreeViewItem.ImageCache.Angle;
        RotateFlags rotateFlag;
        if (angle == 0)
        {
            return null;
        }
        else if (angle == 90)
        {
            rotateFlag = RotateFlags.Rotate90Clockwise;
        }
        else if (angle == 180)
        {
            rotateFlag = RotateFlags.Rotate180;
        }
        else if (angle == 270)
        {
            rotateFlag = RotateFlags.Rotate90Counterclockwise;
        }
        else
        {
            throw new ArgumentException("Invalid rotation angle.");
        }
        byte[] buffer = memoryStream.ToArray();
        Mat mat = Mat.FromImageData(buffer);
        Mat rotatedImage = new Mat();
        Cv2.Rotate(mat, rotatedImage, rotateFlag);
        byte[] rotatedData = rotatedImage.ToBytes(".jpg");

        return new MemoryStream(rotatedData);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceMemoryStream"></param>
    /// <param name="imageTreeViewItem"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public MemoryStream RotateImage(MemoryStream sourceMemoryStream, ImageTreeViewItem imageTreeViewItem)
    {
        byte[] buffer = sourceMemoryStream.ToArray();
        double angle = imageTreeViewItem.ImageCache.Angle;
        RotateFlags rotateFlag;

        if (angle == 0)
        {
            return null;
        }
        else if (angle == 90)
        {
            rotateFlag = RotateFlags.Rotate90Clockwise;
        }
        else if (angle == 180)
        {
            rotateFlag = RotateFlags.Rotate180;
        }
        else if (angle == 270)
        {
            rotateFlag = RotateFlags.Rotate90Counterclockwise;
        }
        else
        {
            throw new ArgumentException("Invalid rotation angle.");
        }

        Mat mat = Mat.FromImageData(buffer);
        Mat rotatedImage = new Mat();
        Cv2.Rotate(mat, rotatedImage, rotateFlag);
        byte[] rotatedData = rotatedImage.ToBytes(".jpg");

        return new MemoryStream(rotatedData);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    /// <returns></returns>
    public MemoryStream ChangeConvertToFlip(ImageTreeViewItem imageTreeViewItem)
    {


        //좌우반전일 경우
        if (imageTreeViewItem.ImageCache.IsFlip == true)
        {
            return ConvertToFlip(imageTreeViewItem.FullName);
        }
        //좌우반전이 아닐 경우
        if (imageTreeViewItem.ImageCache.IsFlip == false)
        {
            return imageStream.LoadOriginalImage(imageTreeViewItem.FullName);
        }
        return null;
    }



    /// <summary>
    /// 이미지 좌우반전
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public MemoryStream ConvertToFlip(string path)
    {
        Mat src = Cv2.ImRead(path);
        Mat flip = new Mat();

        Cv2.Flip(src, flip, FlipMode.Y);
        byte[] data = flip.ToBytes(".jpg");

        MemoryStream memoryStream = new MemoryStream(data);
        return memoryStream;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    /// <returns></returns>
    public ImageCache CreateImageCacheFromTree(ImageTreeViewItem imageTreeViewItem)
    {
        StackPanel stack = imageTreeViewItem.Header as StackPanel;

        ImageCache imageCache = new ImageCache();
        imageCache.ImageName = imageTreeViewItem.ImageName;
        imageCache.ImagePath = imageTreeViewItem._FullName;

        return imageCache;
    }


    /// <summary>
    /// 이미지 좌우반전
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public void ConvertToFlip(ImageTreeViewItem imageTreeViewItem)
    {


        if (imageTreeViewItem.ImageCache == null)
        {
            imageTreeViewItem.ImageCache = CreateImageCacheFromTree(imageTreeViewItem);
        }


        if (imageTreeViewItem.ImageCache.IsFlip == true)
            imageTreeViewItem.ImageCache.IsFlip = false;


        if (imageTreeViewItem.ImageCache.IsFlip == false)
            imageTreeViewItem.ImageCache.IsFlip = true;


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageTreeViewItem"></param>
    public void ConvertToGrayscale(ImageTreeViewItem imageTreeViewItem)
    {
        if (imageTreeViewItem.ImageCache == null)
        {
            imageTreeViewItem.ImageCache = CreateImageCacheFromTree(imageTreeViewItem);
        }

        if (!imageTreeViewItem.ImageCache.IsGrayscale)
            imageTreeViewItem.ImageCache.IsGrayscale = true;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceMemoryStrem"></param>
    /// <returns></returns>
    public MemoryStream ConvertToGrayscale(MemoryStream sourceMemoryStrem)
    {
        byte[] buffer = sourceMemoryStrem.ToArray();

        Mat src = Mat.FromImageData(buffer);
        Mat Gray = new Mat();

        Cv2.CvtColor(src, Gray, ColorConversionCodes.BGR2GRAY);
        byte[] data = Gray.ToBytes(".jpg");

        MemoryStream memoryStream = new MemoryStream(data);


        return memoryStream;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public MemoryStream ConvertToGrayscale(string path)
    {
        Mat src = Cv2.ImRead(path);
        Mat Gray = new Mat();

        Cv2.CvtColor(src, Gray, ColorConversionCodes.BGR2GRAY);
        byte[] data = Gray.ToBytes(".jpg");

        MemoryStream memoryStream = new MemoryStream(data);


        return memoryStream;
    }
    #endregion



    #region 크로키 목록 


    /// <summary>
    /// 크로키 목록의 미리보기
    /// </summary>
    /// <param name="fullName"></param>
    public async void DisplayImageOnUIThreadTree(ListBox PictureViewer, ImageTreeViewItem treeViewItem)
    {
        using (MemoryStream memoryStream = ChangeMainWin(treeViewItem, null))
        {
            // 이미지를 지정된 크기로 리사이징한 후 MemoryStream에 저장합니다.
            using (MemoryStream ResizeMemoryStream = imageStream.ResizeImage(memoryStream, 129, 129))
            {
                // 이미지 리사이징에 실패한 경우 처리를 중단합니다.
                if (ResizeMemoryStream == null)
                {
                    return;
                }
                // MemoryStream의 위치를 처음으로 되돌립니다.
                ResizeMemoryStream.Seek(0, SeekOrigin.Begin);

                // UI 스레드로 비동기적으로 이미지를 표시합니다.
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    // 이미지를 캐시로 로드하고 스트림을 지정합니다.
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ResizeMemoryStream;
                    bitmapImage.EndInit();

                    // 이미지를 찾아서 미리보기 컨트롤에 표시합니다.
                    ImageBlock FindBlock = FindImageBlockByTag(PictureViewer, treeViewItem._FullName);
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

    #endregion










    #region 보조 메서드 


    /// <summary>
    /// PictureViewer 내에서 특정 태그를 가진 ImageBlock을 찾는 보조 메서드
    /// </summary>
    /// <param name="pictureBox"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private ImageBlock FindImageBlockByTag(System.Windows.Controls.ListBox pictureBox, string tag)
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
}
