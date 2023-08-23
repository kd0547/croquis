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
    public MouseButtonEventHandler _PreviewMouseRightButtonDown;




    public ImageTreeViewItemFactory(FactoryBuilder builder)
    {
        _Expanded = builder._Expanded;
        _TargetExpanded = builder._TargetExpanded;
        _PreviewMouseDoubleClick = builder._PreviewMouseDoubleClick;
        _MouseDoubleClick = builder._MouseDoubleClick;
        _PreviewMouseDown = builder._PreviewMouseDown;
        _PreviewMouseLeftButtonDown = builder._PreviewMouseLeftButtonDown;
        _PreviewMouseRightButtonDown = builder._PreviewMouseRightButtonDown;
    }

    public class FactoryBuilder
    {
        public RoutedEventHandler _Expanded;
        public RoutedEventHandler _TargetExpanded;
        public MouseButtonEventHandler _PreviewMouseDoubleClick;
        public MouseButtonEventHandler _MouseDoubleClick;
        public MouseButtonEventHandler _PreviewMouseDown;
        public MouseButtonEventHandler _PreviewMouseLeftButtonDown;
        public MouseButtonEventHandler _PreviewMouseRightButtonDown;

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

        public FactoryBuilder SetPreviewMouseRightButtonDown(MouseButtonEventHandler handler)
        {
            _PreviewMouseRightButtonDown = handler;
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
            .WithFullName(FullName)
            .WithImageSource(ImageSource)
            .WithText(Text)
            //이벤트
            .WithExpandedEvent(_Expanded)
            .build ();
    }

    public ImageTreeViewItem CreateSubDirectories(ImageSource ImageSource, string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .WithFullName(FullName)
            .WithImageSource(ImageSource)
            .WithText(Text)
            //이벤트
            .WithExpandedEvent(_Expanded)
            .WithPreviewMouseDoubleClickEvent(_PreviewMouseDoubleClick)
            .WithPreviewMouseDownEvent(_PreviewMouseDown)
            .build();
    }

    public ImageTreeViewItem CreateTargetGetDirectories(ImageSource ImageSource, string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .WithFullName(FullName)
            .WithImageSource(ImageSource)
            .WithText(Text)
            //이벤트
            .WithExpandedEvent(_TargetExpanded)
            .WithMouseDoubleClickEvent(_PreviewMouseDoubleClick)
            .WithPreviewMouseRightButtonDownEvent(_PreviewMouseRightButtonDown)
            .build();
    }

    public ImageTreeViewItem CreateTargetGetFile(string Text, string FullName)
    {
        return new ImageTreeViewItem.ImageTreeViewItemBuilder()
            .WithFullName(FullName)
            
            .WithText(Text)
            //이벤트
            .WithPreviewMouseLeftButtonDownEvent(_PreviewMouseLeftButtonDown)
            .WithPreviewMouseRightButtonDownEvent(_PreviewMouseRightButtonDown)
            .build();
    }

}