﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NotSoBoring.Domain.Utils
{
    public static class LocationUtils
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = DegToRad(lat2 - lat1);  // deg2rad below
            var dLon = DegToRad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double DegToRad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static string DistanceToString(double distance)
        {
            if (distance < 1)
                return "Less than 1km";
            else return $"Approximately {distance}km";
        }
    }
}
