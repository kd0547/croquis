using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public class ImageTreeViewItemFactory
{
    public RoutedEventHandler _Expanded;
    public RoutedEventHandler _TargetExpanded;
    public MouseButtonEventHandler _PreviewMouseDoubleClick;
    public MouseButtonEventHandler _MouseDoubleClick;
    public MouseButtonEventHandler _PreviewMouseDown;
    public MouseButtonEventHandler _PreviewMouseLeftButtonDown;

    //마우스 오른쪽 클릭 이벤트 
    public MouseButtonEventHandler _MouseRightButtonDown;

    
    public MouseButtonEventHandler _BookMarKImageLeftButtonDown;
    


    public ImageTreeViewItemFactory(FactoryBuilder builder)
    {
        this._Expanded = builder._Expanded;
        this._TargetExpanded = builder._TargetExpanded;
        this._PreviewMouseDoubleClick = builder._PreviewMouseDoubleClick;
        this._MouseDoubleClick = builder._MouseDoubleClick;
        this._PreviewMouseDown = builder._PreviewMouseDown;


        this._PreviewMouseLeftButtonDown = builder._PreviewMouseLeftButtonDown;
        this._MouseRightButtonDown = builder._MouseRightButtonDown;

        this._BookMarKImageLeftButtonDown = builder._BookMarKImageLeftButtonDown;
    }

    public class FactoryBuilder
    {
        public RoutedEventHandler _Expanded;
        public RoutedEventHandler _TargetExpanded;
        public MouseButtonEventHandler _PreviewMouseDoubleClick;
        public MouseButtonEventHandler _MouseDoubleClick;
        public MouseButtonEventHandler _PreviewMouseDown;

        public MouseButtonEventHandler _PreviewMouseLeftButtonDown;
        public MouseButtonEventHandler _MouseRightButtonDown;

        public MouseButtonEventHandler _BookMarKImageLeftButtonDown;

        public FactoryBuilder SetTargetExpanded(RoutedEventHandler TargetExpanded)
        {
            _TargetExpanded = TargetExpanded;
            return this;
        }

        public FactoryBuilder SetExpanded(RoutedEventHandler handler)
        {
            _Expanded = handler;
            return this;
        }

        public FactoryBuilder SetPreviewMouseDoubleClick(MouseButtonEventHandler handler)
        {
            _PreviewMouseDoubleClick = handler;
            return this;
        }

        public FactoryBuilder SetMouseDoubleClick(MouseButtonEventHandler handler)
        {
            _MouseDoubleClick = handler;
            return this;
        }

        public FactoryBuilder SetPreviewMouseDown(MouseButtonEventHandler handler)
        {
            _PreviewMouseDown = handler;
            return this;
        }

        public FactoryBuilder SetPreviewMouseLeftButtonDown(MouseButtonEventHandler handler)
        {
            _PreviewMouseLeftButtonDown = handler;
            return this;
        }

        public FactoryBuilder SetMouseRightButtonDown(MouseButtonEventHandler handler)
        {
            _MouseRightButtonDown = handler;
            return this;
        }
        public FactoryBuilder SetBookMarKImageLeftButtonDown(MouseButtonEventHandler handler)
        {
            _BookMarKImageLeftButtonDown = handler;
            return this;
        }

        public ImageTreeViewItemFactory Build()
        {
            return new ImageTreeViewItemFactory(this);
        }
    }

    public ImageTreeViewItem CreateLocalDrivesItem(ImageSource ImageSource, string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageSource(ImageSource)
            .SetText(Text)
            //이벤트
            .SetExpandedEvent(_Expanded)
            .build ();
    }

    public ImageTreeViewItem CreateSubDirectories(ImageSource ImageSource, string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageSource(ImageSource)
            
            .SetText(Text)
            .SetImageName(Text)
            //파일 확장 
            .SetExpandedEvent(_Expanded)
            //미리보기 이벤트 
            .SetMouseDoubleClickEvent(_PreviewMouseDoubleClick)

            .SetPreviewMouseDownEvent(_PreviewMouseDown)
            .build();
    }

    public ImageTreeViewItem CreateSubDirectories(ImageSource ImageSource, string Text, string FullName, ImageSource BookMarkImageSource)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageSource(ImageSource)
            .SetText(Text)
            .SetBookMarkImageSource(BookMarkImageSource)
            .SetImageName(Text)
            //이벤트
            .SetExpandedEvent(_Expanded)
            //미리보기 이벤트 
            .SetPreviewMouseDoubleClickEvent(_PreviewMouseDoubleClick)

            .SetPreviewMouseDownEvent(_PreviewMouseDown)

            .SetBookMarKImageLeftButtonDownEvent(_BookMarKImageLeftButtonDown)
            .build();
    }

    public ImageTreeViewItem CreateTargetGetDirectories(ImageSource ImageSource, string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageSource(ImageSource)
            .SetText(Text)
            .SetImageName(Text)
            //이벤트
            .SetExpandedEvent(_TargetExpanded)
            //미리보기 이벤트 
            .SetMouseDoubleClickEvent(_PreviewMouseDoubleClick)
            .SetMouseRightButtonDownEvent(_MouseRightButtonDown)
            .build();
    }

    public ImageTreeViewItem CreateTargetGetDirectories(ImageSource ImageSource, string Text, string FullName, ImageSource BookMarkImageSource)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageSource(ImageSource)
            .SetText(Text)
            .SetImageName(Text)
            .SetBookMarkImageSource(BookMarkImageSource)
            //이벤트
            .SetExpandedEvent(_TargetExpanded)
            //미리보기 이벤트 
            .SetMouseDoubleClickEvent(_PreviewMouseDoubleClick)
            .SetMouseRightButtonDownEvent(_MouseRightButtonDown)
            .build();
    }


    public ImageTreeViewItem CreateTargetGetFile(string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .SetFullName(FullName)
            .SetImageName(Text)
            .SetText(Text)
            //이벤트
            .SetPreviewMouseLeftButtonDownEvent(_PreviewMouseLeftButtonDown)
            //메뉴 확장
            .SetMouseRightButtonDownEvent(_MouseRightButtonDown)
            .build();
    }

}