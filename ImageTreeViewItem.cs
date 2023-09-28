using croquis;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
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

    Image _bookMarkImage = null;
    ImageSource _bookMarkImageSource = null;

    ImageCache imageCache = null;

    bool _isBookMarkSeleted = false;


    public string _ImageName = string.Empty;
	public string _FullName = string.Empty;

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

	public Image Image
	{
		get { return _Image; }
	    set { _Image = value; }
	}

	public ImageSource imageSource
	{
		get { return _ImageSource; }
		set { 
			_ImageSource = value;
			_Image.Source = _ImageSource;
		}
	}

    public string ImageName
    {
        set { _ImageName = value; }
        get { return _ImageName; }
    }

    public Image BookMarkImage
    {
        get { return _bookMarkImage; }
        set { _bookMarkImage = value;}
    }

    public ImageSource BookMarkImageSource
    {
        get {  return _bookMarkImageSource;}
        set
        {
            _bookMarkImageSource = value;
            _bookMarkImage.Source = _bookMarkImageSource;
        }
    }

    public ImageCache ImageCache
    {
        get; set;
    }

    public bool IsBookMarkSelected
    {
        get;set;
    }

    public ImageTreeViewItem(ImageTreeViewItemBuilder builder)
    {
        CreateTreeViewItemTemplate();


        imageSource = builder._ImageSource;
        FullName = builder._FullName; 
        
        if(builder._BookMarkSource != null) { this.BookMarkImageSource = builder._BookMarkSource; }
        if(builder._BookMarkImage != null) { this.BookMarkImage = builder._BookMarkImage; }

        if(builder._Image != null) { Image = builder._Image; }

        if(builder._textBlock != null) { _textBlock = builder._textBlock; }
        Text = builder._Text;

        if(builder._ImageName != null) { _ImageName = builder._ImageName; }

        //이벤트 등록 
        if(builder._Expanded != null) { this.Expanded += builder._Expanded; }
        if(builder._PreviewMouseDoubleClick != null) { this.PreviewMouseDoubleClick += builder._PreviewMouseDoubleClick; }
        if(builder._MouseDoubleClick != null) { this.MouseDoubleClick += builder._MouseDoubleClick; }
        if(builder._PreviewMouseDown != null) { this.PreviewMouseDown += builder._PreviewMouseDown; }



        if(builder._PreviewMouseLeftButtonDown != null) { this.PreviewMouseLeftButtonDown += builder._PreviewMouseLeftButtonDown; }
        if(builder._MouseRightButtonDown != null) { this.MouseRightButtonDown += builder._MouseRightButtonDown; }

        if(builder._BookMarKImageLeftButtonDown != null)
        {
            this._bookMarkImage.MouseDown += builder._BookMarKImageLeftButtonDown;
        }
    }

    public class ImageTreeViewItemBuilder
	{
        public Image _Image = null;
        public ImageSource _ImageSource = null;
        public TextBlock _textBlock = null;
        public Image _BookMarkImage = null;
        public ImageSource _BookMarkSource = null;

        public string _ImageName = string.Empty;

        public string _FullName = string.Empty;
        public string _Text = string.Empty;

		//이벤트 등록
        public RoutedEventHandler _Expanded;
        public MouseButtonEventHandler _PreviewMouseDoubleClick;
        public MouseButtonEventHandler _MouseDoubleClick;
        public MouseButtonEventHandler _PreviewMouseDown;
        
        public MouseButtonEventHandler _PreviewMouseLeftButtonDown;
        public MouseButtonEventHandler _MouseRightButtonDown;

        public MouseButtonEventHandler _BookMarKImageLeftButtonDown;
        public ImageTreeViewItemBuilder()
        {        
        }

        public ImageTreeViewItemBuilder SetImageName(string imageName)
        {
            _ImageName = imageName;
            return this;
        }

        public ImageTreeViewItemBuilder SetText(string Text)
        {
            _Text = Text;
            return this;
        }

        public ImageTreeViewItemBuilder SetImage(Image image)
        {
            _Image = image;
            return this;
        }

        public ImageTreeViewItemBuilder SetImageSource(ImageSource imageSource)
        {
            _ImageSource = imageSource;
            return this;
        }

        public ImageTreeViewItemBuilder SetBookMarkImage(Image Image)
        {
            _BookMarkImage = Image;
            return this;
        }

        public ImageTreeViewItemBuilder SetBookMarkImageSource(ImageSource Image)
        {
            _BookMarkSource = Image;
            return this;
        }

        public ImageTreeViewItemBuilder SetTextBlock(TextBlock textBlock)
        {
            _textBlock = textBlock;
            return this;
        }

        public ImageTreeViewItemBuilder SetFullName(string fullName)
        {
            _FullName = fullName;
            return this;
        }

        public ImageTreeViewItemBuilder SetExpandedEvent(RoutedEventHandler handler)
        {
            _Expanded = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetPreviewMouseDoubleClickEvent(MouseButtonEventHandler handler)
        {
            _PreviewMouseDoubleClick = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetMouseDoubleClickEvent(MouseButtonEventHandler handler)
        {
            _MouseDoubleClick = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetPreviewMouseDownEvent(MouseButtonEventHandler handler)
        {
            _PreviewMouseDown = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetPreviewMouseLeftButtonDownEvent(MouseButtonEventHandler handler)
        {
            _PreviewMouseLeftButtonDown = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetMouseRightButtonDownEvent(MouseButtonEventHandler handler)
        {
            _MouseRightButtonDown = handler;
            return this;
        }

        public ImageTreeViewItemBuilder SetBookMarKImageLeftButtonDownEvent(MouseButtonEventHandler handler)
        {
            _BookMarKImageLeftButtonDown = handler;
            return this;
        }

        public ImageTreeViewItem build()
		{
			return new ImageTreeViewItem(this);
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
        _textBlock.FontSize = 14;
        _textBlock.FontWeight = FontWeights.Medium;
        _textBlock.FontFamily = new FontFamily("나눔 고딕");
        
		_textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        _textBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8));//FFD8D8D8

        //테스트
        _bookMarkImage = new Image();
        _bookMarkImage.Margin = new System.Windows.Thickness(2);
      

        stack.Children.Add(_Image);
        stack.Children.Add(_textBlock);
        stack.Children.Add(_bookMarkImage);
		
		Header = stack;

	}

    
}
