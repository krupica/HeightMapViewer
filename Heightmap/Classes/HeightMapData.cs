using Heightmap.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Heightmap.Classes
{
    public class HeightMapData
    {
        // počet sloupců
        public int Ncols { get; set; }

        // počet řádků
        public int Nrows { get; set; }

        // x souřadnice levého dolního rohu
        public float Xllcorner { get; set; }

        // y souřadnice levého dolního rohu
        public float Yllcorner { get; set; }

        // velikost buňky
        public float Cellsize { get; set; }

        // data výškové mapy
        public List<Coordinate> Data { get; set; }       

        /// <summary>
        /// Převedení na Pole hodnot 0-1 udávajících velikost.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Získání hodnot v kruhu výběru.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Získání hodnoty na dané pozici.
        /// Při neúspěchu vrací -1.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
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
