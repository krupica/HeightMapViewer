using System.Windows;

namespace Heightmap.Classes
{
    /// <summary>
    /// Souřadnice a hodnota.
    /// </summary>
    public class Coordinate
    {
        // x,y souřadnice
        public Point Location { get; set; }
        
        //výšková hodnota
        public int Value { get; set; }
       
    }
}
