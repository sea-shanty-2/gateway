using System;

namespace Gateway.Models
{
    public class Location
    {
        private double? longitude;
        public double? Longitude
        {
            get { return longitude; }
            set
            {
                if (value < -180 || value > 180)
                {
                    throw new ArgumentOutOfRangeException(
                        "Longitude",
                        value,
                        "The valid range for longitude is -180 to 180"
                    );
                }
                else
                {

                    longitude = value;
                }
            }
        }

        private double? latitude;
        public double? Latitude
        {
            get { return latitude; }
            set
            {
                if (value < -90 || value > 90)
                {
                    throw new ArgumentOutOfRangeException(
                        "Latitude",
                        value,
                        "The valid range for latitude is -90 to 90"
                    );
                }
                else
                {
                    latitude = value;
                }
            }
        }
    }

}