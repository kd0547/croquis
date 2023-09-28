using croquis;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// <summary>
/// 이 클래스는 프로그램 실행시 한 번만 생성되는 클래스입니다. 
/// 
/// </summary>

public class ResourceImage
{

    private static readonly Lazy<ResourceImage> instance = new Lazy<ResourceImage>(() => new ResourceImage());

    public static ResourceImage Instance => instance.Value;

    // 북마크 이미지 
    public ImageSource FullStarImage { get; } = null;
    public ImageSource EmptyStarImage { get; } = null;


    //로컬디스크 이미지 
    public ImageSource DriveImage { get; } = null;

    //디렉토리 이미지 
    public ImageSource DirectoryImage { get; } = null;



	private ResourceImage()
	{
        FullStarImage = CreateStarImage(Resource1.FullStar);
        EmptyStarImage = CreateStarImage(Resource1.EmptyStar);
    }


    private ImageSource CreateStarImage(System.Drawing.Bitmap bitmap)
    {
        try
        {
            System.Drawing.Bitmap resize = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(14, 14));

            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                resize.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        } 
        catch (Exception e )
        {
            // 예외를 던집니다.
            throw new Exception("Failed to create star image", e);
        }
        
    }


}

        


