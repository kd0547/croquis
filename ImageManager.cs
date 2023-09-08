using croquis;
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

    private ExceptionLogger log;

    public MouseButtonEventHandler ImageClick { get; set; }


    public ImageManager(ExceptionLogger log)
	{
        this.log = log;
	}


    //https://dev-dudfufl.tistory.com/9




    /// <summary>
    /// 지정된 이미지 파일을 로드하고, 원본 크기의 비트맵 이미지를 반환합니다.
    /// </summary>
    /// <param name="Path">이미지 파일의 경로</param>
    /// <returns>원본 크기의 비트맵 이미지</returns>
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

    

    //https://stackoverflow.com/questions/7533845/system-argumentexception-parameter-is-not-valid 
    // 해결할 수 있는 코드 같은데 아직 모르겠음
    // 이미지 손상이라 해결할 필요 없음 ㅅㄱ

    /// <summary>
    /// 이미지 파일의 회전 정보를 확인하여 이미지가 회전되었는지 여부를 반환합니다.
    /// </summary>
    /// <param name="memoryStream">이미지 파일의 데이터가 포함된 MemoryStream</param>
    /// <returns>이미지가 시계방향으로 회전된 경우 true, 그렇지 않으면 false</returns>
    public bool ImageFileRotateCheck(MemoryStream memoryStream)
    {

        try
        {
            // MemoryStream에서 이미지를 읽어옵니다.
            using (System.Drawing.Image img = System.Drawing.Image.FromStream(memoryStream))
            {
                // 이미지의 속성(Property) 목록에 회전 정보를 나타내는 속성 (0x112)이 있는지 확인합니다.
                if (img.PropertyIdList.Contains(0x112))
                {
                    foreach (var prop in img.PropertyItems)
                    {
                        if (prop.Id == 0x112)
                        {
                            // 회전 정보 속성 (0x112)을 찾고, 속성 값이 6인 경우 이미지가 시계방향으로 회전되었다고 판단합니다.
                            if (prop.Value[0] == 6)
                            {
                                return true;
                            }
                        }
                    }
                }

                // 회전 정보가 없거나 회전되지 않은 이미지인 경우 false를 반환합니다.
                return false;
            }
        }
        catch (Exception ex)
        {
            // 에러가 발생한 경우 디버그 출력으로 에러 메시지와 관련 정보를 표시합니다.

        }

        // 에러가 발생한 경우 또는 회전 정보를 확인할 수 없는 경우 false를 반환합니다.
        return false;
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
                        stream.Seek(0, SeekOrigin.Begin);
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
        pictureBox.DataContext = new PreviewGridSize(rows, columns);

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
    /// 지정된 이미지 파일의 경로로부터 이미지를 읽기 모드로 로드하여 MemoryStream으로 반환합니다.
    /// </summary>
    /// <param name="ImagePath">이미지 파일 경로</param>
    /// <returns>로드된 이미지의 MemoryStream</returns>
    public MemoryStream LoadImageStream(string ImagePath)
    {
        using (FileStream stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                return memoryStream;
            }
        }
    }


    /// <summary>
    /// 지정된 이미지 파일의 경로로부터 이미지를 비동기적으로 읽어와 MemoryStream 형태로 반환합니다.
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
    /// 제공된 MemoryStream에서 이미지를 가져와 지정된 최대 너비 및 높이로 조정하고, 
    /// 리사이즈된 이미지의 MemoryStream을 반환합니다. 이미지의 원래 종횡비는 유지됩니다.
    /// </summary>
    /// <param name="sourceStream">조정할 이미지가 들어있는 MemoryStream입니다.</param>
    /// <param name="maxWidth">리사이즈된 이미지의 최대 너비입니다.</param>
    /// <param name="maxHeight">리사이즈된 이미지의 최대 높이입니다.</param>
    /// <returns>리사이즈된 이미지의 MemoryStream을 반환합니다.</returns>
    public MemoryStream ResizeImage(MemoryStream sourceStream, int maxWidth, int maxHeight)
    {
        try
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
        catch (Exception ex)
        {
            this.log.LogWrite(ex.StackTrace);
        }

        return null;
    }



    /// <summary>
    /// 지정된 이미지의 MemoryStream을 받아와 비동기적으로 최대 너비 및 높이로 리사이즈한 후,
    /// 리사이즈된 이미지의 MemoryStream을 반환합니다.
    /// </summary>
    /// <param name="sourceStream">리사이즈할 이미지의 MemoryStream</param>
    /// <param name="maxWidth">최대 너비</param>
    /// <param name="maxHeight">최대 높이</param>
    /// <returns>리사이즈된 이미지의 MemoryStream</returns>
    public async Task<MemoryStream> ResizeImageAsync(MemoryStream sourceStream, int maxWidth, int maxHeight)
    {
        return await Task<MemoryStream>.Run(() =>
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
        });
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
