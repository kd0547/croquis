using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public class ImageTreeViewItemFactory
{
    public event RoutedEventHandler _Expanded;
    public event MouseButtonEventHandler _PreviewMouseDoubleClick;
    public event MouseButtonEventHandler _MouseDoubleClick;
    public event MouseButtonEventHandler _PreviewMouseDown;
    public event MouseButtonEventHandler _PreviewMouseLeftButtonDown;
    public event MouseButtonEventHandler _PreviewMouseRightButtonDown;
    public ImageTreeViewItemFactory()
    {

    }

    

}
//빌더패턴으로 구현 진행중
public class ImageTreeViewItemBuilder
{
    public Image _Image = null;
    public ImageSource _ImageSource = null;
    public TextBlock _textBlock = null;
    public string _FullName = string.Empty;
    public string _Text = string.Empty;

    

    public ImageTreeViewItemBuilder SetImageSource(ImageSource imageSource)
    {
        _ImageSource = imageSource;
        return this;
    }
    public ImageTreeViewItemBuilder SetFullName(string FullName)
    {
        _FullName = FullName;
        return this;
    }

    public ImageTreeViewItemBuilder SetText(string Text)
    {
        _Text = Text;
        return this;
    }

    public ImageTreeViewItemBuilder SetExpanded(RoutedEventHandler Expanded)
    {
        _Expanded = Expanded;
        return this;
    }

    public ImageTreeViewItemBuilder SetPreviewMouseDoubleClick(MouseButtonEventHandler PreviewMouseDoubleClick)
    {
        _PreviewMouseDoubleClick = PreviewMouseDoubleClick;
        return this;
    }



    public ImageTreeViewItemBuilder SetMouseDoubleClick(MouseButtonEventHandler MouseDoubleClick)
    {
        _MouseDoubleClick = MouseDoubleClick;
        return this;
    }
    public ImageTreeViewItemBuilder SetPreviewMouseDown(MouseButtonEventHandler PreviewMouseDown)
    {
        _PreviewMouseDown = PreviewMouseDown;
        return this;
    }
    public ImageTreeViewItemBuilder SetPreviewMouseLeftButtonDown(MouseButtonEventHandler PreviewMouseLeftButtonDown)
    {
        _PreviewMouseLeftButtonDown = PreviewMouseLeftButtonDown;
        return this;
    }
    public ImageTreeViewItemBuilder SetPreviewMouseRightButtonDown(MouseButtonEventHandler PreviewMouseRightButtonDown)
    {
        _PreviewMouseRightButtonDown = PreviewMouseRightButtonDown;
        return this;
    }
    
    public ImageTreeViewItem build()
    {
        return new ImageTreeViewItem(this);
    }
}

