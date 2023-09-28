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

/// <summary>
/// 로컬 디스크에서 지정 경로의 이미지를 가져오는 클래스입니다. 
/// 형태는 Stream으로 가져옵니다.
/// </summary>
public class ImageStream
{

    public int ImageSize { get; set; } = 129;
    public int PreviewGridWidth { get; set; }

    private ExceptionLogger log;

    public MouseButtonEventHandler ImageClick { get; set; }


    public ImageStream(ExceptionLogger log)
	{
        this.log = log;
	}


    //https://dev-dudfufl.tistory.com/9

    #region 미리보기 이미지 스트림

    /// <summary>
    /// 주어진 이미지 파일 경로를 이용해 이미지를 읽고, MemoryStream으로 반환합니다.
    /// </summary>
    /// <param name="ImagePath">로드할 이미지의 파일 경로</param>
    /// <returns>이미지의 내용을 담은 memoryStream</returns>
    /// <exception cref="System.ArgumentException">유효하지 않은 이미지 경로</exception>
    /// <exception cref="System.IO.FileNotFoundException">지정된 경로의 파일을 찾을 수 없음</exception>
    /// <exception cref="System.IO.IOException">파일 읽기 중 오류 발생</exception>
    /// <exception cref="System.UnauthorizedAccessException">파일 읽기 권한이 없음</exception>
    public MemoryStream LoadImageToMemoryStream(string ImagePath)
    {
        try
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
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"지정된 경로의 파일을 찾을 수 없습니다: {ImagePath}", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"파일 '{ImagePath}' 읽기 중 오류가 발생했습니다.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"파일 '{ImagePath}' 읽기 권한이 없습니다.", ex);
        }
    }


    /// <summary>
    /// 지정된 이미지 파일의 경로로부터 이미지를 비동기적으로 읽어와 memoryStream 형태로 반환합니다.
    /// </summary>
    /// <param name="ImagePath">이미지 파일 경로</param>
    /// <returns>이미지 파일의 memoryStream</returns>
    /// <exception cref="System.ArgumentException">유효하지 않은 이미지 경로</exception>
    /// <exception cref="System.IO.FileNotFoundException">지정된 경로의 파일을 찾을 수 없음</exception>
    /// <exception cref="System.IO.IOException">파일 읽기 중 오류 발생</exception>
    /// <exception cref="System.UnauthorizedAccessException">파일 읽기 권한이 없음</exception>
    public async Task<MemoryStream> LoadImageToMemoryStreamAsync(string ImagePath)
    {
        return await Task.Run(() => {
            if (string.IsNullOrWhiteSpace(ImagePath))
                throw new ArgumentException("ImagePath cannot be null or empty.", nameof(ImagePath));

            try
            {
                using (FileStream stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        return new MemoryStream(reader.ReadBytes((int)stream.Length));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"{ImagePath} 경로의 파일을 찾을 수 없습니다.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"{ImagePath} 경로의 파일 읽기 접근이 거부되었습니다.");
            }
            catch (IOException e)
            {
                throw new IOException("파일을 읽는 중 오류가 발생하였습니다.", e);
            }
        });
    }



    #endregion

    #region 이미지 리사이징

    /// <summary>
    /// 제공된 MemoryStream에서 이미지를 로드하여 지정된 최대 너비 및 높이로 리사이징하며, 
    /// 변경된 이미지를 memoryStream 형태로 반환합니다. 원래의 종횡비는 유지됩니다.
    /// </summary>
    /// <param name="sourceStream">원본 이미지가 포함된 MemoryStream입니다.</param>
    /// <param name="maxWidth">변경된 이미지의 최대 너비입니다.</param>
    /// <param name="maxHeight">변경된 이미지의 최대 높이입니다.</param>
    /// <returns>변경된 이미지를 포함하는 MemoryStream을 반환합니다.</returns>
    public MemoryStream ResizeImage(MemoryStream sourceStream, int maxWidth, int maxHeight)
    {
        if (sourceStream == null || sourceStream.Length == 0)
        {
            log.LogWrite("제공된 MemoryStream이 비어 있습니다.");
            return null;
        }

        try
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(sourceStream))
            {
                // 이미지 크기를 계산
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
                    // 변경된 이미지를 MemoryStream에 저장
                    MemoryStream resizedStream = new MemoryStream();
                    resizedImage.Save(resizedStream, System.Drawing.Imaging.ImageFormat.Jpeg); // 필요한 이미지 형식으로 변경 가능
                    resizedStream.Seek(0, SeekOrigin.Begin);

                    return resizedStream;
                }
            }
        }
        catch (ArgumentException ae)
        {
            log.LogWrite("올바르지 않은 이미지 형식: " + ae.Message);
        }
        catch (Exception ex)
        {
            log.LogWrite("이미지 리사이징 중 오류 발생: " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 지정된 이미지의 MemoryStream을 받아와 최대 너비 및 높이에 맞게 비동기적으로 리사이즈하고,
    /// 리사이즈된 이미지의 MemoryStream을 반환합니다. 이미지의 원래 종횡비는 유지됩니다.
    /// </summary>
    /// <param name="sourceStream">조정할 이미지가 들어있는 MemoryStream입니다.</param>
    /// <param name="maxWidth">리사이즈된 이미지의 최대 너비입니다.</param>
    /// <param name="maxHeight">리사이즈된 이미지의 최대 높이입니다.</param>
    /// <returns>비동기적으로 리사이즈된 이미지의 MemoryStream을 반환합니다.</returns>
    /// <exception cref="OutOfMemoryException">메모리 부족 시 발생할 수 있습니다.</exception>
    /// <exception cref="ArgumentException">잘못된 이미지 포맷이 전달될 경우 발생합니다.</exception>
    public async Task<MemoryStream> ResizeImageStreamAsync(MemoryStream sourceStream, int maxWidth, int maxHeight)
    {
        return await Task.Run(() =>
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
            catch (OutOfMemoryException)
            {
                throw new OutOfMemoryException("메모리 부족으로 이미지를 리사이즈할 수 없습니다.");
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("지원되지 않는 이미지 포맷입니다.");
            }
        });
    }



    #endregion




    #region 원본 이미지 스트림 
    /// <summary>
    /// 지정된 이미지 파일을 로드하고, 원본 크기의 비트맵 이미지를 반환합니다.
    /// </summary>
    /// <param name="Path">이미지 파일의 경로</param>
    /// <returns>원본 크기의 비트맵 이미지</returns>
    public MemoryStream LoadOriginalImage(string Path)
    {
        if (System.IO.File.Exists(Path))
        {

            //이미지를 읽기 전용으로 오픈 
            using (System.IO.FileStream stream = new System.IO.FileStream(Path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                {
                    var memoryStream = new System.IO.MemoryStream(reader.ReadBytes((int)stream.Length));
                    return memoryStream;
                }
            }
        }
        else
        {
            return null;
        }
    }



    #endregion





    //https://stackoverflow.com/questions/7533845/system-argumentexception-parameter-is-not-valid 
    // 해결할 수 있는 코드 같은데 아직 모르겠음
    // 이미지 손상이라 해결할 필요 없음 ㅅㄱ

    /// <summary>
    /// 이미지 파일의 회전 정보를 확인하여 이미지가 회전되었는지 여부를 반환합니다.
    /// </summary>
    /// <param name="memoryStream">이미지 파일의 데이터가 포함된 memoryStream</param>
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
