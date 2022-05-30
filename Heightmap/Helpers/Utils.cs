using Heightmap.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Heightmap.Helpers
{
    internal class Utils
    {
        public static int StrToInt(string s)
        {           
            if (int.TryParse(s, out int number))
            {
                return number;
            }
            
            throw new Exception("Expected int value, got : \"" + s + "\" ");            
        }

        public static float StrToFloat(string s)
        {       
            if (float.TryParse(s, out float number))
            {
                return number;
            }            

            throw new Exception("Expected float value, got : \"" + s + "\" ");
        }

        public static bool CompareStr(string actual, string expected)
        {
            if (expected == actual.ToLower())
            {
                return true;
            }

            throw new Exception("Expected String: \"" + expected + "\", actual : {1}\"" + actual + "\"");
        }

        public static double CalculateDistance(Point point1, Point point2)
        {
            var dx = point1.X - point2.X;
            var dy = point1.Y - point2.Y;

            var distance = Math.Sqrt((Math.Pow(dx, 2) + Math.Pow(dy, 2)));

            return distance;
        }

        public static List<Coordinate> GetMaxValue(List<Coordinate> coordsInCircle, bool min)
        {
            if (min)
            {
                return coordsInCircle.OrderBy(x => x.Value).Take(10).ToList();
            }
            
            return coordsInCircle.OrderByDescending(x => x.Value).Take(10).ToList();
        }
    }
}
