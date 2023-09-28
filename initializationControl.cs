using System.Windows;

namespace initializationControl
{
 


    public class ControlSize
    {
        public double width { get; set; }
        public double height { get; set; }

    }

    public class ControlResize
    {
        

        public double DefaultWidth { get; set; }
        public double DefaultHeight { get; set; }
        public Thickness DefaultLocation { get; set; }
        

        public ControlResize() { }

        public ControlResize(double defaultWidth, double defaultHeight,Thickness thickness)
        {
            this.DefaultWidth = defaultWidth;
            this.DefaultHeight = defaultHeight;
            this.DefaultLocation = thickness;
        }


        public ControlResize(double defaultWidth, double defaultHeight)
        {
            this.DefaultWidth = defaultWidth;
            this.DefaultHeight = defaultHeight;
        }

        public ControlSize minus(double width, double height)
        {

            return new ControlSize()
            {
                width = width - DefaultWidth,
                height = height - DefaultHeight
            };

        }

        public double plusHeight(double height)
        {
            return DefaultHeight + height;
        }

        public double plusWidth(double width)
        {
            return DefaultWidth + width;
        }

    }
}



