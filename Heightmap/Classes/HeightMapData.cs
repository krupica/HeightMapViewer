using Heightmap.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Heightmap.Classes
{
    public class HeightMapData
    {
        public int Ncols { get; set; }

        public int Nrows { get; set; }

        public float Xllcorner { get; set; }

        public float Yllcorner { get; set; }

        public float Cellsize { get; set; }

        public List<Coordinate> Data { get; set; }       

        public float[] GetNormalizedArray()
        {
            var result = Data.Select(x => (float)(x.Value));
            var dataMax = result.Max();
            var dataMin = result.Min();
            var len = result.Count();
            var range = dataMax - dataMin;

            if (dataMin == dataMax)
            {
                return result.Select(x => x / len).ToArray();
            }            
            
            return result.Select(x => (x - dataMin) / range).ToArray();         
        }

        public List<Coordinate> GetCoordsInCircle(Point center, double radius) 
        {
            var coordsInCircle = new List<Coordinate>();

            foreach (var coord in Data)
            {
                if (Utils.CalculateDistance(center, coord.Location) <= radius) 
                {
                    coordsInCircle.Add(coord);
                }                    
            }

            return coordsInCircle;
        }

        public int GetValueAt(Point location) 
        {
            var result = Data.Find(x => x.Location == location);

            if (result == null)
            { 
                return -1;
            }

            return result.Value;
        }
    }
}
