namespace initializationControl
{
    public class window
    {

    }


    public class ControlSize
    {
        public double width { get; init; }
        public double height { get; init; }

    }

    public class ControlResize
    {
        private double default_width { get; }
        private double default_height { get; }

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

    }
}



