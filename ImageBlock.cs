using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class ImageBlock : StackPanel
{
    public Image? _Image { get; set; }
    public ImageSource? _Source { get; set; }
    public TextBlock? _TextBlock { get; set; }
    
    public string? Text { get; set; }

     
    
    public ImageBlock() 
    {
        
    }
    public ImageBlock(ImageSource imageSource, string text)
    {
        CreateListViewTemplate(imageSource, text);
    }
    public ImageBlock(string text)
    {
        CreateListViewTemplate(null, text);
    }


    private void CreateListViewTemplate(ImageSource imageSource, string text)
    {
        _Image = new Image();
        _Image.Margin = new System.Windows.Thickness(2);
        if (imageSource != null) { _Image.Source = imageSource; }
        _Image.Width = 129; _Image.Height = 129;
        this.Height = _Image.Height + 25;


        _TextBlock = new TextBlock();
        _TextBlock.Margin = new System.Windows.Thickness(2);
        _TextBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8));//FFD8D8D8
        _TextBlock.Text = text;
        _TextBlock.Height = 25;


        this.Children.Add(_Image);
        this.Children.Add(_TextBlock);

    }



}
