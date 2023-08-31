namespace initializationControl
{
 


    public class ControlSize
    {
        public double width { get; set; }
        public double height { get; set; }

    }

    public class ControlResize
    {
        public double default_width { get; set; }
        public double default_height { get; set; }

        public ControlResize() { }

        public ControlResize(double default_width, double default_height)
        {
            this.default_width = default_width;
            this.default_height = default_height;
        }

        public ControlSize minus(double _width, double _height)
        {

            return new ControlSize()
            {
                width = _width - default_width,
                height = _height - default_height
            };

        }

        public double plusHeight(double _height)
        {
            return default_height + _height;
        }

        public double plusWidth(double _width)
        {
            return default_width + _width;
        }

    }
}



