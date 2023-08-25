using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

public class ImageManager
{

    public int ImageSize { get; set; } = 129;
    public int PreviewGridWidth { get; set; }


    public MouseButtonEventHandler ImageClick { get; set; }


    public ImageManager()
	{
	}

    
    //https://dev-dudfufl.tistory.com/9

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public BitmapImage LoadOriginalImage(string Path)
    {
        if (System.IO.File.Exists(Path))
        {
            //이미지를 읽기 전용으로 오픈 
            using (System.IO.FileStream stream = new System.IO.FileStream(Path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                {
                    using(var MemoryStream = new System.IO.MemoryStream(reader.ReadBytes((int)stream.Length)))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();

                        if (ImageFileRotateCheck(MemoryStream))
                        {
                            bitmapImage.Rotation = Rotation.Rotate90;
                        }
                        MemoryStream.Seek(0, SeekOrigin.Begin);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = MemoryStream;
                        bitmapImage.EndInit();


                        return bitmapImage;
                        
                    }
                }
            }
        }
        else
        {
            return null;
        }
    }

    

    private bool ImageFileRotateCheck(MemoryStream memoryStream)
    {
        using (System.Drawing.Image img = System.Drawing.Image.FromStream(memoryStream))
        {
            if (img.PropertyIdList.Contains(0x112))
            {
                foreach (var prop in img.PropertyItems)
                {
                    if (prop.Id == 0x112)
                    {
                        if (prop.Value[0] == 6)
                        {
                            Debug.WriteLine(prop.Value[0]);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }



    /// <summary>
    /// 지정된 디렉토리에서 이미지 파일을 로드하여 ListBox에 미리보기 이미지를 표시합니다.
    /// </summary>
    /// <param name="pictureBox">미리보기 이미지를 표시할 ListBox</param>
    /// <param name="path">이미지 파일이 포함된 디렉토리 경로</param>
    public void LoadAndDisplayPreviewImages(ListBox pictureBox,string path)
    {
        pictureBox.Items.Clear();

        DirectoryInfo directory = new DirectoryInfo(path);
        int imageCount = directory.GetFiles().Count();

        // ListBox 열 수 계산
        int columns = CalculateColumns((int)pictureBox.ActualWidth);
        int rows = (imageCount / columns) + 1;

        // ListBox에 DataContext 설정
        pictureBox.DataContext = new PictureBoxRowCols(rows, columns);

        foreach (FileInfo file in directory.GetFiles())
        {

            if (!IsImageExtension(file.FullName))
            {
                continue;
            }
            ImageSource imageSource = LoadImage(file.FullName);
            BitmapImage bitmapImage = (BitmapImage) imageSource;

            ImageBlock imageBlock = new ImageBlock(bitmapImage, file.Name);
            imageBlock._Image.Tag = file.FullName;
            imageBlock._Image.MouseLeftButtonDown += ImageClick;
            

            pictureBox.Items.Add(imageBlock);
        }
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ImagePath"></param>
    /// <returns></returns>
    public BitmapImage LoadBitMapImage(string ImagePath)
    {
        if (System.IO.File.Exists(ImagePath))
        {
            //이미지를 읽기 전용으로 오픈 
            using (System.IO.FileStream stream = new System.IO.FileStream(ImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                {

                    using (var MemoryStream = new System.IO.MemoryStream(reader.ReadBytes((int)stream.Length)))
                    {
                        return null;
                        //return ResizeImage(MemoryStream, 129, 129);
                    }

                }
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 파일 경로로부터 이미지를 로드하여 BitmapImage를 반환합니다.
    /// </summary>
    /// <param name="filePath">이미지 파일 경로</param>
    /// <returns>로드된 이미지의 BitmapImage</returns>
    public BitmapImage LoadImage(string ImagePath)
    {
        if (System.IO.File.Exists(ImagePath))
        {
            //이미지를 읽기 전용으로 오픈 
            using (System.IO.FileStream stream = new System.IO.FileStream(ImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                {

                    using (var MemoryStream = new System.IO.MemoryStream(reader.ReadBytes((int)stream.Length)))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        if (ImageFileRotateCheck(MemoryStream))
                        {
                            bitmapImage.Rotation = Rotation.Rotate90;
                        }
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        stream.Seek(0,SeekOrigin.Begin);
                        bitmapImage.StreamSource = stream;
                        bitmapImage.DecodePixelWidth = ImageSize;
                        bitmapImage.EndInit();

                        

                        return bitmapImage;
                    }

                }
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 지정된 이미지 파일을 비동기적으로 로드하고, 리사이징하여 BitmapImage로 반환합니다.
    /// </summary>
    /// <param name="fullName">이미지 파일의 전체 경로</param>
    /// <returns>리사이즈된 이미지의 BitmapImage</returns>
    public async Task<BitmapImage> LoadBitmapImageAsync(string fullName)
    {
        BitmapImage bitmapImage = new BitmapImage();

        

        using (var stream = await LoadImageStreamAsync(fullName))
        {
            MemoryStream resizeMemoryStream = ResizeImage(stream, 129, 129);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = resizeMemoryStream;

            if (ImageFileRotateCheck(stream))
            {
                bitmapImage.Rotation = Rotation.Rotate90;
            }

            bitmapImage.DecodePixelWidth = ImageSize;
            resizeMemoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.EndInit();
        }

        

        return bitmapImage;
    }

    /// <summary>
    /// 이미지 파일을 비동기적으로 읽어서 MemoryStream 형태로 반환합니다.
    /// </summary>
    /// <param name="ImagePath">이미지 파일 경로</param>
    /// <returns>이미지 파일의 MemoryStream</returns>
    public async Task<MemoryStream> LoadImageStreamAsync(string ImagePath)
    {
        return await Task<MemoryStream>.Run(() => {

            using (FileStream stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    var memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                    return memoryStream;
                }
            }
            

        });
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceStream"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    /// <returns></returns>
    public MemoryStream ResizeImage(MemoryStream sourceStream, int maxWidth, int maxHeight)
    {
        using (System.Drawing.Image image = System.Drawing.Image.FromStream(sourceStream))
        {
            // 이미지 크기를 조절
            int newWidth, newHeight;

            if (image.Width > image.Height)
            {
                newWidth = maxWidth;
                newHeight = (int)((double)image.Height / image.Width * maxWidth);
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)((double)image.Width / image.Height * maxHeight);
            }

            using (System.Drawing.Image resizedImage = new Bitmap(image, newWidth, newHeight))
            {
                // 리사이즈된 이미지를 MemoryStream에 저장
                MemoryStream resizedStream = new MemoryStream();
                resizedImage.Save(resizedStream, System.Drawing.Imaging.ImageFormat.Jpeg); // 이미지 형식에 맞게 변경
                resizedStream.Seek(0, SeekOrigin.Begin);

                return resizedStream;
            }
        }
    }


 


    private BitmapImage createImage(string ImagePath)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri(ImagePath);
        bitmapImage.DecodePixelWidth = ImageSize;
        bitmapImage.EndInit();

        return bitmapImage;
    }


    /// <summary>
    /// 주어진 파일 경로의 확장자가 이미지 파일인지 확인합니다.
    /// </summary>
    /// <param name="path">확인할 파일 경로</param>
    /// <returns>이미지 파일이면 true, 아니면 false</returns>
    public bool IsImageExtension(string path)
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
    /// ListBox의 열 수를 계산합니다.
    /// </summary>
    /// <param name="listBoxWidth">ListBox의 너비</param>
    /// <returns>계산된 열 수</returns>
    public int CalculateColumns(int width)
    {
        return (int)width / ImageSize;
    }

}
