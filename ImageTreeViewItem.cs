using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ImageTreeViewItem : TreeViewItem
{
	Image _Image = null;
	ImageSource _ImageSource = null;
	TextBlock _textBlock = null;
	string _FullName = string.Empty;
	
	public string FullName
	{
		get { return _FullName; }
		set { _FullName = value;
			this.Tag = _FullName;
		}
	}

	public string Text
	{
		get { return _textBlock.Text; }
		set { _textBlock.Text = value;}
	}

	public Image image
	{
		get { return _Image; }
		//set { _Image.Source = new BitmapImage(value); }
	}

	public ImageSource imageSource
	{
		get { return _ImageSource; }
		set { 
			_ImageSource = value;
			_Image.Source = _ImageSource;
		}
	}


	

    public ImageTreeViewItem()
	{
		CreateTreeViewItemTemplate();

    }
    

    public static ImageTreeViewItem createImageTreeViewItem(ImageTreeViewItem obj)
    {
        ImageTreeViewItem item = new ImageTreeViewItem()
        {
            imageSource = obj.imageSource,
            Text = obj.Text,
            FullName = obj.FullName,
        };

        return item;
    }

    public static ImageTreeViewItem createImageTreeViewItem(string Text, string FullName)
    {
        return createImageTreeViewItem(null, Text, FullName);
    }

    public static ImageTreeViewItem createImageTreeViewItem(ImageSource ImageSource,string Text, string FullName)
	{
        ImageTreeViewItem item = new ImageTreeViewItem()
        {
            imageSource = ImageSource,
            Text = Text,
            FullName = FullName,
        };

		return item;
    }


    private void copy(ref TreeViewItem source, ref TreeViewItem destination)
    {
        destination.Header = source.Header;

    }

    private void CreateTreeViewItemTemplate()
	{
		StackPanel stack = new StackPanel();
		stack.Orientation = Orientation.Horizontal;

		_Image = new Image();
		_Image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
		_Image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
		_Image.Margin = new System.Windows.Thickness(2);

		_textBlock = new TextBlock();
		_textBlock.Margin = new System.Windows.Thickness(2);
		_textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        _textBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8));//FFD8D8D8

        stack.Children.Add(_Image);
        stack.Children.Add(_textBlock);

		
		Header = stack;

	}
}
