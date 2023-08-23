using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
    /// 폴더에서 이미지를 가져오는 함수
    /// </summary>
    /// <param name="path"></param>
    public void showImageFile(ListBox pictureBox,string path)
    {
        pictureBox.Items.Clear();

        DirectoryInfo directory = new DirectoryInfo(path);
        int count = directory.GetFiles().Length;

        
        int col = calculateCol((int)pictureBox.ActualWidth);
        int row = (count / col) + 1;
        
        pictureBox.DataContext = new PictureBoxRowCols(row, col);

        foreach (FileInfo file in directory.GetFiles())
        {

            if (!IsImageExtension(file.FullName))
            {
                continue;
            }
            //Debug.WriteLine(file.FullName);
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();

            img.Width = ImageSize;
            img.Height = ImageSize;

            BitmapImage bitmapImage = TEST(file.FullName);

            img.Source = bitmapImage;
            img.Tag = file.FullName;

            img.MouseLeftButtonDown += ImageClick;

            

            pictureBox.Items.Add(img);
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
                        return ResizeImage(MemoryStream, 129, 129);
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
    /// 
    /// </summary>
    /// <param name="ImagePath"></param>
    /// <returns></returns>
    public BitmapImage TEST(string ImagePath)
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
    /// 
    /// </summary>
    /// <param name="sourceStream"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    /// <returns></returns>
    public BitmapImage ResizeImage(MemoryStream sourceStream, int maxWidth, int maxHeight)
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
                using (MemoryStream resizedStream = new MemoryStream())
                {
                    resizedImage.Save(resizedStream, System.Drawing.Imaging.ImageFormat.Jpeg); // 이미지 형식에 맞게 변경
                    resizedStream.Seek(0, SeekOrigin.Begin);

                    // BitmapImage로 변환
                    BitmapImage resizedBitmapImage = new BitmapImage();
                    resizedBitmapImage.BeginInit();
                    if (ImageFileRotateCheck(sourceStream))
                    {
                        resizedBitmapImage.Rotation = Rotation.Rotate90;
                    }
                    resizedStream.Seek(0, SeekOrigin.Begin);


                    resizedBitmapImage.StreamSource = resizedStream;
                    resizedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    resizedBitmapImage.EndInit();

                    return resizedBitmapImage;
                }
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
    /// PreView의 width크기를 가져와 
    /// </summary>
    /// <param name="width"></param>
    /// <returns></returns>
    private int calculateCol(int width)
    {
        return (int)width / ImageSize;
    }

}
